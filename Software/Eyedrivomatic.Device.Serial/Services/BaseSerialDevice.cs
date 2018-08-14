using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Serial.Communications;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using NullGuard;
using Prism.Events;
using Prism.Mvvm;

namespace Eyedrivomatic.Device.Serial.Services
{
    public abstract class BaseSerialDevice : BindableBase, IDevice
    {
        private readonly IConnectionEnumerationService _connectionEnumerationService;
        private readonly IDeviceConnectionFactory _connectionFactory;
        private readonly IFirmwareUpdateService _firmwareUpdateService;
        private IDeviceConnection _connection;
        private ConnectionState _connectionState;

        private readonly IList<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>> _messageProcessors;
        private readonly IEventAggregator _eventAggregator;
        private readonly string _variant;

        private IDisposable _connectionDatastream;

        protected readonly IDeviceCommands Commands;

        public static uint SwitchCount = 4;
        public static Version MinFirmwareVersion = new Version(2, 1, 0);

        public BaseSerialDevice(
            IDeviceCommands commandFactory,
            [ImportMany] IEnumerable<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>> messageProcessors,
            IDeviceStatus deviceStatus,
            IDeviceSettings deviceSettings,
            IConnectionEnumerationService connectionEnumerationService,
            IDeviceConnectionFactory connectionFactory,
            [Import("FirmwareUpdateWithConfirmation")] IFirmwareUpdateService firmwareUpdateService,
            IEventAggregator eventAggregator,
            [Import("DeviceVariant")] string variant)
        {
            Commands = commandFactory;
            _messageProcessors = new List<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>>(messageProcessors);
            DeviceStatus = deviceStatus;
            DeviceSettings = deviceSettings;
            _connectionEnumerationService = connectionEnumerationService;
            _connectionFactory = connectionFactory;
            _firmwareUpdateService = firmwareUpdateService;
            _eventAggregator = eventAggregator;
            _variant = variant;

            deviceStatus.PropertyChanged += OnDeviceStatusChanged;
        }

        public bool DeviceReady => Connection?.State == ConnectionState.Connected && DeviceStatus.IsKnown;

        private void OnDeviceStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceStatus.IsKnown))
            {
                RaisePropertyChanged(nameof(DeviceReady));
            }

            RaisePropertyChanged(nameof(DeviceStatus));
        }

        private void AttachToDataStream(IObservable<char> observable)
        {
            var dataStream = observable.Publish();
            var sendStream = new AnonymousObserver<string>(message => Connection?.SendMessage(message));

            foreach (var lazy in _messageProcessors)
            {
                Log.Debug(this, $"Attaching datastream to [{lazy.Metadata.Name}].");
                lazy.Value.Attach(dataStream, sendStream);
            }

            _connectionDatastream = dataStream.Connect();

            Commands.GetConfiguration(SettingNames.All);
            Commands.GetStatus();
        }

        private void OnConnectionStateChanged(object sender, EventArgs eventArgs)
        {
            ConnectionState = Connection?.State ?? ConnectionState.Disconnected;
        }

        #region Connection

        public IList<DeviceDescriptor> GetAvailableDevices(bool includeAllSerialDevices)
        {
            return _connectionEnumerationService.GetAvailableDevices(includeAllSerialDevices);
        }

        public async Task AutoConnectAsync(bool autoUpdateFirmware, CancellationToken cancellationToken)
        {
            try
            {
                ConnectionState = ConnectionState.Connecting;

                Connection = null;
                _eventAggregator.GetEvent<DeviceConnectionEvent>().Publish(ConnectionState.Connecting);

                var connection = await _connectionEnumerationService.DetectDeviceAsync(MinFirmwareVersion, cancellationToken);
                if (connection == null)
                {
                    Log.Error(this, "Device not found!");
                    throw new ConnectionFailedException(Strings.DeviceConnection_Error_Auto_NotFound);
                }

                await CheckFirmwareVersion(connection, autoUpdateFirmware);
                Connection = connection;
            }
            catch (Exception)
            {
                _eventAggregator.GetEvent<DeviceConnectionEvent>().Publish(ConnectionState.Error);
                ConnectionState = ConnectionState.Disconnected;
                throw;
            }
        }

        public async Task ConnectAsync(string connectionString, bool autoUpdateFirmware, CancellationToken cancellationToken)
        {
            Connection = null;
            ConnectionState = ConnectionState.Connecting;
            try
            {
                var device = GetAvailableDevices(true).FirstOrDefault(d => StringComparer.OrdinalIgnoreCase.Compare(d.ConnectionString, connectionString) == 0);
                if (device == null)
                {
                    Log.Error(this, $"Device [{connectionString}] not found!");
                    throw new ConnectionFailedException(string.Format(Strings.DeviceConnection_Error_Manual_NotFound, connectionString));
                }

                var connection = _connectionFactory.CreateConnection(device);
                await connection.ConnectAsync(cancellationToken);

                if (connection.State != ConnectionState.Connected)
                {
                    if (GetAvailableDevices(false)
                        .All(d => StringComparer.OrdinalIgnoreCase.Compare(d.ConnectionString, connectionString) != 0))
                    {
                        Log.Error(this, $"Connection to device [{connectionString}] failed!");

                        throw new ConnectionFailedException(
                            string.Format(Strings.DeviceConnection_Error_Manual, connectionString));
                    }
                }

                await CheckFirmwareVersion(connection, autoUpdateFirmware);
                Connection = connection;
            }
            catch (Exception)
            {
                _eventAggregator.GetEvent<DeviceConnectionEvent>().Publish(ConnectionState.Error);
                throw;
            }
        }

        private async Task CheckFirmwareVersion(IDeviceConnection connection, bool autoUpdateFirmware)
        {
            var latestVersion = _firmwareUpdateService.GetLatestVersion(connection.VersionInfo.Model, _variant);

            //Required update.
            if (!string.IsNullOrEmpty(_variant) && string.CompareOrdinal(connection.VersionInfo.Variant, _variant) != 0)
            {
                if (latestVersion == null || latestVersion.Version < MinFirmwareVersion)
                {
                    Log.Error(this, $"A device was detected with firmware for [{(string.IsNullOrEmpty(connection.VersionInfo.Variant) ? "standard" : connection.VersionInfo.Variant)}] hardware, however firmware for [{_variant}] is required. Unfortunately the firmware file cannot be found.");
                    throw new ConnectionFailedException(Strings.DeviceConnection_MinFirmwareNotAvailable);
                }

                if (!autoUpdateFirmware || !await _firmwareUpdateService.UpdateFirmwareAsync(connection, latestVersion, true))
                    throw new ConnectionFailedException(string.Format(Strings.DeviceConnection_Error_FirmwareCheck, connection.ConnectionString));
            }

            if (connection.VersionInfo.Version < MinFirmwareVersion)
            {
                if (latestVersion == null || latestVersion.Version < MinFirmwareVersion)
                {
                    Log.Error(this, $"A device was detected with firmware version [{connection.VersionInfo.Version}], However a minimum version [{MinFirmwareVersion}] is required. However the firmware file cannot be found.");
                    throw new ConnectionFailedException(Strings.DeviceConnection_MinFirmwareNotAvailable);
                }

                if (!autoUpdateFirmware || !await _firmwareUpdateService.UpdateFirmwareAsync(connection, latestVersion, true))
                    throw new ConnectionFailedException(string.Format(Strings.DeviceConnection_Error_FirmwareCheck, connection.ConnectionString));
            }

            if (autoUpdateFirmware && latestVersion != null && latestVersion.Version > connection.VersionInfo.Version)
            {
                await _firmwareUpdateService.UpdateFirmwareAsync(connection, latestVersion, false);
            }
        }

        [Export]
        [AllowNull]
        public IDeviceConnection Connection
        {
            get => _connection;
            private set
            {
                if (value == _connection) return;

                _connectionDatastream?.Dispose();
                _connectionDatastream = null;

                if (_connection != null) _connection.ConnectionStateChanged -= OnConnectionStateChanged;
                _connection = value;

                if (_connection != null)
                {
                    _connection.ConnectionStateChanged += OnConnectionStateChanged;
                    _connection.DataStream.Subscribe(AttachToDataStream);
                }

                OnConnectionStateChanged(this, EventArgs.Empty);
            }
        }

        #endregion Connection

        public IDeviceSettings DeviceSettings { get; }

        public IDeviceStatus DeviceStatus { get; }

        public virtual ConnectionState ConnectionState
        {
            get => _connectionState;
            protected set
            {
                if (!SetProperty(ref _connectionState, value)) return;

                RaisePropertyChanged(nameof(DeviceStatus));
                _eventAggregator.GetEvent<DeviceConnectionEvent>().Publish(value);
            }
        }

        public Task<bool> Move(Point point, TimeSpan duration)
        {
            Log.Info(this, $"Move Point[{point}].");
            return Commands.Move(point, duration);
        }

        public virtual Task<bool> Go(Vector vector, TimeSpan duration)
        {
            Log.Info(this, $"Go Vector:[{vector}].");
            return Commands.Go(vector, duration);
        }

        public virtual void Stop()
        {
            Log.Info(this, "Stop.");
            Commands.Stop();
        }

        public async Task CycleSwitchAsync(uint relay, uint repeat = 1, uint toggleDelayMs = 500, uint repeatDelayMs = 1000)
        {
            Log.Info(this, $"Cycling relay {relay} for {toggleDelayMs} ms, {repeat} times with a delay of {repeatDelayMs} ms.");
            try
            {
                for (int i = 0; i < repeat; i++)
                {
                    if (i > 0) await Task.Delay(TimeSpan.FromMilliseconds(repeatDelayMs));

                    Log.Info(this, $"Cycling relay {relay}.");
                    await Commands.ToggleSwitch(relay, TimeSpan.FromMilliseconds(toggleDelayMs));
                    await Task.Delay(TimeSpan.FromMilliseconds(220 + toggleDelayMs)); //the relay cycles over 200ms. Add a few ms to allow for communication.
                }
                Log.Info(this, $"Cycling relay {relay} done.");
            }
            catch (Exception e)
            {
                Log.Error(this, $"Failed to toggle relay command - [{e}]");
            }
        }

        #region IDisposable
        public void Dispose()
        {
            _connection?.Dispose();
            _connectionDatastream?.Dispose();
            _connectionDatastream = null;
        }
        #endregion IDisposable
    }
}
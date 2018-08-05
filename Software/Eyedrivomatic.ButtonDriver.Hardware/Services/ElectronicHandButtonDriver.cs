//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Device.Models;
using Eyedrivomatic.Device;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using NullGuard;
using Prism.Events;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.Device.Services
{
    [Export(typeof(IButtonDriver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public class ElectronicHandButtonDriver : BindableBase, IButtonDriver
    {
        private readonly IDeviceEnumerationService _deviceEnumerationService;
        private readonly IElectronicHandConnectionFactory _connectionFactory;
        private readonly IFirmwareUpdateService _firmwareUpdateService;
        private readonly IBrainBoxCommands _commands;
        private readonly IList<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>> _messageProcessors;
        private readonly IEventAggregator _eventAggregator;
        private readonly string _variant;

        private IDisposable _connectionDatastream;

        public static uint AvailableRelays = 4;
        public static Version MinFirmwareVersion = new Version(2, 1, 0);

        [ImportingConstructor]
        internal ElectronicHandButtonDriver(
            IBrainBoxCommands commandFactory,
            [ImportMany] IEnumerable<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>> messageProcessors,
            IDeviceStatus deviceStatus,
            IDeviceSettings deviceSettings, 
            IDeviceEnumerationService deviceEnumerationService, 
            IElectronicHandConnectionFactory connectionFactory, 
            [Import("FirmwareUpdateWithConfirmation")] IFirmwareUpdateService firmwareUpdateService, 
            IEventAggregator eventAggregator,
            [Import("DeviceVariant")] string variant)
        {
            _commands = commandFactory;
            _messageProcessors = new List<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>>(messageProcessors);
            DeviceStatus = deviceStatus;
            DeviceSettings = deviceSettings;
            _deviceEnumerationService = deviceEnumerationService;
            _connectionFactory = connectionFactory;
            _firmwareUpdateService = firmwareUpdateService;
            _eventAggregator = eventAggregator;
            _variant = variant;

            deviceStatus.PropertyChanged += OnDeviceStatusChanged;
        }

        private void OnDeviceStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceStatus.IsKnown))
            {
                RaisePropertyChanged(nameof(DeviceReady));
            }

            RaisePropertyChanged(nameof(CurrentDirection));
            RaisePropertyChanged(nameof(ReadyState));
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

            _commands.GetConfiguration(SettingNames.All);
            _commands.GetStatus();
        }

        private void OnConnectionStateChanged(object sender, EventArgs eventArgs)
        {
            ConnectionState = Connection?.State ?? ConnectionState.Disconnected;
        }

        #region Connection

        public IList<DeviceDescriptor> GetAvailableDevices(bool includeAllSerialDevices)
        {
            return _deviceEnumerationService.GetAvailableDevices(includeAllSerialDevices);
        }

        public async Task AutoConnectAsync(bool autoUpdateFirmware, CancellationToken cancellationToken)
        {
            try
            {
                ConnectionState = ConnectionState.Connecting;

                Connection = null;
                _eventAggregator.GetEvent<DeviceConnectionEvent>().Publish(ConnectionState.Connecting);

                var connection = await _deviceEnumerationService.DetectDeviceAsync(MinFirmwareVersion, cancellationToken);
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

#region DeviceInfo
        public uint RelayCount => 3;
#endregion DeviceInfo

#region Status

        public IDeviceStatus DeviceStatus { get; }

        public ConnectionState ConnectionState
        {
            get => _connectionState;
            private set
            {
                if (!SetProperty(ref _connectionState, value)) return;

                RaisePropertyChanged(nameof(CurrentDirection));
                RaisePropertyChanged(nameof(DeviceReady));
                RaisePropertyChanged(nameof(ReadyState));
                RaisePropertyChanged(nameof(DeviceStatus));
                _eventAggregator.GetEvent<DeviceConnectionEvent>().Publish(value);
            }
        }

        public bool DeviceReady => Connection?.State == ConnectionState.Connected && DeviceStatus.IsKnown;

        public ReadyState ReadyState
        {
            get
            {
                if (!DeviceReady) return ReadyState.None;

                if (Profile.SafetyBypass) return ReadyState.Any;

                if (LastDirection == Direction.None || ContinueState != ContinueState.NotContinuedRecently) return ReadyState.Any;
                return ReadyState.Continue;
            }
        }

        public Direction CurrentDirection
        {
            get
            {
                if (!DeviceReady) return Direction.None;

                if (DeviceStatus.Vector.Speed < 0.1m) return Direction.None;
                return _lastDirection;
            }
        }

        public Direction LastDirection
        {
            get => _lastDirection;
            private set
            {
                SetProperty(ref _lastDirection, value);
                RaisePropertyChanged(nameof(ReadyState));
            }
        }

        public ContinueState ContinueState
        {
            get => _continueState;
            private set
            {
                SetProperty(ref _continueState, value);
                RaisePropertyChanged(nameof(ReadyState));
            }
        }

#endregion Status

#region Settings

        public IDeviceSettings DeviceSettings { get; }

#region Trim

        private Direction _lastDirection;
        private ContinueState _continueState;
        private Profile _profile;
        private int _nudge;
        private IDeviceConnection _connection;
        private ConnectionState _connectionState;

        #endregion Trim

        public Profile Profile
        {
            get => _profile;
            set
            {
                if (_profile != null) _profile.PropertyChanged -= ProfileOnPropertyChanged;
                SetProperty(ref _profile, value);
                if (_profile != null) _profile.PropertyChanged += ProfileOnPropertyChanged;
            }
        }

        private void ProfileOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            RaisePropertyChanged(nameof(Profile));
        }

#endregion Settings

#region Control
        public void Continue()
        {
            Log.Info(this, "Continue.");
            ContinueState = ContinueState.Continued;
        }

        public void Stop()
        {
            Log.Info(this, "Stop.");
            _commands.Stop();
            _nudge = 0;
            ContinueState = ContinueState.Continued;
        }

        public async Task Nudge(XDirection direction, TimeSpan duration)
        {
            Log.Info(this, $"Nudge {direction}.");

            if (LastDirection != Direction.Forward || CurrentDirection == Direction.None)
            {
                Log.Warn(this, "Nudge only available while moving forward.");
                return;
            }

            ContinueState = ContinueState.Continued;

            try
            {
                _nudge += direction == XDirection.Right ? Profile.CurrentSpeed.Nudge : -Profile.CurrentSpeed.Nudge;
                if (!await _commands.Move(new Point(_nudge, Profile.CurrentSpeed.YForward), duration))
                {
                    Log.Error(this, $"Failed to send nudge [{direction}] command.");
                }
            }
            catch (Exception e)
            {
                Log.Error(this, $"Failed to send nudge [{direction}] command - [{e}]");
            }
        }

        public bool CanMove(Direction direction)
        {
            return DeviceReady &&
                (Profile.SafetyBypass
                || ContinueState == ContinueState.Continued
                || LastDirection != direction);
        }

        public async Task Move(Direction direction, TimeSpan duration)
        {
            Log.Info(this, $"Move {direction}.");

            ContinueState = ContinueState.NotContinuedRecently;

            if (CurrentDirection != Direction.Forward && CurrentDirection != Direction.Backward) _nudge = 0;

            var speed = Profile.CurrentSpeed;

            var directionCommands = new Dictionary<Direction, Func<Task<bool>>>
                {
                    {Direction.Forward,       () => _commands.Move(new Point(_nudge, speed.YForward), duration)},
                    {Direction.ForwardRight,  () => _commands.Move(new Point(speed.XDiag, speed.YForwardDiag), duration)},
                    {Direction.Right,         () => _commands.Move(new Point(speed.X, 0), duration)},
                    {Direction.BackwardRight, () => _commands.Move(new Point(speed.XDiag,  -speed.YBackwardDiag), duration)},
                    {Direction.Backward,      () => _commands.Move(new Point(0, -speed.YBackward), duration)},
                    {Direction.BackwardLeft,  () => _commands.Move(new Point(-speed.XDiag, -speed.YBackwardDiag), duration)},
                    {Direction.Left,          () => _commands.Move(new Point(-speed.X, 0), duration)},
                    {Direction.ForwardLeft,   () => _commands.Move(new Point(-speed.XDiag, speed.YForwardDiag), duration)},
                };

            if (!directionCommands.ContainsKey(direction)) throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(Direction));

            try
            {
                LastDirection = direction;
                if (!await directionCommands[direction]())
                {
                    Log.Error(this, $"Failed to send move [{direction}] command.");
                }
            }
            catch (Exception e)
            {
                Log.Error(this, $"Failed to send move [{direction}] command - [{e}]");
            }
        }

        public async Task CycleRelayAsync(uint relay, uint repeat = 1, uint toggleDelayMs = 500, uint repeatDelayMs = 1000)
        {
            Log.Info(this, $"Cycling relay {relay} for {toggleDelayMs} ms, {repeat} times with a delay of {repeatDelayMs} ms.");
            try
            {
                for (int i = 0; i < repeat; i++)
                {
                    if (i > 0) await Task.Delay(TimeSpan.FromMilliseconds(repeatDelayMs));

                    Log.Info(this, $"Cycling relay {relay}.");
                    await _commands.ToggleRelay(relay, TimeSpan.FromMilliseconds(toggleDelayMs));
                    await Task.Delay(TimeSpan.FromMilliseconds(220 + toggleDelayMs)); //the relay cycles over 200ms. Add a few ms to allow for communication.
                }
                Log.Info(this, $"Cycling relay {relay} done.");
            }
            catch (Exception e)
            {
                Log.Error(this, $"Failed to toggle relay command - [{e}]");
            }
        }
#endregion Control

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

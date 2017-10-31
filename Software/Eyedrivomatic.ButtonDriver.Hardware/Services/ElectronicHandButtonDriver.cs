// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


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
using Eyedrivomatic.ButtonDriver.Hardware.Models;
using Eyedrivomatic.Hardware.Commands;
using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Hardware.Services;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using NullGuard;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    [Export(typeof(IButtonDriver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public class ElectronicHandButtonDriver : BindableBase, IButtonDriver
    {
        private readonly IDeviceEnumerationService _deviceEnumerationService;
        private readonly IElectronicHandConnectionFactory _connectionFactory;
        private readonly IFirmwareUpdateService _firmwareUpdateService;
        private readonly InteractionRequest<INotification> _connectionFailureNotification;
        private readonly IBrainBoxCommands _commands;
        private readonly IList<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>> _messageProcessors;

        private IDisposable _connectionDatastream;

        public static uint AvailableRelays = 3;
        public static Version MinFirmwareVersion = new Version(2, 0, 0);

        [ImportingConstructor]
        internal ElectronicHandButtonDriver(
            IBrainBoxCommands commandFactory,
            [ImportMany] IEnumerable<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>> messageProcessors,
            IDeviceStatus deviceStatus,
            IDeviceSettings deviceSettings, 
            IDeviceEnumerationService deviceEnumerationService, 
            IElectronicHandConnectionFactory connectionFactory, 
            [Import("FirmwareUpdateWithConfirmation")] IFirmwareUpdateService firmwareUpdateService,
            InteractionRequest<INotification> connectionFailureNotification )
        {
            _commands = commandFactory;
            _messageProcessors = new List<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>>(messageProcessors);
            DeviceStatus = deviceStatus;
            DeviceSettings = deviceSettings;
            _deviceEnumerationService = deviceEnumerationService;
            _connectionFactory = connectionFactory;
            _firmwareUpdateService = firmwareUpdateService;
            _connectionFailureNotification = connectionFailureNotification;

            deviceStatus.PropertyChanged += OnStatusChanged;
        }

        private void OnStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceStatus.IsKnown))
            {
                RaisePropertyChanged(nameof(HardwareReady));
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
            RaisePropertyChanged(nameof(CurrentDirection));
            RaisePropertyChanged(nameof(HardwareReady));
            RaisePropertyChanged(nameof(ReadyState));
            RaisePropertyChanged(nameof(DeviceStatus));
            RaisePropertyChanged(nameof(Connection));
        }

        #region Connection

        public IList<DeviceDescriptor> GetAvailableDevices(bool includeAllSerialDevices)
        {
            return _deviceEnumerationService.GetAvailableDevices(includeAllSerialDevices);
        }

        public async Task AutoConnectAsync(CancellationToken cancellationToken)
        {
            Connection = null;

            var connection = await _deviceEnumerationService.DetectDeviceAsync(MinFirmwareVersion, cancellationToken);
            if (connection == null)
            {
                Log.Error(this, "Device not found!");
                throw new ConnectionFailedException(Strings.DeviceConnection_Error_Auto_NotFound);
            }

            if (await CheckFirmwareVersion(connection)) Connection = connection;
        }

        public async Task ConnectAsync(string connectionString, CancellationToken cancellationToken)
        {
            Connection = null;
            var device = GetAvailableDevices(true).FirstOrDefault(d => d.ConnectionString == connectionString);
            if (device == null)
            {
                Log.Error(this, $"Device [{connectionString}] not found!");
                throw new ConnectionFailedException(string.Format(Strings.DeviceConnection_Error_Manual_NotFound, connectionString));
            }

            var connection = _connectionFactory.CreateConnection(device);
            await connection.ConnectAsync(cancellationToken);
            if (connection.State != ConnectionState.Connected)
            {
                throw new ConnectionFailedException(string.Format(Strings.DeviceConnection_Error_Manual, connectionString));
            }

            if (await CheckFirmwareVersion(connection)) Connection = connection;
        }

        private async Task<bool> CheckFirmwareVersion(IDeviceConnection connection)
        {
            try
            {
                var latestVersion = _firmwareUpdateService.GetLatestVersion();

                //Required update.
                if (connection.FirmwareVersion == null || connection.FirmwareVersion < MinFirmwareVersion)
                {
                    if (latestVersion == null || latestVersion < MinFirmwareVersion)
                    {
                        Log.Error(this, $"A device was detected with firmware version [{connection.FirmwareVersion?.ToString() ?? "NA"}, However a minimum version [{MinFirmwareVersion}] is required. However the firmware file cannot be found.");
                        throw new ConnectionFailedException(Strings.DeviceConnection_MinFirmwareNotAvailable);
                    }

                    await _firmwareUpdateService.UpdateFirmwareAsync(connection, latestVersion, true);
                    return true;
                }

                //Optional update.
                if (latestVersion != null && latestVersion > connection.FirmwareVersion)
                {
                    await _firmwareUpdateService.UpdateFirmwareAsync(connection, latestVersion, false);
                }

                return true;
            }
            catch (ConnectionFailedException cfe)
            {
                _connectionFailureNotification.Raise(
                    new Prism.Interactivity.InteractionRequest.Notification
                    {
                        Title = Strings.DeviceConnection_Error_Title,
                        Content = cfe.Message
                    });
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Firmware version check failed! [{ex}]");
                _connectionFailureNotification.Raise(
                    new Prism.Interactivity.InteractionRequest.Notification
                    {
                        Title = Strings.DeviceConnection_Error_Title,
                        Content = string.Format(Strings.DeviceConnection_Error_FirmwareCheck, connection.ConnectionString)
                    });
                return false;
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

        public bool HardwareReady => Connection?.State == ConnectionState.Connected && DeviceStatus.IsKnown;

        public ReadyState ReadyState
        {
            get
            {
                if (!HardwareReady) return ReadyState.None;

                if (Profile.SafetyBypass) return ReadyState.Any;

                if (LastDirection == Direction.None || ContinueState != ContinueState.NotContinuedRecently) return ReadyState.Any;
                return ReadyState.Continue;
            }
        }

        public Direction CurrentDirection
        {
            get
            {
                if (!HardwareReady) return Direction.None;

                if (DeviceStatus.YPosition < 0)
                {
                    return DeviceStatus.XPosition == 0
                        ? Direction.Backward
                        : DeviceStatus.XPosition > 0
                            ? Direction.BackwardRight
                            : Direction.BackwardLeft;
                }

                if (DeviceStatus.YPosition > 0)
                {
                    return DeviceStatus.XPosition == 0
                        ? Direction.Forward
                        : DeviceStatus.XPosition > 0
                            ? Direction.ForwardRight
                            : Direction.ForwardLeft;
                }

                return DeviceStatus.XPosition == 0
                    ? Direction.None
                    : DeviceStatus.XPosition > 0
                        ? Direction.Right
                        : Direction.Left;
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
                if (!await _commands.Move(_nudge, Profile.CurrentSpeed.YForward, duration))
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
            return HardwareReady &&
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
                    {Direction.Forward,       () => _commands.Move(_nudge, speed.YForward, duration)},
                    {Direction.ForwardRight,  () => _commands.Move(speed.XDiag, speed.YForwardDiag, duration)},
                    {Direction.Right,         () => _commands.Move(speed.X, 0, duration)},
                    {Direction.BackwardRight, () => _commands.Move(speed.XDiag,  -speed.YBackwardDiag, duration)},
                    {Direction.Backward,      () => _commands.Move(0, -speed.YBackward, duration)},
                    {Direction.BackwardLeft,  () => _commands.Move(-speed.XDiag, -speed.YBackwardDiag, duration)},
                    {Direction.Left,          () => _commands.Move(-speed.X, 0, duration)},
                    {Direction.ForwardLeft,   () => _commands.Move(-speed.XDiag, speed.YForwardDiag, duration)},
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

        public async Task CycleRelayAsync(uint relay, uint repeat = 1, uint repeatDelayMs = 0)
        {
            Log.Info(this, $"Cycling relay {relay} {repeat} times with a delay of {repeatDelayMs}.");
            try
            {
                for (int i = 0; i < repeat; i++)
                {
                    if (i > 0) await Task.Delay(TimeSpan.FromMilliseconds(repeatDelayMs));

                    Log.Info(this, $"Cycling relay {relay}.");
                    await _commands.ToggleRelay(relay, TimeSpan.Zero);
                    await Task.Delay(220); //the relay cycles over 200ms. Add a few ms to allow for communication.
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

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
using System.Reactive.Linq;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware.Commands;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Eyedrivomatic.ButtonDriver.Hardware.Models;
using Prism.Logging;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    [Export(typeof(IButtonDriver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public class BrainBoxDriver : BindableBase, IButtonDriver
    {
        private readonly IBrainBoxCommands _commands;
        private readonly IList<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>> _messageProcessors;

        public static uint AvailableRelays = 3;

        private ILoggerFacade Logger { get; }

        [ImportingConstructor]
        internal BrainBoxDriver(
            IBrainBoxConnection connection, 
            IBrainBoxCommands commandFactory,
            [ImportMany] IEnumerable<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>> messageProcessors,
            IDeviceStatus deviceStatus,
            IDeviceSettings deviceSettings,
            ILoggerFacade logger
            )
        {
            Logger = logger;
            Connection = connection;
            _commands = commandFactory;
            _messageProcessors = new List<Lazy<IBrainBoxMessageProcessor, IMessageProcessorMetadata>>(messageProcessors);
            DeviceStatus = deviceStatus;
            DeviceSettings = deviceSettings;

            deviceStatus.PropertyChanged += OnStatusChanged;

            connection.ConnectionStateChanged += OnConnectionStateChanged;
            connection.DataStream.Subscribe(AttachToDataStream);
        }

        private void OnStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceStatus.IsKnown))
            {
                RaisePropertyChanged(nameof(HardwareReady));
            }

            RaisePropertyChanged(nameof(CurrentDirection));
            RaisePropertyChanged(nameof(ReadyState));
        }

        private void AttachToDataStream(IObservable<char> observable)
        {
            var dataStream = observable.Publish();

            foreach (var lazy in _messageProcessors)
            {
                Logger.Log($"Attaching datastream to [{lazy.Metadata.Name}].", Category.Debug, Priority.None);
                lazy.Value.Attach(dataStream);
            }

            dataStream.Connect();

            _commands.GetConfiguration(SettingNames.All);
            _commands.GetStatus();
        }

        private void OnConnectionStateChanged(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(nameof(CurrentDirection));
            RaisePropertyChanged(nameof(HardwareReady));
            RaisePropertyChanged(nameof(ReadyState));
        }

        #region Connection

        public IBrainBoxConnection Connection { get; }

        #endregion Connection

        #region DeviceInfo
        public uint RelayCount => 3;
        #endregion DeviceInfo

        #region Status

        public IDeviceStatus DeviceStatus { get; }

        public bool HardwareReady => Connection.State == ConnectionState.Connected && DeviceStatus.IsKnown;

        public ReadyState ReadyState
        {
            get
            {
                if (!HardwareReady) return ReadyState.None;

                if (SafetyBypass == SafetyBypassState.Unsafe) return ReadyState.Any;

                if (LastDirection == Direction.None || ContinueState != ContinueState.NotContinuedRecently) return ReadyState.Any;
                if (CurrentDirection != Direction.None) return ReadyState.Continue;

                return ReadyState.Reset;
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

        private SafetyBypassState _safetyBypass = SafetyBypassState.Safe;
        public SafetyBypassState SafetyBypass
        {
            get => _safetyBypass;
            set
            {
                Logger?.Log($"Toggling safety bypass status.", Category.Warn, Priority.None);
                SetProperty(ref _safetyBypass, value);
                RaisePropertyChanged(nameof(ReadyState));
            }
        }

        public IDeviceSettings DeviceSettings { get; }

        #region Trim

        private Direction _lastDirection;
        private ContinueState _continueState;
        private Profile _profile;

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
            Logger?.Log($"Continue.", Category.Info, Priority.None);
            ContinueState = ContinueState.Continued;
        }

        public void Stop()
        {
            Logger?.Log($"Stop.", Category.Info, Priority.None);
            _commands.Stop();
            ContinueState = ContinueState.Continued;
        }

        public async Task Nudge(XDirection direction)
        {
            Logger?.Log($"Nudge {direction}.", Category.Info, Priority.None);

            if (CurrentDirection != Direction.Forward)
            {
                Logger?.Log($"Nudge only available while moving forward.", Category.Warn, Priority.None);
                return;
            }

            ContinueState = ContinueState.NotContinuedRecently;
           
            try
            {
                var x = direction == XDirection.Right ? Profile.CurrentSpeed.Nudge : -Profile.CurrentSpeed.Nudge;
                if (!await _commands.Move(x, Profile.CurrentSpeed.YForward, Profile.NudgeDuration))
                {
                    Logger?.Log($"Failed to send nudge [{direction}] command.", Category.Exception, Priority.None);
                }
            }
            catch (Exception e)
            {
                Logger?.Log($"Failed to send nudge [{direction}] command - [{e}]", Category.Exception, Priority.None);
            }
        }

        public bool CanMove(Direction direction)
        {
            return HardwareReady &&
                (SafetyBypass == SafetyBypassState.Unsafe
                || ContinueState == ContinueState.Continued
                || LastDirection != direction);
        }

        public async Task Move(Direction direction)
        {
            Logger?.Log($"Move {direction}.", Category.Info, Priority.None);

            ContinueState = ContinueState.NotContinuedRecently;

            var speed = Profile.CurrentSpeed;
            var xDiag = Profile.DiagonalSpeedReduction ? speed.XDiagReduced : speed.XDiag;
            var yDiagForward = Profile.DiagonalSpeedReduction ? speed.YForwardDiagReduced : speed.YForwardDiag;
            var yDiagBackward = Profile.DiagonalSpeedReduction ? speed.YBackwardDiagReduced : speed.YBackwardDiag;

            var directionCommands = new Dictionary<Direction, Func<Task<bool>>>
                {
                    {Direction.Forward,       () => _commands.Move(0, speed.YForward, Profile.YDuration)},
                    {Direction.ForwardRight,  () => _commands.Move(xDiag, yDiagForward, Profile.YDuration)},
                    {Direction.Right,         () => _commands.Move(speed.X, 0, Profile.XDuration)},
                    {Direction.BackwardRight, () => _commands.Move(xDiag,  yDiagBackward, Profile.YDuration)},
                    {Direction.Backward,      () => _commands.Move(0, speed.YBackward, Profile.YDuration)},
                    {Direction.BackwardLeft,  () => _commands.Move(-xDiag, yDiagBackward, Profile.YDuration)},
                    {Direction.Left,          () => _commands.Move(-speed.X, 0, Profile.XDuration)},
                    {Direction.ForwardLeft,   () => _commands.Move(-xDiag, yDiagForward, Profile.YDuration)},
                };

            if (!directionCommands.ContainsKey(direction)) throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(Direction));

            try
            {
                LastDirection = direction;
                if (!await directionCommands[direction]())
                {
                    Logger?.Log($"Failed to send move [{direction}] command.", Category.Exception, Priority.None);
                }
            }
            catch (Exception e)
            {
                Logger?.Log($"Failed to send move [{direction}] command - [{e}]", Category.Exception, Priority.None);
            }
        }

        public async Task CycleRelayAsync(uint relay, uint repeat = 1, uint repeatDelayMs = 0)
        {
            Logger?.Log($"Cycling relay {relay} {repeat} times with a delay of {repeatDelayMs}.", Category.Info, Priority.None);
            try
            {
                for (int i = 0; i < repeat; i++)
                {
                    if (i > 0) await Task.Delay(TimeSpan.FromMilliseconds(repeatDelayMs));

                    Logger?.Log($"Cycling relay {relay}.", Category.Info, Priority.None);
                    await _commands.ToggleRelay(relay, TimeSpan.Zero);
                    await Task.Delay(220); //the relay cycles over 200ms. Add a few ms to allow for communication.
                }
                Logger?.Log($"Cycling relay {relay} done.", Category.Info, Priority.None);
            }
            catch (Exception e)
            {
                Logger?.Log($"Failed to toggle relay command - [{e}]", Category.Exception, Priority.None);
            }
        }
        #endregion Control

        #region IDisposable
        public void Dispose()
        {
            Connection.Dispose();
        }
        #endregion IDisposable
    }
}

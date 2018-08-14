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
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.Common;
using Eyedrivomatic.Device;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Logging;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.Services
{
    [Export(typeof(IButtonDriver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public class ButtonDriver : BindableBase, IButtonDriver
    {
        private Direction _lastDirection;
        private ContinueState _continueState;
        private int _nudge;
        private Profile _profile;
        private readonly IDevice _device;

        [ImportingConstructor]
        internal ButtonDriver(IDevice device)
        {
            _device = device;
            _device.DeviceStatus.PropertyChanged += OnDeviceStatusChanged;
            _device.Connection.ConnectionStateChanged += OnDeviceConnectionStateChanged;
        }

        #region Status
        private void OnDeviceStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CurrentDirection));
            RaisePropertyChanged(nameof(ReadyState));
        }

        private void OnDeviceConnectionStateChanged(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(nameof(CurrentDirection));
            RaisePropertyChanged(nameof(ReadyState));
        }

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

        public ConnectionState ConnectionState => _device.Connection.State;

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

        public uint SwitchCount => _device.DeviceSettings.SwitchCount;
        #endregion Status

        #region Settings
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

        public bool DeviceReady => _device.DeviceReady;

        public IDeviceStatus DeviceStatus => _device.DeviceStatus;

        public Task CycleSwitchAsync(uint relay, uint repeat = 1, uint toggleDelayMs = 500, uint repeatDelayMs = 1000)
        {
            return _device.CycleSwitchAsync(relay, repeat, toggleDelayMs, repeatDelayMs);
        }

        public void Stop()
        {
            _device.Stop();
            _nudge = 0;
            ContinueState = ContinueState.Continued;
        }

        public void Continue()
        {
            Log.Info(this, "Continue.");
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
                if (!await _device.Move(new Point(_nudge, Profile.CurrentSpeed.YForward), duration))
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
                    {Direction.Forward,       () => _device.Move(new Point(_nudge, speed.YForward), duration)},
                    {Direction.ForwardRight,  () => _device.Move(new Point(speed.XDiag, speed.YForwardDiag), duration)},
                    {Direction.Right,         () => _device.Move(new Point(speed.X, 0), duration)},
                    {Direction.BackwardRight, () => _device.Move(new Point(speed.XDiag,  -speed.YBackwardDiag), duration)},
                    {Direction.Backward,      () => _device.Move(new Point(0, -speed.YBackward), duration)},
                    {Direction.BackwardLeft,  () => _device.Move(new Point(-speed.XDiag, -speed.YBackwardDiag), duration)},
                    {Direction.Left,          () => _device.Move(new Point(-speed.X, 0), duration)},
                    {Direction.ForwardLeft,   () => _device.Move(new Point(-speed.XDiag, speed.YForwardDiag), duration)},
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

        #endregion Control
    }
}

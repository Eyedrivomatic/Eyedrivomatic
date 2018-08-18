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
using Eyedrivomatic.Device.Services;
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
        private readonly IDeviceService _deviceService;

        [ImportingConstructor]
        internal ButtonDriver(IDeviceService deviceService)
        {
            _deviceService = deviceService;
            _deviceService.ConnectionStateChanged += OnDeviceConnectionStateChanged;
            _deviceService.ConnectedDeviceChanged += OnConnectedDeviceChanged;
            if (_deviceService.ConnectedDevice != null)
            {
                _deviceService.ConnectedDevice.PropertyChanged += OnDeviceStatusChanged;
            }
        }

        #region Status
        private void OnConnectedDeviceChanged(object sender, ConnectedDeviceChangedArgs eventArgs)
        {
            if (eventArgs.PrevDevice != null)
            {
                eventArgs.PrevDevice.PropertyChanged -= OnDeviceStatusChanged;
            }
            if (_deviceService.ConnectedDevice != null)
            {
                _deviceService.ConnectedDevice.PropertyChanged += OnDeviceStatusChanged;
            }

            RaisePropertyChanged(string.Empty);
        }

        private void OnDeviceStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CurrentDirection));
            RaisePropertyChanged(nameof(ReadyState));
        }

        private void OnDeviceConnectionStateChanged(object sender, ConnectionState connectionState)
        {
            RaisePropertyChanged(nameof(ConnectionState));
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

        
        public ConnectionState ConnectionState => _deviceService.ConnectionState;

        public Direction CurrentDirection
        {
            get
            {
                if (!DeviceReady || DeviceStatus.Vector.Speed < 0.1m) return Direction.None;
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

        public uint SwitchCount => (uint)(_deviceService.ConnectedDevice?.DeviceStatus.Switches.Count ?? 0);
        #endregion Status

        #region Settings
        [Import("CurrentProfile")]
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

        public bool DeviceReady => _deviceService.ConnectedDevice?.DeviceReady ?? false;

        [Export(typeof(IDeviceStatus))]
        public IDeviceStatus DeviceStatus => _deviceService.ConnectedDevice?.DeviceStatus;

        public Task CycleSwitchAsync(uint relay, uint repeat = 1, uint toggleDelayMs = 500, uint repeatDelayMs = 1000)
        {
            return _deviceService.ConnectedDevice?.CycleSwitchAsync(relay, repeat, toggleDelayMs, repeatDelayMs) ?? throw new InvalidOperationException("Not Connected");
        }

        public void Stop()
        {
            _deviceService.ConnectedDevice?.Stop();
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

            if (!DeviceReady) throw new InvalidOperationException("Not connected");

            if (LastDirection != Direction.Forward || CurrentDirection == Direction.None)
            {
                Log.Warn(this, "Nudge only available while moving forward.");
                return;
            }

            ContinueState = ContinueState.Continued;

            try
            {
                _nudge += direction == XDirection.Right ? Profile.CurrentSpeed.Nudge : -Profile.CurrentSpeed.Nudge;
                if (!await _deviceService.ConnectedDevice.Move(new Point(_nudge, Profile.CurrentSpeed.YForward), duration))
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

            if (!DeviceReady) throw new InvalidOperationException("Not connected");

            ContinueState = ContinueState.NotContinuedRecently;

            if (CurrentDirection != Direction.Forward && CurrentDirection != Direction.Backward) _nudge = 0;

            var speed = Profile.CurrentSpeed;

            var directionCommands = new Dictionary<Direction, Func<Task<bool>>>
                {
                    {Direction.Forward,       () => _deviceService.ConnectedDevice.Move(new Point(_nudge, speed.YForward), duration)},
                    {Direction.ForwardRight,  () => _deviceService.ConnectedDevice.Move(new Point(speed.XDiag, speed.YForwardDiag), duration)},
                    {Direction.Right,         () => _deviceService.ConnectedDevice.Move(new Point(speed.X, 0), duration)},
                    {Direction.BackwardRight, () => _deviceService.ConnectedDevice.Move(new Point(speed.XDiag,  -speed.YBackwardDiag), duration)},
                    {Direction.Backward,      () => _deviceService.ConnectedDevice.Move(new Point(0, -speed.YBackward), duration)},
                    {Direction.BackwardLeft,  () => _deviceService.ConnectedDevice.Move(new Point(-speed.XDiag, -speed.YBackwardDiag), duration)},
                    {Direction.Left,          () => _deviceService.ConnectedDevice.Move(new Point(-speed.X, 0), duration)},
                    {Direction.ForwardLeft,   () => _deviceService.ConnectedDevice.Move(new Point(-speed.XDiag, speed.YForwardDiag), duration)},
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

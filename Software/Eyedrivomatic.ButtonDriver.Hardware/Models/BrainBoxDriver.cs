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
//    Eyedrivomaticis distributed in the hope that it will be useful,
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
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Ports;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using System.Reactive.Linq;

using Prism.Events;
using Prism.Logging;

namespace Eyedrivomatic.ButtonDriver.Hardware
{
    [Export(typeof(IButtonDriver))]
    public class BrainBoxDriver : IButtonDriver
    {
        private enum BrainBoxCommand
        {
            GetStatus = 33,                      // Get the current status
            ToggleRelay1 = 34,                   // Toggle the relay for output socket 1 for two tenths of a second - wheelchair on / off button
            ToggleRelay2 = 35,                   // Toggle the relay for output socket 2 (mode) for two tenths of a second
            ToggleRelay2Repeat5x = 36,           // Toggle the relay for output socket 2 for two tenths of a second five times
            ToggleRelay3 = 48,                   // Toggle the relay for output socket 3 for two tenths of a second 
            MoveForwardLeft = 37,                // Forward-Left (NW) direction button press
            MoveForward = 38,                    // Forward (N) direction button press 
            MoveForwardRight = 39,               // Forward-Right (NE) direction button press
            MoveRight = 40,                      // Right (E) direction button press
            MoveBackwardRight = 41,             // Backward-Right (SE) direction button press
            MoveBackward = 42,                   // Backward (S) direction button press
            MoveBackwardLeft = 43,               // Backward-Left (SW) direction button press
            MoveLeft = 44,                       // Left (W) direction button press
            Continue = 47,                       // Continue button press
            SafetyBypassToggle = 45,             // Safety bypass toggle (this allows repeated button presses) !!!DANGER this feature is inherently dangerous. Use with extreme caution!
            NudgeLeft = 46,                      // Nudge left
            NudgeRight = 77,                     // Nudge right
            Stop = 49,                           // Stop
            TrimForward = 50,                    // Trim Forward (N)
            TrimRight = 51,                      // Trim Right (E)
            TrimBackward = 52,                   // Trim Backward (S)
            TrimLeft = 53,                       // Trim Left (W)
            SpeedLevel1 = 55,                    // Set Speed = 1
            SpeedLevel2 = 56,                    // Set Speed = 2
            SpeedLevel3 = 57,                    // Set Speed = 3
            SpeedLevel4 = 58,                    // Set Speed = 4
            DurationForwardBackHalf = 60,        // Set Forward-Backward (NS) Duration = half
            DurationForwardBack1 = 61,           // Set Forward-Backward (NS) Duration = 1
            DurationForwardBack2 = 62,           // Set Forward-Backward (NS) Duration = 2
            DurationForwardBack3 = 63,           // Set Forward-Backward (NS) Duration = 3
            DurationForwardBack4 = 64,           // Set Forward-Backward (NS) Duration = 4
            DurationForwardBack5 = 54,           // Set Forward-Backward (NS) Duration = 5
            DurationForwardBack6 = 65,           // Set Forward-Backward (NS) Duration = 6
            DurationLeftRightHalf = 66,          // Set Right-Left (EW) Duration = half
            DurationLeftRight1 = 67,             // Set Right-Left (EW) Duration = 1
            DurationLeftRight1AndHalf = 68,      // Set Right-Left (EW) Duration = 1.5
            DurationLeftRight2 = 69,             // Set Right-Left (EW) Duration = 1
            DurationLeftRight3 = 70,             // Set Right-Left (EW) Duration = 1
            DurationLeftRight4 = 71,             // Set Right-Left (EW) Duration = 1
            DurationLeftRight5 = 72,             // Set Right-Left (EW) Duration = 1
            NudgeDurationUp300Ms = 73,           // Nudge Duration up 300 ms
            NudgeDurationDown300Ms = 74,         // Nudge Duration down 300 ms
            SaveSettings = 75,                   // Save settings to EEPROM
            NudgeSpeedUp1 = 59,                  // Nudge Speed Up One
            NudgeSpeedDown1 = 76,                // Nudge Speed Down One
            DiagonalReducerToggle = 78,          // Diagonal Reducer Toggle
        }

        private ILoggerFacade Logger { get; }
        private IEventAggregator Events { get; }

        private SerialPort _serialPort;
        private BrainBoxStatusMessage _lastStatusMessage = BrainBoxStatusMessage.Empty;
        private readonly object _statusLock = new object();
        private IDisposable _statusFeed;
        private bool _isTogglingRelays;

        [ImportingConstructor]
        public BrainBoxDriver(IEventAggregator events, ILoggerFacade logger)
        {
            Contract.Requires<ArgumentNullException>(events != null, nameof(events));
            Logger = logger;
        }

        public uint RelayCount => 3;

        public bool IsConnected => _serialPort?.IsOpen ?? false;
        public bool HardwareReady => IsConnected && _lastStatusMessage.IsValid;
        public string ConnectionString => _serialPort?.PortName ?? String.Empty;

        public ReadyState ReadyState
        {
            get
            {
                lock (_statusLock)
                {
                    if (!HardwareReady) return ReadyState.None;

                    if (_isTogglingRelays) return ReadyState.None;

                    if (SafetyBypassStatus == SafetyBypassState.Unsafe) return ReadyState.Any;

                    if (LastDirection == Direction.None || ContinueState != ContinueState.NotContinuedRecently) return ReadyState.Any;
                    if (CurrentDirection != Direction.None) return ReadyState.Continue;

                    return ReadyState.Reset;
                }
            }
        }

        public async Task<bool> ConnectAsync(string configuration = null)
        {
            try
            {
                Disconnect();
            }
            catch (IOException)
            {
                //Ignore failure to close. 
            }

            if (String.IsNullOrEmpty(configuration))
            {
                _serialPort = await BrainBoxPortFinder.DetectDeviceAsync();
            }
            else
            {
                _serialPort = BrainBoxPortFinder.OpenSerialPort(configuration);
                var reader = new StreamReader(_serialPort.BaseStream); //Don't dispose. It will close the underlying stream.
                await reader.ReadLineAsync();
            }

            OnStatusChanged();

            //If the serial port could not be opened (is null or ISOpen is false), the connection failed.
            if (!IsConnected) return false;

            var status = Observable
                .FromAsync(() => ReadStatusAsync(_serialPort))
                .Repeat()
                .Publish()
                .RefCount()
                .SubscribeOn(TaskPoolScheduler.Default);

            _statusFeed = status.Subscribe(OnDeviceStatusChanged);

            return IsConnected;
        }

        private void OnDeviceStatusChanged(BrainBoxStatusMessage message)
        {
            if (!IsConnected || _lastStatusMessage.Equals(message)) return; //This and the Disconnect method are the only setters of _lastStatusMessage. So no need to put the equality test in a lock.

            lock (_statusLock)
            {
                _lastStatusMessage = message;
                Logger?.Log($"Status Changed: NewStatus = {message}", Category.Info, Priority.None);
            }
            OnStatusChanged();
        }

        private async Task<BrainBoxStatusMessage> ReadStatusAsync(SerialPort serialPort)
        {
            try
            {
                string strMessage;
                if (!_serialPort?.IsOpen ?? false) return BrainBoxStatusMessage.Empty;
                var reader = new StreamReader(_serialPort.BaseStream); //Don't dispose. It will close the underlying stream.
                strMessage = await reader.ReadLineAsync();

                return BrainBoxStatusMessage.BuildStatusMessage(BrainBoxStatusMessage.DefaultPattern, strMessage);
            }
            catch (TimeoutException)
            {
                Logger?.Log("Status request timeout.", Category.Warn, Priority.None);
                //connection failed.
                return BrainBoxStatusMessage.Empty;
            }
            catch (IOException ex)
            {
                if (_serialPort != null)
                {
                    //connection failed.
                    Logger?.Log($"Status request error - {ex}", Category.Exception, Priority.None);
                }

                return BrainBoxStatusMessage.Empty;
            }
        }

        public void Disconnect()
        {
            if (IsConnected) Logger?.Log("Disconnecting", Category.Info, Priority.None);

            _statusFeed?.Dispose();
            var tmp = _serialPort;
            _serialPort = null;
            tmp?.BaseStream.Close();
            tmp?.Close();
            tmp?.Dispose();

            _lastStatusMessage = BrainBoxStatusMessage.Empty;

            OnStatusChanged();
        }

        public Direction CurrentDirection
        {
            get
            {
                lock (_statusLock)
                {
                    if (!_lastStatusMessage.IsValid) return Direction.None;

                    if (_lastStatusMessage.JoystickStateForwardBackward == BrainBoxStatusMessage.JoystickStateForwardBackwardValue.None)
                    {
                        if (_lastStatusMessage.JoystickStateLeftRight == BrainBoxStatusMessage.JoystickStateLeftRightValue.Left) return Direction.Left;
                        if (_lastStatusMessage.JoystickStateLeftRight == BrainBoxStatusMessage.JoystickStateLeftRightValue.Right) return Direction.Right;
                        return Direction.None;
                    }
                    if (_lastStatusMessage.JoystickStateForwardBackward == BrainBoxStatusMessage.JoystickStateForwardBackwardValue.Forward)
                    {
                        if (_lastStatusMessage.JoystickStateLeftRight == BrainBoxStatusMessage.JoystickStateLeftRightValue.Left) return Direction.ForwardLeft;
                        if (_lastStatusMessage.JoystickStateLeftRight == BrainBoxStatusMessage.JoystickStateLeftRightValue.Right) return Direction.ForwardRight;
                        return Direction.Forward;
                    }
                    else if (_lastStatusMessage.JoystickStateForwardBackward == BrainBoxStatusMessage.JoystickStateForwardBackwardValue.Backward)
                    {
                        if (_lastStatusMessage.JoystickStateLeftRight == BrainBoxStatusMessage.JoystickStateLeftRightValue.Left) return Direction.BackwardLeft;
                        if (_lastStatusMessage.JoystickStateLeftRight == BrainBoxStatusMessage.JoystickStateLeftRightValue.Right) return Direction.BackwardRight;
                        return Direction.Backward;
                    }
                    return Direction.None;
                }
            }
        }
        public Direction LastDirection { get { lock (_statusLock) { return _lastStatusMessage.LastDirbuttpress; } } }
        public ContinueState ContinueState { get { lock (_statusLock) { return _lastStatusMessage.ContinueState; } } }

        public bool DiagonalSpeedReduction
        {
            get { lock (_statusLock) { return HardwareReady && _lastStatusMessage.DiagonalReducer == BrainBoxStatusMessage.DiagonalReducerState.Enabled; } }
            set
            {
                lock (_statusLock)
                {
                    if (DiagonalSpeedReduction == value) return;

                    Logger?.Log($"Toggling diagonal speed reduction.", Category.Warn, Priority.None);
                    ExecuteCommand(BrainBoxCommand.DiagonalReducerToggle);
                }
            }
        }

        public ulong YDuration
        {
            get { lock (_statusLock) { return _lastStatusMessage.DurationTimeForwardBackward; } }
            set
            {
                Logger?.Log($"Setting Forward/Backward duration to {value} ms.", Category.Info, Priority.None);

                if (value == 500) ExecuteCommand(BrainBoxCommand.DurationForwardBackHalf );
                else if (value == 1000) ExecuteCommand(BrainBoxCommand.DurationForwardBack1);
                else if (value == 2000) ExecuteCommand(BrainBoxCommand.DurationForwardBack2 );
                else if (value == 3000) ExecuteCommand(BrainBoxCommand.DurationForwardBack3 );
                else if (value == 4000) ExecuteCommand(BrainBoxCommand.DurationForwardBack4 );
                else if (value == 5000) ExecuteCommand(BrainBoxCommand.DurationForwardBack5 );
                else if (value == 6000) ExecuteCommand(BrainBoxCommand.DurationForwardBack6 );
                else throw new ArgumentOutOfRangeException("Custom durations are not yet supported.");
            }
        }

        public ulong XDuration
        {
            get { lock (_statusLock) { return _lastStatusMessage.DurationTimeLeftRight; } }
            set
            {
                Logger?.Log($"Setting Left/Right duration to {value} ms.", Category.Info, Priority.None);

                if (value == 500) ExecuteCommand(BrainBoxCommand.DurationLeftRightHalf);
                else if (value == 1000) ExecuteCommand(BrainBoxCommand.DurationLeftRight1);
                else if (value == 1500) ExecuteCommand(BrainBoxCommand.DurationLeftRight1AndHalf);
                else if (value == 2000) ExecuteCommand(BrainBoxCommand.DurationLeftRight2);
                else if (value == 3000) ExecuteCommand(BrainBoxCommand.DurationLeftRight3);
                else if (value == 4000) ExecuteCommand(BrainBoxCommand.DurationLeftRight4);
                else if (value == 5000) ExecuteCommand(BrainBoxCommand.DurationLeftRight5);
                else throw new ArgumentOutOfRangeException("Custom durations are not yet supported.");
            }
        }

        public ulong NudgeDuration
        {
            get { lock (_statusLock) { return _lastStatusMessage.NudgeDuration; } }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void IncreaseNudgeDuration()
        {
            Logger?.Log($"Increasing nudge duration.", Category.Info, Priority.None);
            //TODO: This is a workaround until NudgeDuration can be set directly.
            ExecuteCommand(BrainBoxCommand.NudgeDurationUp300Ms);
        }

        public void DecreaseNudgeDuration()
        {
            Logger?.Log($"Decreasing nudge duration.", Category.Info, Priority.None);
            //TODO: This is a workaround until NudgeDuration can be set directly.
            ExecuteCommand(BrainBoxCommand.NudgeDurationDown300Ms);
        }

        public Speed NudgeSpeed
        {
            get { lock (_statusLock) { return _lastStatusMessage.NudgeSpeed; } }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void IncreaseNudgeSpeed()
        {
            Logger?.Log("Increasing nudge speed.", Category.Info, Priority.None);
            //TODO: This is a workaround until NudgeSpeed can be set directly.
            ExecuteCommand(BrainBoxCommand.NudgeSpeedUp1);
        }

        public void DecreaseNudgeSpeed()
        {
            Logger?.Log("Decreasing nudge speed.", Category.Info, Priority.None);
            //TODO: This is a workaround until NudgeSpeed can be set directly.
            ExecuteCommand(BrainBoxCommand.NudgeSpeedDown1);
        }

        public SafetyBypassState SafetyBypassStatus
        {
            get { lock (_statusLock) { return _lastStatusMessage.SafetyBypass; } }
            set
            {
                lock (_statusLock)
                {
                    Logger?.Log($"Toggling safety bypass status.", Category.Warn, Priority.None);
                    ExecuteCommand(BrainBoxCommand.SafetyBypassToggle); //TODO: THIS SHOULD REALLY BE AN ON/OFF COMMAND.
                }
            }
        }

        public Speed Speed
        {
            get { lock (_statusLock) { return _lastStatusMessage.SpeedState; } }
            set
            {
                Logger?.Log($"Setting nudge speed to {value}.", Category.Info, Priority.None);

                if (value == Speed.Slow) ExecuteCommand(BrainBoxCommand.SpeedLevel1);
                else if (value == Speed.Walk) ExecuteCommand(BrainBoxCommand.SpeedLevel2);
                else if (value == Speed.Fast) ExecuteCommand(BrainBoxCommand.SpeedLevel3);
                else if (value == Speed.Manic) ExecuteCommand(BrainBoxCommand.SpeedLevel4);
                else throw new InvalidEnumArgumentException("Custom speeds are not supported.");
            }
        }

        public int XServoCenter
        {
            get { lock (_statusLock) { return _lastStatusMessage.XMidPos; } }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void TrimRight()
        {
            Logger?.Log($"Trim right.", Category.Info, Priority.None);
            ExecuteCommand(BrainBoxCommand.TrimRight);
        }

        public void TrimLeft()
        {
            Logger?.Log($"Trim left.", Category.Info, Priority.None);
            ExecuteCommand(BrainBoxCommand.TrimLeft);
        }

        public int YServoCenter
        {
            get { lock (_statusLock) { return _lastStatusMessage.YMidPos; } }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void TrimForward()
        {
            Logger?.Log($"Trim forward.", Category.Info, Priority.None);
            ExecuteCommand(BrainBoxCommand.TrimForward);
        }

        public void TrimBackward()
        {
            Logger?.Log($"Trim backward.", Category.Info, Priority.None);
            ExecuteCommand(BrainBoxCommand.TrimBackward);
        }

        public bool CanMove(Direction direction)
        {
            lock(_statusLock)
            {
                return HardwareReady && !_isTogglingRelays && 
                    ( SafetyBypassStatus == SafetyBypassState.Unsafe
                    || ContinueState != ContinueState.NotContinuedRecently
                    || LastDirection != direction);
            }
        }

        public void Continue()
        {
            Logger?.Log($"Continue.", Category.Info, Priority.None);
            ExecuteCommand(BrainBoxCommand.Continue);
        }

        public void Move(Direction direction)
        {
            Logger?.Log($"Move {direction}.", Category.Info, Priority.None);

            switch (direction)
            {
                case Direction.ForwardLeft:
                    ExecuteCommand(BrainBoxCommand.MoveForwardLeft);
                    break;
                case Direction.Forward:
                    ExecuteCommand(BrainBoxCommand.MoveForward);
                    break;
                case Direction.ForwardRight:
                    ExecuteCommand(BrainBoxCommand.MoveForwardRight);
                    break;
                case Direction.Right:
                    ExecuteCommand(BrainBoxCommand.MoveRight);
                    break;
                case Direction.BackwardRight:
                    ExecuteCommand(BrainBoxCommand.MoveBackwardRight);
                    break;
                case Direction.Backward:
                    ExecuteCommand(BrainBoxCommand.MoveBackward);
                    break;
                case Direction.BackwardLeft:
                    ExecuteCommand(BrainBoxCommand.MoveBackwardLeft);
                    break;
                case Direction.Left:
                    ExecuteCommand(BrainBoxCommand.MoveLeft);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(Direction));
            }
        }

        public void Nudge(XDirection direction)
        {
            Logger?.Log($"Nudge {direction}.", Category.Info, Priority.None);

            switch (direction)
            {
                case XDirection.Left:
                    ExecuteCommand(BrainBoxCommand.NudgeLeft);
                    break;
                case XDirection.Right:
                    ExecuteCommand(BrainBoxCommand.NudgeRight);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(XDirection));
            }
        }

        public void Reset()
        {
            Logger?.Log($"Reset.", Category.Info, Priority.None);
            ExecuteCommand(BrainBoxCommand.Stop);
        }

        public async Task ToggleRelayAsync(uint relay, uint repeat = 1, uint repeatDelayMs = 0)
        {
            Logger?.Log($"Toggling relay {relay} {repeat} times with a delay of {repeatDelayMs}.", Category.Info, Priority.None);
            _isTogglingRelays = true;

            try
            {
                BrainBoxCommand relayCommand;
                if (relay == 1) relayCommand = BrainBoxCommand.ToggleRelay1;
                else if (relay == 2) relayCommand = BrainBoxCommand.ToggleRelay2;
                else if (relay == 3) relayCommand = BrainBoxCommand.ToggleRelay3;
                else throw new ArgumentOutOfRangeException(nameof(relay));

                for (int i = 0; i < repeat; i++)
                {
                    if (i > 0) await Task.Delay(TimeSpan.FromMilliseconds(repeatDelayMs));

                    Logger?.Log($"Toggling relay {relay}.", Category.Info, Priority.None);
                    ExecuteCommand(relayCommand);
                }
            }
            finally
            {
                _isTogglingRelays = false;
            }
        }

        private void ExecuteCommand(BrainBoxCommand command)
        {
            _serialPort?.Write(new char[] { (char)command }, 0, 1);
        }

        public IList<string> GetAvailableDevices()
        {
            return BrainBoxPortFinder.GetAvailableDevices();
        }

        public event EventHandler StatusChanged;
        private void OnStatusChanged()
        {
            StatusChanged?.Invoke(this, new EventArgs());
        }
    }
}

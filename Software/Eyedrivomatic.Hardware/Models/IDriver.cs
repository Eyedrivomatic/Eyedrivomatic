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


using Eyedrivomatic.Hardware.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Eyedrivomatic.Hardware
{
    public enum ReadyState { None, Any, Continue, Reset }

    public enum Speed { None, Slow, Walk, Fast, Manic }

    public enum Direction { None, Forward, ForwardLeft, ForwardRight, Left, Right, Backward, BackwardLeft, BackwardRight }

    public enum XDirection { Left, Right }

    public enum YDirection { Forward, Backward }

    public enum SafetyBypassState { Safe, Unsafe }

    public enum ContinueState {
        NotContinuedRecently = 0,
        JustContinuedDiagonally = 1,
        JustContinuedForwardBackward = 2,
        JustContinuedLeftRight = 3
    }

    /// <summary>
    /// This it the interface to the device hardware.
    /// </summary>
    [ContractClass(typeof(DriverContract))]
    public interface IDriver
    {
        uint RelayCount { get; }

        /// <summary>
        /// True when a connection to the device has been established.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// The device is currently sending valid status messages.
        /// </summary>
        bool HardwareReady { get; }

        /// <summary>
        /// The connection string that was used to start the connection.
        /// This is the auto-detected configuration is none was supplied to the Connect method.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Indicates which next actions are valid.
        /// </summary>
        ReadyState ReadyState { get; }

        /// <summary>
        /// The state of the SafetyBypass
        /// </summary>
        SafetyBypassState SafetyBypassStatus { get; set; }

        /// <summary>
        /// True if diagnonal speed reduction is enabled.
        /// </summary>
        bool DiagonalSpeedReduction { get; set; }

        /// <summary>
        /// The direction that the device is currently applying.
        /// </summary>
        Direction CurrentDirection { get; }

        /// <summary>
        /// The last move direction that the device executed.
        /// </summary>
        Direction LastDirection { get; }

        /// <summary>
        /// The continue state indicates whether the continue button was pressed recently.
        /// The state also indicates which direction type (forward/backward, left/right, or diagonal) was 
        /// executing when the button was pressed.
        /// </summary>
        ContinueState ContinueState { get; }

        /// <summary>
        /// The center position of the Left/Right servo
        /// </summary>
        int XServoCenter { get; set; }

        /// <summary>
        /// The center position of the Forward/Backward servo
        /// </summary>
        int YServoCenter { get; set; }

        /// <summary>
        /// Fired when the device updates its status.
        /// All status values are refreshed befor this event fires.
        /// </summary>
        event EventHandler StatusChanged;

        /// <summary>
        /// Connect to the device.
        /// For the current Arduino platform, the configuration should be either null or "COM#" where # is the COM port number
        /// assigned to the device.
        /// If configuration is null or empty, the driver will attempt to auto-discover the device.
        /// </summary>
        /// <param name="connectionString">The platform specific connection string.</param>
        /// <returns>True if the connection was established. False otherwise.</returns>
        Task<bool> ConnectAsync(string connectionString = null);

        /// <summary>
        /// Disconnect from the device.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// The duration of forward/backward movements.
        /// </summary>
        ulong ForwardBackwardDuration { get; set; }

        /// <summary>
        /// The duration of left/right movements.
        /// </summary>
        ulong LeftRightDuration { get; set; }

        /// <summary>
        /// The speed (joystick throw) of movements.
        /// </summary>
        Speed Speed { get; set; }

        /// <summary>
        /// The speed (joystick throw) of nudges.
        /// </summary>
        Speed NudgeSpeed { get; set; }

        /// <summary>
        /// The duration of nudge movements.
        /// </summary>
        ulong NudgeDuration { get; set; }

        /// <summary>
        /// Get a list of potential devices that are connected to the computer.
        /// For the Arduino platform, this is a list of all COM ports.
        /// </summary>
        /// <returns></returns>
        IList<string> GetAvailableDevices();

        /// <summary>
        /// Called to allow a movement in the same direction following the current movement.
        /// </summary>
        void Continue();

        /// <summary>
        /// Reset current and last movements.
        /// Allows the user to move in any direction.
        /// </summary>
        void Reset();

        /// <summary>
        /// Nudge in the direction indicated.
        /// </summary>
        /// <param name="direction"></param>
        void Nudge(XDirection direction);

        /// <summary>
        /// Returns true if the user can move in the direction indicated.
        /// This is dictated by the safety override, or the last direction and the continue state.
        /// </summary>
        /// <param name="direction">The direction to test</param>
        /// <returns>True if the next move command may be in that direction.</returns>
        bool CanMove(Direction direction);

        /// <summary>
        /// Move in the direction indicated.
        /// </summary>
        /// <param name="direction">The direction to move</param>
        void Move(Direction direction);

        /// <summary>
        /// Increase Nudge duration by a set step.
        /// This is to be replaced by NudgeDuration set implementation in the future.
        /// </summary>
        void IncreaseNudgeDuration();

        /// <summary>
        /// Decrease Nudge duration by a set step.
        /// This is to be replaced by NudgeDuration set implementation in the future.
        /// </summary>
        void DecreaseNudgeDuration();

        /// <summary>
        /// Increase Nudge speed by a set step.
        /// This is to be replaced by NudgeSpeed set implementation in the future.
        /// </summary>
        void IncreaseNudgeSpeed();

        /// <summary>
        /// Increase Nudge speed by a set step.
        /// This is to be replaced by NudgeSpeed set implementation in the future.
        /// </summary>
        void DecreaseNudgeSpeed();

        /// <summary>
        /// Increase the X servo center point by a degree
        /// </summary>
        void TrimRight();

        /// <summary>
        /// Decrease the X servo center point by a degree
        /// </summary>
        void TrimLeft();

        /// <summary>
        /// Increase the Y servo center point by a degree
        /// </summary>
        void TrimForward();

        /// <summary>
        /// Decrease the Y servo center point by a degree
        /// </summary>
        void TrimBackward();

        /// <summary>
        /// Toggle the specefied relay. Repeat as desired.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the relay does not exist.</exception>
        /// <param name="relay">The relay to toggle.</param>
        /// <param name="repeat">The number of times to repeat the toggle. If 0, the relay will not be activated.</param>
        /// <param name="repeatDelayMs">The delay between repeated relay toggles.</param>
        Task ToggleRelayAsync(uint relay, uint repeat = 1, uint repeatDelayMs = 0);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IDriver))]
        public abstract class DriverContract : IDriver
        {
            public abstract string ConnectionString { get; }
            public ContinueState ContinueState 
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(ContinueState), Contract.Result<ContinueState>()));
                    return default(ContinueState);
                }
            }

            public Direction CurrentDirection
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(Direction), Contract.Result<Direction>()));
                    return default(Direction);
                }
            }

            public abstract bool DiagonalSpeedReduction { get; set; }

            public abstract ulong ForwardBackwardDuration { get; set; }

            public abstract bool HardwareReady { get; }

            public abstract bool IsConnected { get; }

            public Direction LastDirection 
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(Direction), Contract.Result<Direction>()));
                    return default(Direction);
                }
            }

            public abstract ulong LeftRightDuration { get; set; }

            public abstract ulong NudgeDuration { get; set; }

            public Speed NudgeSpeed
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(Speed), Contract.Result<Speed>()));
                    return default(Speed);
                }

                set
                {
                    Contract.Requires(Enum.IsDefined(typeof(Speed), value));
                }
            }

            public ReadyState ReadyState
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(ReadyState), Contract.Result<ReadyState>()));
                    return default(ReadyState);
                }
            }

            public uint RelayCount
            {
                get
                {
                    //This may change in the future. But his makes sure that implementations dont break current expectations.
                    Contract.Ensures(Contract.Result<uint>() > 0);
                    return default(uint);
                }
            }

            public SafetyBypassState SafetyBypassStatus
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(SafetyBypassState), Contract.Result<SafetyBypassState>()));
                    return default(SafetyBypassState);
                }

                set
                {
                    Contract.Requires(Enum.IsDefined(typeof(SafetyBypassState), value));
                }
            }

            public Speed Speed
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(Speed), Contract.Result<Speed>()));
                    return default(Speed);
                }

                set
                {
                    Contract.Requires(Enum.IsDefined(typeof(Speed), value));
                }
            }

            public int XServoCenter
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() >= 0);
                    Contract.Ensures(Contract.Result<int>() <= 180);
                    return default(int);
                }

                set
                {
                    Contract.Requires(value > 0);
                    Contract.Ensures(value < 180);
                }
            }

            public int YServoCenter
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() >= 0);
                    Contract.Ensures(Contract.Result<int>() <= 180);
                    return default(int);
                }

                set
                {
                    Contract.Requires(value > 0);
                    Contract.Ensures(value < 180);
                }
            }

            public abstract event EventHandler StatusChanged;

            public bool CanMove(Direction direction)
            {
                Contract.Requires(Enum.IsDefined(typeof(Direction), direction));
                throw new NotImplementedException();
            }

            public abstract Task<bool> ConnectAsync(string connectionString = null);

            public abstract void Continue();

            public abstract void DecreaseNudgeDuration();

            public abstract void DecreaseNudgeSpeed();

            public abstract void Disconnect();

            public IList<string> GetAvailableDevices()
            {
                Contract.Ensures(Contract.Result<IList<string>>() != null);
                throw new NotImplementedException();
            }

            public abstract void IncreaseNudgeDuration();

            public abstract void IncreaseNudgeSpeed();

            public void Move(Direction direction)
            {
                Contract.Requires(Enum.IsDefined(typeof(Direction), direction));
                throw new NotImplementedException();
            }

            public void Nudge(XDirection direction)
            {
                Contract.Requires(Enum.IsDefined(typeof(XDirection), direction));
                throw new NotImplementedException();
            }

            public abstract void Reset();

            public Task ToggleRelayAsync(uint relay, uint repeat = 1, uint repeatDelayMs = 0)
            {
                Contract.Requires<ArgumentOutOfRangeException>(relay > 0 && relay <= RelayCount, nameof(relay));
                Contract.Requires<ArgumentOutOfRangeException>(repeat > 0, nameof(repeat));
                throw new NotImplementedException();
            }

            public abstract void TrimBackward();

            public abstract void TrimForward();

            public abstract void TrimLeft();

            public abstract void TrimRight();
        }
    }
}

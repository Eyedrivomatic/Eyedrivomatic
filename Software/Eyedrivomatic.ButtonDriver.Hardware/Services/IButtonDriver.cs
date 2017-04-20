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
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Eyedrivomatic.ButtonDriver.Hardware.Models;
using Eyedrivomatic.ButtonDriver.Hardware.Services.Contracts;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    #region Enums
    public enum ReadyState { None, Any, Continue, Reset }

    public enum Direction { None, Forward, ForwardLeft, ForwardRight, Left, Right, Backward, BackwardLeft, BackwardRight }

    public enum XDirection { Left, Right }

    public enum YDirection { Forward, Backward }

    public enum SafetyBypassState { Safe, Unsafe }

    public enum ContinueState { NotContinuedRecently, Continued}
    #endregion Enums

    /// <summary>
    /// This it the interface to the device hardware.
    /// </summary>
    [ContractClass(typeof(DriverContract))]
    public interface IButtonDriver : INotifyPropertyChanged, IDisposable
    {
        #region Connection

        IBrainBoxConnection Connection { get; }

        #endregion Connection

        #region DeviceInfo

        /// <summary>
        /// Returns the number of relays (switches) on the device.
        /// </summary>
        uint RelayCount { get; }

        #endregion DeviceInfo

        #region Status

        /// <summary>
        /// The device is currently sending valid status messages.
        /// </summary>
        bool HardwareReady { get; }

        /// <summary>
        /// Indicates which next actions are valid.
        /// </summary>
        ReadyState ReadyState { get; }

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
        /// The current positions of the servos and state of the relays. 
        /// These values are not valid until the device has connected and reported it status.
        /// </summary>
        IDeviceStatus DeviceStatus { get; }

        #endregion Status

        #region Settings
        /// <summary>
        /// The state of the SafetyBypass
        /// </summary>
        SafetyBypassState SafetyBypass { get; set; }

        /// <summary>
        /// Settings that are managed by and saved on the device.
        /// These values are not valid until the device has connected and reported its settings.
        /// </summary>
        IDeviceSettings DeviceSettings { get; }

        /// <summary>
        /// The current drive profile.
        /// </summary>
        Profile Profile { get; set; }
        #endregion Settings

        #region Control
        /// <summary>
        /// Called to allow a movement in the same direction following the current movement.
        /// </summary>
        void Continue();

        /// <summary>
        /// Immediately stop all movements.
        /// </summary>
        void Stop();

        /// <summary>
        /// Nudge in the direction indicated.
        /// </summary>
        /// <param name="direction"></param>
        Task Nudge(XDirection direction);

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
        Task Move(Direction direction);

        /// <summary>
        /// Toggle the specefied relay. Repeat as desired.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the relay does not exist.</exception>
        /// <param name="relay">The relay to cycle.</param>
        /// <param name="repeat">The number of times to repeat the cycle. If 0, the relay will not be activated.</param>
        /// <param name="repeatDelayMs">The delay between repeated relay cycles.</param>
        Task CycleRelayAsync(uint relay, uint repeat = 1, uint repeatDelayMs = 0);
        #endregion Control
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IButtonDriver))]
        internal abstract class DriverContract : IButtonDriver
        {

            #region Connection

            public IBrainBoxConnection Connection
            {
                get
                {
                    Contract.Ensures(Contract.Result<IBrainBoxConnection>() != null);
                    return default(IBrainBoxConnection);
                }
            }

            #endregion Connection

            #region DeviceInfo
            public uint RelayCount
            {
                get
                {
                    //This may change in the future. But his makes sure that implementations dont break current expectations.
                    Contract.Ensures(Contract.Result<uint>() > 0);
                    return default(uint);
                }
            }
            #endregion DeviceInfo

            #region Status
            public abstract bool HardwareReady { get; }

            public ReadyState ReadyState
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(ReadyState), Contract.Result<ReadyState>()));
                    return default(ReadyState);
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

            public Direction LastDirection
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(Direction), Contract.Result<Direction>()));
                    return default(Direction);
                }
            }

            public ContinueState ContinueState
            {
                get
                {
                    Contract.Ensures(Enum.IsDefined(typeof(ContinueState), Contract.Result<ContinueState>()));
                    return default(ContinueState);
                }
            }

            public IDeviceStatus DeviceStatus
            {
                get
                {
                    Contract.Ensures(Contract.Result<IDeviceStatus>() != null);
                    return default(IDeviceStatus);
                }
            }

            #endregion Status

            #region Settings

            public SafetyBypassState SafetyBypass
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

            public IDeviceSettings DeviceSettings
            {
                get
                {
                    Contract.Ensures(Contract.Result<IDeviceSettings>() != null);
                    return default(IDeviceSettings);
                }
            }


            public Profile Profile
            {
                get
                {
                    Contract.Ensures(Contract.Result<Profile>() != null);
                    return default(Profile);
                }

                set
                {
                    Contract.Requires<ArgumentNullException>(value != null, nameof(value));
                }
            }

            #endregion Settings

            #region Control

            public abstract void Continue();

            public abstract void Stop();


            public Task Nudge(XDirection direction)
            {
                Contract.Requires(Enum.IsDefined(typeof(XDirection), direction));
                throw new NotImplementedException();
            }

            public bool CanMove(Direction direction)
            {
                Contract.Requires(Enum.IsDefined(typeof(Direction), direction));
                throw new NotImplementedException();
            }

            public Task Move(Direction direction)
            {
                Contract.Requires(Enum.IsDefined(typeof(Direction), direction));
                throw new NotImplementedException();
            }

            public Task CycleRelayAsync(uint relay, uint repeat = 1, uint repeatDelayMs = 0)
            {
                Contract.Requires<ArgumentOutOfRangeException>(relay > 0 && relay <= RelayCount, nameof(relay));
                Contract.Requires<ArgumentOutOfRangeException>(repeat > 0, nameof(repeat));
                throw new NotImplementedException();
            }

            #region IDisposable

            #endregion Control

            public abstract void Dispose();

            #endregion IDisposable

            public abstract event PropertyChangedEventHandler PropertyChanged;
        }
    }
}

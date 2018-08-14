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
using System.ComponentModel;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.Common;
using Eyedrivomatic.Device;
using Eyedrivomatic.Device.Communications;

namespace Eyedrivomatic.ButtonDriver.Services
{
    #region Enums
    public enum ReadyState { None, Any, Continue }

    public enum ContinueState { NotContinuedRecently, Continued}
    #endregion Enums

    /// <summary>
    /// This it the interface to the device hardware.
    /// </summary>
    public interface IButtonDriver : INotifyPropertyChanged
    {

        #region Status

        /// <summary>
        /// The device is currently sending valid status messages.
        /// </summary>
        bool DeviceReady { get; }

        /// <summary>
        /// Indicates which next actions are valid.
        /// </summary>
        ReadyState ReadyState { get; }

        /// <summary>
        /// The status of the device.
        /// </summary>
        IDeviceStatus DeviceStatus { get; }


        /// <summary>
        /// Indicates the connection state of the device.
        /// </summary>
        ConnectionState ConnectionState { get; }

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
        /// returns the number of switches on the device.
        /// </summary>
        uint SwitchCount { get; }
        #endregion Status

        #region Settings
        /// <summary>
        /// The current drive profile.
        /// </summary>
        Profile Profile { get; set; }

        #endregion Settings

        #region Control
        /// <summary>
        /// Toggle the specefied relay. Repeat as desired.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">If the relay does not exist.</exception>
        /// <param name="relay">The relay to cycle.</param>
        /// <param name="repeat">The number of times to repeat the cycle. If 0, the relay will not be activated.</param>
        /// <param name="toggleDelayMs">The delay between relay state toggles in a cycle.</param>
        /// <param name="repeatDelayMs">The delay between repeated relay cycles.</param>
        Task CycleSwitchAsync(uint relay, uint repeat = 1, uint toggleDelayMs = 500, uint repeatDelayMs = 1000);

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
        Task Nudge(XDirection direction, TimeSpan duration);

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
        /// <param name="duration">The time to move in that direction</param>
        Task Move(Direction direction, TimeSpan duration);
        #endregion Control
    }
}

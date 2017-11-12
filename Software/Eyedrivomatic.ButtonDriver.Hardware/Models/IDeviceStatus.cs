using System;
using System.ComponentModel;

namespace Eyedrivomatic.ButtonDriver.Hardware.Models
{
    /// <summary>
    /// Represents the stauts of the device.
    /// These values are not valid until the device has connected and reported it status.
    /// </summary>
    public interface IDeviceStatus : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Use this property to determine if the current status is known.
        /// The other status values have no meaning unless this property is true.
        /// </summary>
        /// <returns>
        /// bool - true if a valid status message has been receieved, 
        /// false if no status message has been received or if the last message was invalid.
        ///  </returns>
        bool IsKnown { get; }

        /// <summary>
        /// The Relative position of the X (left/right) servo.
        /// </summary>
        int XPosition { get; }

        /// <summary>
        /// The relative position of the Y (forward/backward) servo.
        /// </summary>
        int YPosition { get; }

        /// <summary>
        /// The on/off state of switch 1. True = on.
        /// </summary>
        bool Switch1 { get; }

        /// <summary>
        /// The on/off state of switch 2. True = on.
        /// </summary>
        bool Switch2 { get; }

        /// <summary>
        /// The on/off state of switch 3. True = on.
        /// </summary>
        bool Switch3 { get; }
    }
}
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
using Eyedrivomatic.Device.Commands;

namespace Eyedrivomatic.ButtonDriver.Device.Models
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
        /// The direction and speed applied to the wheelchair joystick.
        /// </summary>
        Vector Vector { get; }

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

        /// <summary>
        /// The on/off state of switch 4. True = on.
        /// </summary>
        bool Switch4 { get; }
    }
}
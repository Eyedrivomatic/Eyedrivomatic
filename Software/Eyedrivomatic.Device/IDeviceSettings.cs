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
using Eyedrivomatic.Device.Commands;

namespace Eyedrivomatic.Device
{
    public enum DeviceOrientation
    {
        Rotate0Deg = 0,
        Rotate90Deg = 90,
        Rotate180Deg = 180,
        Rotate270Deg = 270
    }

    public interface IDeviceSettings : INotifyPropertyChanged, IDisposable
    {
        //The physical limit for the hardware.
        decimal DeviceMaxSpeed { get; }

        /// <summary>
        /// Returns the number of relays (switches) on the device.
        /// </summary>
        uint SwitchCount { get; }

        Point? CenterPos { get; set; }
        decimal? MaxSpeed { get; set; }
        DeviceOrientation Orientation { get; set; }
        bool?[] SwitchDefaults { get; }

        Task<bool> Save();
    }
}
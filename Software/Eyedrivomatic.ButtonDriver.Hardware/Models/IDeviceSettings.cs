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

namespace Eyedrivomatic.ButtonDriver.Device.Models
{
    public interface IDeviceSettings : INotifyPropertyChanged, IDisposable
    {
        int DeviceMaxPosX { get; }
        int DeviceMinPosX { get; }
        int DeviceMaxPosY { get; }
        int DeviceMinPosY { get; }

        int? CenterPosX { get; set; }
        int? MinPosX { get; set; }
        int? MaxPosX { get; set; }

        int? CenterPosY { get; set; }
        int? MinPosY { get; set; }
        int? MaxPosY { get; set; }

        bool? Switch1Default { get; set; }
        bool? Switch2Default { get; set; }
        bool? Switch3Default { get; set; }
        bool? Switch4Default { get; set; }

        Task<bool> Save();
    }
}
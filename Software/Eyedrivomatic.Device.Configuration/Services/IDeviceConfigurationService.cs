﻿//	Copyright (c) 2018 Eyedrivomatic Authors
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

namespace Eyedrivomatic.Device.Configuration.Services
{
    public interface IDeviceConfigurationService : INotifyPropertyChanged
    {
        string Variant { get; }
        bool AutoConnect { get; set; }
        string ConnectionString { get; set; }

        TimeSpan CommandTimeout { get; set; }

        void Save();
        bool HasChanges { get; }
    }
}

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


using System.Collections.Generic;

namespace Eyedrivomatic.Device.Communications
{
    public interface IDeviceInfo
    {
        Dictionary<string, DeviceIdFilter> EyedrivomaticIds { get; }
        VersionInfo VerifyStartupMessage(string firstMessage);
    }
}
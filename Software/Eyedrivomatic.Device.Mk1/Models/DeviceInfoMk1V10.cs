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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Services;
using NullGuard;

namespace Eyedrivomatic.Device.Mk1.Models
{
    [Export(typeof(IDeviceInfo))]
    internal class DeviceInfoMk1V10 : IDeviceInfo
    {
        private static readonly string StartupMessage = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";

        [return:AllowNull]
        public VersionInfo VerifyStartupMessage(string firstMessage)
        {
            return string.CompareOrdinal(firstMessage.Substring(0, Math.Min(StartupMessage.Length, firstMessage.Length)), StartupMessage) == 0 
                ? new VersionInfo("Original", new Version(1, 0, 0, 0)) 
                : null;
        }

        public Dictionary<string, DeviceIdFilter> EyedrivomaticIds => ArduinoInfo.UnoDeviceIds;
    }
}
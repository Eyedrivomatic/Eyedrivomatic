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

namespace Eyedrivomatic.Device.Communications
{
    public class VersionInfo
    {
        public static VersionInfo Unknown = new VersionInfo("N/A", new Version(0,0,0));

        public VersionInfo(string model, Version version, string variant = null)
        {
            Model = model;
            Variant = variant ?? string.Empty;
            Version = version;
        }

        public string Model { get; }
        public string Variant { get; }
        public Version Version { get; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Variant)
                ? $"{Model} {Version.ToString(3)}"
                : $"{Model} {Version.ToString(3)} ({Variant} build)";
        }
    }

    public interface IElectronicHandDeviceInfo
    {
        Dictionary<string, DeviceIdFilter> EyedrivomaticIds { get; }
        VersionInfo VerifyStartupMessage(string firstMessage);
    }
}
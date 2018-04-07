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

namespace Eyedrivomatic.Hardware.Communications
{
    public class VersionInfo
    {
        public static VersionInfo Unknown = new VersionInfo(new Version(0,0,0));

        public VersionInfo(Version version, string variant = null)
        {
            Variant = variant ?? string.Empty;
            Version = version;
        }

        public string Variant { get; }
        public Version Version { get; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Variant) 
                ? Version.ToString(3)
                : $"{Version.ToString(3)} ({Variant} build)";
        }
    }

    public interface IElectronicHandDeviceInfo
    {
        Dictionary<string, HardwareIdFilter> EyedrivomaticIds { get; }
        VersionInfo VerifyStartupMessage(string firstMessage);
    }
}
using System;
using System.Collections.Generic;

namespace Eyedrivomatic.Hardware.Communications
{
    public interface IElectronicHandDeviceInfo
    {
        Dictionary<string, HardwareIdFilter> EyedrivomaticIds { get; }
        Version VerifyStartupMessage(string firstMessage);
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace Eyedrivomatic.Hardware.Communications
{
    [Export(typeof(IElectronicHandDeviceInfo))]
    internal class ElectronicHandDeviceInfoV10 : IElectronicHandDeviceInfo
    {
        private static readonly string StartupMessage = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";

        public Version VerifyStartupMessage(string firstMessage)
        {
            return string.CompareOrdinal(firstMessage.Substring(0, Math.Min(StartupMessage.Length, firstMessage.Length)), StartupMessage) == 0 ? new Version(1, 0, 0, 0) : null;
        }

        public Dictionary<string, HardwareIdFilter> EyedrivomaticIds => ArduinoInfo.UnoDeviceIds;
    }

    [Export(typeof(IElectronicHandDeviceInfo))]
    internal class ElectronicHandDeviceInfoV20 : IElectronicHandDeviceInfo
    {
        public Version VerifyStartupMessage(string firstMessage)
        {
            var regex = new Regex(@"START: Eyedrivomatic - version (?<Version>2\.(?<Minor>[0-9]+)\.(?<Build>[0-9]+)(\.(?<Revision>[0-9]+))?)");
            var match = regex.Match(firstMessage);
            return match.Success ? new Version(match.Groups["Version"].Value) : null;
        }

        public Dictionary<string, HardwareIdFilter> EyedrivomaticIds => ArduinoInfo.UnoDeviceIds;
    }
}
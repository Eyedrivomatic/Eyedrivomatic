using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Services;
using NullGuard;

namespace Eyedrivomatic.Device.Delta.Models
{
    [Export(typeof(IDeviceInfo))]
    internal class DeviceInfoDeltaV10 : IDeviceInfo
    {
        [return: AllowNull]
        public VersionInfo VerifyStartupMessage(string firstMessage)
        {
            var regex = new Regex(@"START: Eyedrivomatic Delta (\[(?<Variant>\w+)\] )?- version (?<Version>(?<Major>[0-9]+)\.(?<Minor>[0-9]+)\.(?<Build>[0-9]+)(\.(?<Revision>[0-9]+))?)");
            var match = regex.Match(firstMessage);
            if (!match.Success)
            {
                return null;
            }
            var version = new Version(match.Groups["Version"].Value);
            var variant = match.Groups["Variant"].Value;
            return new VersionInfo("Delta", version, variant);
        }

        public Dictionary<string, DeviceIdFilter> EyedrivomaticIds => ArduinoInfo.TeencyDeviceIds;
    }
}
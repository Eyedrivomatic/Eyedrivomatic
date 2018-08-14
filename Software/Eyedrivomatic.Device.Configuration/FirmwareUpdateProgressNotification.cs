using System;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Device.Communications;

namespace Eyedrivomatic.Device.Configuration
{
    public class FirmwareUpdateProgressNotification : ProgressNotification<double>, IFirmwareUpdateProgressNotification
    {
        public FirmwareUpdateProgressNotification(VersionInfo fromVersion, VersionInfo toVersion, IProgress<double> progress = null)
            : base(progress)
        {
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public VersionInfo FromVersion { get; }
        public VersionInfo ToVersion { get; }
    }
}
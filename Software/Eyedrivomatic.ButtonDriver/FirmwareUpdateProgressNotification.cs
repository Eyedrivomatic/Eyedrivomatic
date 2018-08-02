using System;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver
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
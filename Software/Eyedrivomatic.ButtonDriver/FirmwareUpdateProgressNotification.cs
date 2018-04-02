using System;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver
{
    public class FirmwareUpdateProgressNotification : ProgressNotification<double>, IFirmwareUpdateProgressNotification
    {
        public FirmwareUpdateProgressNotification(Version fromVersion, Version toVersion, IProgress<double> progress = null)
            : base(progress)
        {
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }

        public Version FromVersion { get; }
        public Version ToVersion { get; }
    }
}
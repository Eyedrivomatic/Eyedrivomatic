using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Device.Communications;

namespace Eyedrivomatic.Device.Configuration
{
    public interface IFirmwareUpdateProgressNotification : IProgressNotification<double>
    {
        VersionInfo FromVersion { get; }
        VersionInfo ToVersion { get; }
    }
}
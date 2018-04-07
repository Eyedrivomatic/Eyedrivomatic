using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver
{
    public interface IFirmwareUpdateProgressNotification : IProgressNotification<double>
    {
        VersionInfo FromVersion { get; }
        VersionInfo ToVersion { get; }
    }
}
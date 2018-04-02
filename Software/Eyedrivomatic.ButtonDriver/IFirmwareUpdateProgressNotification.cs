using System;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver
{
    public interface IFirmwareUpdateProgressNotification : IProgressNotification<double>
    {
        Version FromVersion { get; }
        Version ToVersion { get; }
    }
}
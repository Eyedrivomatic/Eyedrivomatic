using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Communications;

namespace Eyedrivomatic.Hardware.Services
{
    public interface IFirmwareUpdateService
    {
        IEnumerable<Version> GetAvailableFirmware(Version minVersion);
        bool HasFirmwareUpdate(IDeviceConnection connection, Version minVersion);
        Task UpdateLatestFirmwareAsync(IDeviceConnection connection, IProgress<double> progress = null);
        Task UpdateFirmwareAsync(IDeviceConnection connection, Version version, IProgress<double> progress = null);
    }
}
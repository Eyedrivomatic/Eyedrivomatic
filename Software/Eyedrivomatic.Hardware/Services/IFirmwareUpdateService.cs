using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Communications;

namespace Eyedrivomatic.Hardware.Services
{
    public interface IFirmwareUpdateService
    {
        IEnumerable<Version> GetAvailableFirmware();
        Version GetLatestVersion();
        Task<bool> UpdateFirmwareAsync(IDeviceConnection connection, Version version, bool required, IProgress<double> progress = null);
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Communications;

namespace Eyedrivomatic.Hardware.Services
{
    public interface IDeviceEnumerationService
    {
        IList<DeviceDescriptor> GetAvailableDevices(bool includeAllSerialDevices);
        Task<IDeviceConnection> DetectDeviceAsync(Version minVersion, CancellationToken cancellationToken);
    }
}
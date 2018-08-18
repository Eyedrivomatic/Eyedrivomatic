using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Eyedrivomatic.Device.Services
{
    public interface IDeviceFactory
    {
        IList<DeviceDescriptor> GetAvailableDevices();

        Task<IDevice> AutoConnectAsync(bool autoUpdateFirmware, CancellationToken cancellationToken);
        Task<IDevice> ConnectAsync(string connectionString, bool autoUpdateFirmware, CancellationToken cancellationToken);
    }
}
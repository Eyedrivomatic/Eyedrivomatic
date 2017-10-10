using Eyedrivomatic.Hardware.Services;

namespace Eyedrivomatic.Hardware.Communications
{
    public interface IElectronicHandConnectionFactory
    {
        IDeviceConnection CreateConnection(DeviceDescriptor device);
    }
}
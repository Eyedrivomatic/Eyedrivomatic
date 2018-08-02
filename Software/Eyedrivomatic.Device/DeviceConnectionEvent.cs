using Eyedrivomatic.Device.Communications;
using Prism.Events;

namespace Eyedrivomatic.Device
{
    public class DeviceConnectionEvent : PubSubEvent<ConnectionState>
    {}
}

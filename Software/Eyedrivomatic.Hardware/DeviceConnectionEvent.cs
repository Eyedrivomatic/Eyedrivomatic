using Eyedrivomatic.Hardware.Communications;
using Prism.Events;

namespace Eyedrivomatic.Hardware
{
    public class DeviceConnectionEvent : PubSubEvent<ConnectionState>
    {}
}

using System;
using System.ComponentModel.Composition;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;

namespace Eyedrivomatic.Device.Serial.Communications
{
    [Export]
    internal class DeviceCommandHandlerFactory
    {
        [Export("DeviceCommandHandlerFactory")]
        public virtual IDeviceCommandHandler CreateBrainBoxCommandHandler(IDeviceCommand command, Action<string> sendFunc)
        {
            return new DeviceCommandHandler(s => sendFunc(ChecksumProcessor.ApplyChecksum(s)), command);
        }
    }   
}

using System;

namespace Eyedrivomatic.Hardware.Commands
{
    internal abstract class DeviceCommandBase : IDeviceCommand
    {
        public abstract string Name { get; }

        public virtual int Retries { get; set; } = 3;

        public virtual TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromMilliseconds(500);

        public override string ToString()
        {
            throw new NotImplementedException("This should be overridden in the subclass.");
        }
    }
}

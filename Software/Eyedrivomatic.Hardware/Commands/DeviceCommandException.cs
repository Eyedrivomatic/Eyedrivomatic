using System;

namespace Eyedrivomatic.Hardware.Commands
{
    public class DeviceCommandException : Exception
    {
        internal IDeviceCommand Command { get; }

        internal DeviceCommandException(IDeviceCommand command, string message)
            : base(message)
        {
            Command = command;
        }

        internal DeviceCommandException(IDeviceCommand command, string message, Exception innerException)
            : base(message, innerException)
        {
            Command = command;
        }
    }
}
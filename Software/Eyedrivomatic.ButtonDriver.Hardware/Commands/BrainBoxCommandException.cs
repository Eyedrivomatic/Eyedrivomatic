using System;

namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    public class BrainBoxCommandException : Exception
    {
        internal IBrainBoxCommand Command { get; }

        internal BrainBoxCommandException(IBrainBoxCommand command, string message)
            : base(message)
        {
            Command = command;
        }

        internal BrainBoxCommandException(IBrainBoxCommand command, string message, Exception innerException)
            : base(message, innerException)
        {
            Command = command;
        }
    }
}
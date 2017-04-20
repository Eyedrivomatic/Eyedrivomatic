using System;
using System.Diagnostics.Contracts;

namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    public class BrainBoxCommandException : Exception
    {
        internal IBrainBoxCommand Command { get; }

        internal BrainBoxCommandException(IBrainBoxCommand command, string message)
            : base(message)
        {
            Contract.Requires<ArgumentNullException>(command != null, nameof(command));
            Command = command;
        }

        internal BrainBoxCommandException(IBrainBoxCommand command, string message, Exception innerException)
            : base(message, innerException)
        {
            Contract.Requires<ArgumentNullException>(command != null, nameof(command));
            Command = command;
        }
    }
}
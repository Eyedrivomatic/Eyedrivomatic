using System;

namespace Eyedrivomatic.Hardware.Commands
{
    public interface IDeviceCommand
    {
        /// <summary>
        /// A name that is used to describe the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The number of times the command should be sent before failing.
        /// Includes the first attempt.
        /// </summary>
        int Retries { get; set; }

        /// <summary>
        /// The timeout period for the command.
        /// This time should be greater than the connections send timeout and should account for other potential commands in the queue.
        /// </summary>
        TimeSpan DefaultTimeout { get; set; }

        /// <summary>
        /// Returns the command text as it is sent to the device.
        /// </summary>
        string ToString();

    }
}
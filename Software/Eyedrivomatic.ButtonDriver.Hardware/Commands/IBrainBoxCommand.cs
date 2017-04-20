using System;
using System.Diagnostics.Contracts;

namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    [ContractClass(typeof(Contracts.BrainBoxCommandContract))]
    public interface IBrainBoxCommand
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

    namespace Contracts
    {
        [ContractClassFor(typeof(IBrainBoxCommand))]
        internal abstract class BrainBoxCommandContract : IBrainBoxCommand
        {
            public string Name
            {
                get
                {
                    Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                    return default(string);
                }
            }

            public int Retries
            {
                get
                {
                    Contract.Ensures(Contract.Result<int>() > 0);
                    return default(int);
                }
                set
                {
                    Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                }
            }

            public TimeSpan DefaultTimeout
            {
                get
                {
                    Contract.Ensures(Contract.Result<TimeSpan>() >= TimeSpan.Zero);
                    return default(TimeSpan);
                }
                set
                {
                    Contract.Requires<ArgumentOutOfRangeException>(value >= TimeSpan.Zero);
                }
            }
        }
    }
}
using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Hardware.Communications.Contracts;

namespace Eyedrivomatic.ButtonDriver.Hardware.Communications
{
    [ContractClass(typeof(BrainBoxCommandHandlerContract))]
    public interface IBrainBoxCommandHandler
    {
        string Name { get; }
        Task<bool> CommandTask { get; }

        void StartTimeoutTimer(TimeSpan timeout);
        Task Send(TimeSpan timeout, CancellationToken cancellationToken);
        bool HandleResponse(bool success);
        void OnError(string message);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IBrainBoxCommandHandler))]
        internal abstract class BrainBoxCommandHandlerContract : IBrainBoxCommandHandler
        {
            public abstract string Name { get; }
            public Task<bool> CommandTask
            {
                get
                {
                    Contract.Ensures(Contract.Result<Task<bool>>() != null);
                    return default(Task<bool>);
                }
            }

            public Task Send(TimeSpan timeout, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public bool HandleResponse(bool success)
            {
                throw new NotImplementedException();
            }

            public void OnError(string message)
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(message), "Invalid message");
                throw new NotImplementedException();
            }

            public void StartTimeoutTimer(TimeSpan timeout)
            {
                Contract.Requires<ArgumentOutOfRangeException>(timeout >= TimeSpan.Zero);
                throw new NotImplementedException();
            }
        }
    }

}

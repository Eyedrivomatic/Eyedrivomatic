using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Hardware.Commands;
using Eyedrivomatic.ButtonDriver.Hardware.Services.Contracts;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    [ContractClass(typeof(BrainBoxCommandQueueContract))]
    internal interface IBrainBoxCommandQueue : IBrainBoxMessageProcessor
    {
        Task<bool> SendCommand(IBrainBoxCommand command);
    }

    namespace Contracts
    {
        [ContractClassFor(typeof(IBrainBoxCommandQueue))]
        internal abstract class BrainBoxCommandQueueContract : IBrainBoxCommandQueue
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public abstract void Attach(IObservable<char> source);

            public Task<bool> SendCommand(IBrainBoxCommand command)
            {
            //    Contract.Requires<ArgumentNullException>(command == null, nameof(command));
            //    Contract.Ensures(Contract.Result<Task<bool>>() != null);
                throw new NotImplementedException();
            }
        }
    }
}
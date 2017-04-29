using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eyedrivomatic.ButtonDriver.Hardware.Communications
{
    public interface IBrainBoxCommandHandler
    {
        string Name { get; }
        Task<bool> CommandTask { get; }

        void StartTimeoutTimer(TimeSpan timeout);
        Task Send(TimeSpan timeout, CancellationToken cancellationToken);
        bool HandleResponse(bool success);
        void OnError(string message);
    }
}

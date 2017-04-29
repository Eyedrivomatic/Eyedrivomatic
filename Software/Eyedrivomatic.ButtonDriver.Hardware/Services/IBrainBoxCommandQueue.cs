using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Hardware.Commands;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    internal interface IBrainBoxCommandQueue : IBrainBoxMessageProcessor
    {
        Task<bool> SendCommand(IBrainBoxCommand command);
    }
}
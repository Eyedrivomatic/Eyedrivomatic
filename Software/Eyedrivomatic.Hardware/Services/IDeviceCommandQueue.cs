using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Commands;

namespace Eyedrivomatic.Hardware.Services
{
    internal interface IDeviceCommandQueue : IBrainBoxMessageProcessor
    {
        Task<bool> SendCommand(IDeviceCommand command);
    }
}
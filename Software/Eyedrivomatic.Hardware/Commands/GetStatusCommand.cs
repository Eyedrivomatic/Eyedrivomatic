namespace Eyedrivomatic.Hardware.Commands
{
    internal class GetStatusCommand : DeviceCommandBase
    {
        public override string Name => "Get Status";

        public override string ToString()
        {
            return "STATUS";
        }
    }
}
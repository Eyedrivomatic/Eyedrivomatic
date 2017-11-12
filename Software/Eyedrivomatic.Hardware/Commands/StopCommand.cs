namespace Eyedrivomatic.Hardware.Commands
{
    internal class StopCommand : DeviceCommandBase
    {
        public override string Name => $"Stop";

        public override string ToString()
        {
            return "STOP";
        }
    }
}
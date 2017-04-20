namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    internal class StopCommand : BrainBoxCommand
    {
        public override string Name => $"Stop";

        public override string ToString()
        {
            return "STOP";
        }
    }
}
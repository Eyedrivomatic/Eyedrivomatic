namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    internal class GetStatusCommand : BrainBoxCommand
    {
        public override string Name => "Get Status";

        public override string ToString()
        {
            return "STATUS";
        }
    }
}
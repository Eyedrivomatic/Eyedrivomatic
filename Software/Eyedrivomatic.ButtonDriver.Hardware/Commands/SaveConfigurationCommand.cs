namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    internal class SaveConfigurationCommand : BrainBoxCommand
    {
        public override string Name => "Save Configuration";

        public override string ToString()
        {
            return "SAVE";
        }
    }
}
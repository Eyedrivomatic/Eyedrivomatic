namespace Eyedrivomatic.Hardware.Commands
{
    internal class SaveConfigurationCommand : DeviceCommandBase
    {
        public override string Name => "Save Configuration";

        public override string ToString()
        {
            return "SAVE";
        }
    }
}
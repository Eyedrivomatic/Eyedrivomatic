namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    internal class GetConfigurationCommand : BrainBoxCommand
    {
        public string SettingName { get; }

        internal GetConfigurationCommand(string settingName)
        {
            SettingName = settingName;
        }

        public override string Name => $"Get {SettingName}";

        public override string ToString()
        {
            return $"GET {SettingName}";
        }
    }
}
namespace Eyedrivomatic.ButtonDriver.Hardware.Commands
{
    internal class SetConfigurationCommand : BrainBoxCommand
    {
        public string SettingName { get; }
        public string Value { get; }

        internal SetConfigurationCommand(string settingName, string value)
        {
            SettingName = settingName;
            Value = value;
        }

        public override string Name => $"Set {SettingName}";

        public override string ToString()
        {
            return $"SET {SettingName} {Value}";
        }
    }
}
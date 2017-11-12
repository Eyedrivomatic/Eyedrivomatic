namespace Eyedrivomatic.Hardware.Commands
{
    internal class SetConfigurationCommand : DeviceCommandBase
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
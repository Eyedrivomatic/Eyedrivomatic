using System;

namespace Eyedrivomatic.Hardware.Services
{
    public interface ISettingsMessageSource
    {
        event EventHandler<SettingMessageEventArgs> SettingsMessageReceived;
        event EventHandler SettingsParseError;
        event EventHandler Disconnected;
    }

    public class SettingMessageEventArgs : EventArgs
    {
        public SettingMessageEventArgs(string settingName, string settingValue)
        {
            SettingName = settingName;
            SettingValue = settingValue;
        }

        public string SettingName { get; }
        public string SettingValue { get; }
    }
}
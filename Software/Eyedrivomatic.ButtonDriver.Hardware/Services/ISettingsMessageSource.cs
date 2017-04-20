using System;
using System.Diagnostics.Contracts;
using Eyedrivomatic.ButtonDriver.Hardware.Services.Contracts;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{

    [ContractClass(typeof(SettingsMessageSourceContract))]
    internal interface ISettingsMessageSource
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

    namespace Contracts
    {
        [ContractClassFor(typeof(ISettingsMessageSource))]
        internal abstract class SettingsMessageSourceContract : ISettingsMessageSource
        {
            public abstract void Dispose();

            public event EventHandler SettingsParseError;
            public event EventHandler<SettingMessageEventArgs> SettingsMessageReceived;
            public event EventHandler Disconnected;
        }
    }
}
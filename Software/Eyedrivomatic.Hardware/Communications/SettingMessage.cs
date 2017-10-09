using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Prism.Logging;

namespace Eyedrivomatic.ButtonDriver.Hardware.Communications
{
    public class SettingMessage : BrainBoxMessage
    {
        public override string MessagePrefix => "SETTING:";

        private SettingMessage() : base(true)
        {
        }

        public SettingMessage(string name, string value)
            : base(true)
        {
            Name = name;
            Value = value;
        }

        public static bool TryParse(string message, out SettingMessage settingMessage)
        {
            var messageFormat = new Regex(@"^(?<Name>.*)=(?<Value>.*)$");
            var match = messageFormat.Match(message);
            if (!match.Success)
            {
                ButtonDriverHardwareModule.Logger?.Log($"Unable to parse settings message - '{message}'.", Category.Exception, Priority.None);
                settingMessage = new SettingMessage();
                return false;
            }

            settingMessage = new SettingMessage(
                match.Groups["Name"].Value,
                match.Groups["Value"].Value);
            return true;
        }

        public string Name { get; }
        public string Value { get; }
    }
}

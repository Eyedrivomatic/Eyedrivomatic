//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


namespace Eyedrivomatic.Device.Commands
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
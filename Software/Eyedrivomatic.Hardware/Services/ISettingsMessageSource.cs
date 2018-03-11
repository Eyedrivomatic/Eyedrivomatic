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
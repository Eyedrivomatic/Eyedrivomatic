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


namespace Eyedrivomatic.Device.Serial.Communications
{
    public static class SettingNames
    {
        public const string All = @"ALL"; //used for 'GET' command only.
        public const string CenterPos = @"CENTER";
        public const string MaxSpeed = @"MAX_SPEED";
        public const string Orientation = @"ORENTIATION";
        public const string Switch1Default = @"SWITCH 1";
        public const string Switch2Default = @"SWITCH 2";
        public const string Switch3Default = @"SWITCH 3";
        public const string Switch4Default = @"SWITCH 4";
    }
}
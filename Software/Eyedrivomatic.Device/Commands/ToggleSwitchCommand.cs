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

namespace Eyedrivomatic.Device.Commands
{
    internal class ToggleSwitchCommand : DeviceCommandBase
    {
        internal ToggleSwitchCommand(uint switchNumber, TimeSpan duration)
        {
            SwitchNumber = switchNumber;
            Duration = duration;
        }

        public uint SwitchNumber { get; }
        public TimeSpan Duration { get; }

        public override string Name => $"Toggle Switch {SwitchNumber} for {Duration}";

        public override string ToString()
        {
            return $"SWITCH {SwitchNumber} {Duration.TotalMilliseconds}";
        }
    }
}
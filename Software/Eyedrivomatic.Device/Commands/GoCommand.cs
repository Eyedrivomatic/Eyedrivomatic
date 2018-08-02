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
    internal class GoCommand : DeviceCommandBase
    {
        internal GoCommand(double direction, double speed, TimeSpan duration)
        {
            Direction = direction;
            Speed = speed;
            Duration = duration;
        }

        public double Direction { get; }
        public double Speed { get; }
        public TimeSpan Duration { get; }

        public override string Name => "Go";

        public override int MaxAttempts => 1; //Move only gets 1 try

        public override string ToString()
        {
            return $"GO {Direction} {Speed} {Duration.TotalMilliseconds}";
        }
    }
}
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
    public interface IStatusMessageSource
    {
        event EventHandler<StatusMessageEventArgs> StatusMessageReceived;
        event EventHandler StatusParseError;
        event EventHandler Disconnected;
    }

    public class StatusMessageEventArgs : EventArgs
    {
        public int XRelative { get; }
        public double XAbsolute { get; }
        public int YRelative { get; }
        public double YAbsolute { get; }
        public bool Switch1 { get; }
        public bool Switch2 { get; }
        public bool Switch3 { get; }

        public StatusMessageEventArgs(int xRelative, double xAbsolute, int yRelative, double yAbsolute, bool switch1, bool switch2, bool switch3)
        {
            XRelative = xRelative;
            XAbsolute = xAbsolute;
            YRelative = yRelative;
            YAbsolute = yAbsolute;
            Switch1 = switch1;
            Switch2 = switch2;
            Switch3 = switch3;
        }

        public override string ToString()
        {
            return $"X:{XRelative} ({XAbsolute}), Y:{YRelative} ({YAbsolute}), Switch1:{(Switch1 ? "on" : "off")}, Switch2:{(Switch2 ? "on" : "off")}, Switch3:{(Switch3 ? "on" : "off")}, ";
        }
    }
}
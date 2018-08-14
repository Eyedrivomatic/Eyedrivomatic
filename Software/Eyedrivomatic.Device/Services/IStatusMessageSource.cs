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
using System.Text;
using Eyedrivomatic.Device.Commands;

namespace Eyedrivomatic.Device.Services
{
    public interface IStatusMessageSource
    {
        event EventHandler<StatusMessageEventArgs> StatusMessageReceived;
        event EventHandler StatusParseError;
        event EventHandler Disconnected;
    }

    public class StatusMessageEventArgs : EventArgs
    {
        public Vector Vector { get; }
        public bool[] Switches { get; }

        public StatusMessageEventArgs(Vector vector, bool[] switches)
        {
            Vector = vector;
            Switches = switches;
        }

        public StatusMessageEventArgs(Point position, bool[] switches)
        {
            Vector = position;
            Switches = switches;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var switchNumber = 1;

            sb.Append($"Vector:{Vector}");

            foreach (var switchState in Switches)
            {
                if (switchNumber > 1) sb.Append(",");
                sb.Append($" Switch{switchNumber++} {(switchState ? "on" : "off")}");
            }

            return sb.ToString();
        }
    }
}
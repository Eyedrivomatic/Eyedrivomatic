using System;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    internal interface IStatusMessageSource
    {
        event EventHandler<StatusMessageEventArgs> StatusMessageReceived;
        event EventHandler StatusParseError;
        event EventHandler Disconnected;
    }

    public class StatusMessageEventArgs : EventArgs
    {
        public int XRelative { get; }
        public int XAbsolute { get; }
        public int YRelative { get; }
        public int YAbsolute { get; }
        public bool Switch1 { get; }
        public bool Switch2 { get; }
        public bool Switch3 { get; }

        public StatusMessageEventArgs(int xRelative, int xAbsolute, int yRelative, int yAbsolute, bool switch1, bool switch2, bool switch3)
        {
            XRelative = xRelative;
            XAbsolute = xAbsolute;
            YRelative = yRelative;
            YAbsolute = yAbsolute;
            Switch1 = switch1;
            Switch2 = switch2;
            Switch3 = switch3;
        }
    }
}
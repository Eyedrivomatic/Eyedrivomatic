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


using Eyedrivomatic.ButtonDriver.Services;
using Eyedrivomatic.Common;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;

namespace Eyedrivomatic.ButtonDriver.UI.ViewModels
{
    public interface IStatusViewModel
    {
        ConnectionState ConnectionState { get; }
        Vector JoystickVector { get; }

        Direction CurrentDirection { get; }
        string Profile { get; }
        ReadyState ReadyState { get; }
        bool SafetyBypassStatus { get; }
        string Speed { get; }

        bool Switch1 { get; }
        bool Switch2 { get; }
        bool Switch3 { get; }
        bool Switch4 { get; }
    }

    class DesignStatusViewModel : IStatusViewModel
    {
        public ConnectionState ConnectionState => ConnectionState.Connecting;
        public Vector JoystickVector => new Vector(45, 50);
        public Direction CurrentDirection => Direction.BackwardRight;
        public string Profile => "Indoors";
        public ReadyState ReadyState => ReadyState.Continue;
        public bool SafetyBypassStatus => false;
        public string Speed => "Walk";
        public bool Switch1 => false;
        public bool Switch2 => true;
        public bool Switch3 => false;
        public bool Switch4 => false;
    }
}
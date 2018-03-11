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

namespace Eyedrivomatic.Hardware.Commands
{
    public class DeviceCommandException : Exception
    {
        internal IDeviceCommand Command { get; }

        internal DeviceCommandException(IDeviceCommand command, string message)
            : base(message)
        {
            Command = command;
        }

        internal DeviceCommandException(IDeviceCommand command, string message, Exception innerException)
            : base(message, innerException)
        {
            Command = command;
        }
    }
}
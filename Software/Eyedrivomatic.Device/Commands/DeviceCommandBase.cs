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
    internal abstract class DeviceCommandBase : IDeviceCommand
    {
        public abstract string Name { get; }

        public virtual int MaxAttempts => 3;

        public virtual TimeSpan DefaultTimeout => TimeSpan.FromMilliseconds(300);

        public override string ToString()
        {
            throw new NotImplementedException("This should be overridden in the subclass.");
        }
    }
}

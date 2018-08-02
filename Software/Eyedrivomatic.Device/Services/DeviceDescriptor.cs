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


namespace Eyedrivomatic.Device.Services
{
    public abstract class DeviceDescriptor
    {
        public string FriendlyName { get; set; }
        public string ConnectionString { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{FriendlyName} [{ConnectionString}]";
        }
    }
}
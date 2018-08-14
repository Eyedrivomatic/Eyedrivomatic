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


using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Services;

namespace Eyedrivomatic.Device.Serial.Communications
{
    [Export(typeof(IDeviceConnectionFactory))]
    public class DeviceConnectionFactory : IDeviceConnectionFactory
    {
        private readonly IList<IDeviceInfo> _infos;

        [ImportingConstructor]
        public DeviceConnectionFactory([ImportMany] IEnumerable<IDeviceInfo> infos)
        {
            _infos = infos.ToList();
        }

        public IDeviceConnection CreateConnection(DeviceDescriptor device)
        {
            return new DeviceConnection(_infos, device);
        }
    }
}
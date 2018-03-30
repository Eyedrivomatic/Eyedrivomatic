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
    internal class GetStatusCommand : DeviceCommandBase
    {
        public override string Name => "Get Status";

        public override int MaxAttempts => -1; //just keep trying.
        public override TimeSpan DefaultTimeout => TimeSpan.FromSeconds(3);

        public override string ToString()
        {
            return "STATUS";
        }
    }
}
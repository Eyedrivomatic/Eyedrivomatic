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

#include "HostConnectionService.h"
#include "Response.h"
#include "VersionInfo.h"
#include "SendStatusAction.h"

HostConnectionServiceClass::HostConnectionServiceClass()
{
	Serial.begin(19200);   //Begin arduino - pc communication
}


HostConnectionServiceClass::~HostConnectionServiceClass()
{
}

void HostConnectionServiceClass::MonitorConnection()
{
	if (Serial && !_available)
	{
		SendStartupInfo();
		SendStatusAction.execute(NULL);
	}
	_available = Serial;
}

bool HostConnectionServiceClass::IsAvailable()
{
	return _available;
}

void HostConnectionServiceClass::SendStartupInfo()
{
	Response.SendResponse_P(PSTR("START: %s - version %s"), VersionInfo::Application, VersionInfo::Version);
}

HostConnectionServiceClass HostConnectionService;
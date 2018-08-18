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
#include "State.h"

HostConnectionServiceClass::HostConnectionServiceClass()
{
	Serial.begin(19200);   //Begin arduino - pc communication
}


HostConnectionServiceClass::~HostConnectionServiceClass()
{
	Serial.end();
}

void HostConnectionServiceClass::MonitorConnection()
{
	if (Serial.available() != 0)
	{
		State.setServoEnabled(false);
		_dtrEnable = false;
		return;
	}

	auto dtr = Serial.dtr();
	if (!dtr && _dtrEnable)
	{
		SendStartupInfo();
		SendStatusAction.execute(NULL);
		SendStatusAction.execute("VECTOR");
		State.setServoEnabled(true);
	}

	_dtrEnable = dtr;
}

bool HostConnectionServiceClass::IsAvailable()
{
	return Serial.available() != 0;
}

void HostConnectionServiceClass::SendStartupInfo()
{
	Response.SendResponse_P(PSTR("START: %s %s - version %s"), VersionInfo::Application, VersionInfo::Model, VersionInfo::Version);
}

HostConnectionServiceClass HostConnectionService;
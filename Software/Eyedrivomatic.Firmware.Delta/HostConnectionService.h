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


// ResetDeviceService.h
#pragma once

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

class HostConnectionServiceClass
{
public:
	HostConnectionServiceClass();
	~HostConnectionServiceClass();

	void MonitorConnection();
	void SendStartupInfo();
	
	bool IsAvailable();
private:
	bool _dtrEnable = true;
	bool _connected = false;
};


extern HostConnectionServiceClass HostConnectionService;

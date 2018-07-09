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


#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "EnableLogAction.h"
#include "LoggerService.h"

void EnableLogActionClass::execute(const char * parameters)
{
	LogSeverity severity = LogSeverity::Debug;
	if (strlen(parameters) > 0) severity = LoggerService.serverityFromName(parameters);
	LoggerService.setLogLevel(severity);
}


EnableLogActionClass EnableLogAction;


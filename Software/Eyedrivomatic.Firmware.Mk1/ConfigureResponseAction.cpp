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


// 
// 
// 

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "ConfigureResponseAction.h"
#include "Message.h"
#include "LoggerService.h"

void ConfigureResponseActionClass::execute(const char * parameters)
{
	size_t paramLen = strlen(parameters);
	
	if (paramLen == 2 && strcmp_P(parameters, PSTR("ON")) == 0) Message.useAckResponses = true;
	else if (paramLen == 3 && strcmp_P(parameters, PSTR("OFF")) == 0) Message.useAckResponses = false;
	else if (paramLen == 0) Message.useAckResponses = !Message.useAckResponses;
	else (LoggerService.error_P(PSTR("Invalid parameters")));

	LoggerService.debug_P(PSTR("ACK/NAK Responses %s."), Message.useAckResponses ? "enabled" : "disabled");
}


ConfigureResponseActionClass ConfigureResponseAction;


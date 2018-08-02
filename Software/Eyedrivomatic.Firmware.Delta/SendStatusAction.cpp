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

#include "Response.h"
#include "SendStatusAction.h"
#include "State.h"
#include "LoggerService.h"
#include "Message.h"

#if defined(ARDUINO) && ARDUINO >= 100
  #include "arduino.h"
#else
  #include "WProgram.h"
#endif

void SendStatusActionClass::execute(const char * parameters)
{
	bool vector = strncmp_P(parameters, PSTR("VECTOR"), strlen_P(OnString)) == 0;

	static char messageBuffer[WRITE_BUFFER_SIZE];
	State.toString(vector, messageBuffer, WRITE_BUFFER_SIZE);
	Response.QueueResponse(messageBuffer);
}

SendStatusActionClass SendStatusAction;


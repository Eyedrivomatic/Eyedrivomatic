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

#include "ActionService.h"
#include "MessageProcessor.h"
#include "LoggerService.h"

void MessageProcessorClass::processMessage(MessageClass & message)
{
	ActionClass & action = ActionService::GetAction(message);

	if (action.expectChecksum() && !message.validateChecksum())
	{
		if (action.shouldRespond()) message.sendNak();
		Message.logMessage();
		return;
	}

	if (action.shouldRespond()) Message.sendAck();
	Message.logMessage();
	action.execute(ActionService::GetParameters(message));
}


MessageProcessorClass MessageProcessor;


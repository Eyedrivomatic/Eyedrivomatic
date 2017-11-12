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


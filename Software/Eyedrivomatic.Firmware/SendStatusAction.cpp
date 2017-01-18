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
	static char messageBuffer[WRITE_BUFFER_SIZE];
	State.toString(messageBuffer, WRITE_BUFFER_SIZE);
	Response.QueueResponse(messageBuffer);
}

SendStatusActionClass SendStatusAction;


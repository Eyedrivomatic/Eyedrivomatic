// 
// 
// 

#include "InvalidAction.h"
#include "LoggerService.h"
#include "Response.h"

const char InvalidActionResponse[] PROGMEM = "ERROR: INVALID COMMAND";

void InvalidActionClass::execute(const char * parameters)
{
	Response.QueueResponse_P(InvalidActionResponse);
}

InvalidActionClass InvalidAction;

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

#include "InvalidAction.h"
#include "LoggerService.h"
#include "Response.h"

const char InvalidActionResponse[] PROGMEM = "ERROR: INVALID COMMAND";

void InvalidActionClass::execute(const char * parameters)
{
	Response.QueueResponse_P(InvalidActionResponse);
}

InvalidActionClass InvalidAction;

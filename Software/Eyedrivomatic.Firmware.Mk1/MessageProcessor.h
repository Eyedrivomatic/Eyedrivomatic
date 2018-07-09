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


// MessageProcessor.h

#ifndef _MESSAGEPROCESSOR_h
#define _MESSAGEPROCESSOR_h

#include "Message.h"

class MessageProcessorClass
{
 public:
	void processMessage(MessageClass & message);
};

extern MessageProcessorClass MessageProcessor;

#endif


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


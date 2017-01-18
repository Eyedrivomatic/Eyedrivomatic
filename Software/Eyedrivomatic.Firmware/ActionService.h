// ActionFactory.h

#ifndef _ACTIONFACTORY_h
#define _ACTIONFACTORY_h

#include "Action.h"
#include "Message.h"

class ActionService
{
public:
	static ActionClass & GetAction(const MessageClass & message);
	static const char * GetParameters(const MessageClass & message);

private:
	static ActionClass & GetAction(const char * buffer);
	static const char * GetCommand(const char * buffer, size_t & commandLength);
	static const char * GetParameters(const char * buffer);

};

#endif


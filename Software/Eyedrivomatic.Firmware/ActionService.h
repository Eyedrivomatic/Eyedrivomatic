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


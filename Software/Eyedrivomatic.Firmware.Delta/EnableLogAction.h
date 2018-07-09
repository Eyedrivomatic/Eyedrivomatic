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


// EnableLogAction.h

#ifndef _ENABLELOGACTION_h
#define _ENABLELOGACTION_h

#include "Action.h"

class EnableLogActionClass : public ActionClass
{
 protected:
	 virtual void execute(const char * parameters);
};

extern EnableLogActionClass EnableLogAction;

#endif


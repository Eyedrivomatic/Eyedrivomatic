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


// GoAction.h

#pragma once

#include "Action.h"

class GoActionClass : public ActionClass
{
public:
	virtual void execute(const char * parameters);
	virtual void cancel(bool reset);

protected:
	static void timer_interupt();
};

extern GoActionClass GoAction;

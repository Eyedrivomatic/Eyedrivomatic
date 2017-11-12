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


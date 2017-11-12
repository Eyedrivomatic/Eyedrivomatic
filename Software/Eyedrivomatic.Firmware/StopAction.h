// StopAction.h

#ifndef _RESETACTION_h
#define _RESETACTION_h

#include "Action.h"

class StopActionClass : public ActionClass
{
public:

	virtual void execute(const char * parameters) override;
};

extern StopActionClass StopAction;

#endif


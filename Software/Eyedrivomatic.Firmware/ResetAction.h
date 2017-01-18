// ResetAction.h

#ifndef _RESETACTION_h
#define _RESETACTION_h

#include "Action.h"

class ResetActionClass : public ActionClass
{
public:

	virtual void execute(const char * parameters) override;
};

extern ResetActionClass ResetAction;

#endif


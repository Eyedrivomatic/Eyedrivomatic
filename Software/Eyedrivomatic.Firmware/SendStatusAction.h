// SendStatusAction.h

#ifndef _SENDSTATUSACTION_h
#define _SENDSTATUSACTION_h

#include "Action.h"

class SendStatusActionClass : public ActionClass
{
public:
	virtual void execute(const char * parameters) override;
};

extern SendStatusActionClass SendStatusAction;

#endif

// NoAction.h

#ifndef _NoACTION_h
#define _NoACTION_h

#include "Action.h"

class NoActionClass : public ActionClass
{
public:
	virtual void execute(const char * parameters) override;
};

extern NoActionClass NoAction;

#endif


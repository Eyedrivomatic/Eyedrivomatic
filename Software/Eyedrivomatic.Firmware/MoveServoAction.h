// MoveServoAction.h

#ifndef _MOVESERVOACTION_h
#define _MOVESERVOACTION_h

#include "Action.h"

class MoveServoActionClass : public ActionClass
{
public:
	virtual void execute(const char * parameters);
	virtual void cancel(bool reset);

protected:
	static void timer_interupt();
};

extern MoveServoActionClass MoveServoAction;

#endif


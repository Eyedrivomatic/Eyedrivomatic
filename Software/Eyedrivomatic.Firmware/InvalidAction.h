// InvalidAction.h

#ifndef _INVALIDACTION_h
#define _INVALIDACTION_h

#include "Action.h"

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

class InvalidActionClass : public ActionClass
{
public:
	virtual void execute(const char * parameters) override;
};

extern InvalidActionClass InvalidAction;

#endif


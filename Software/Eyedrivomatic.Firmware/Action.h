// Action.h

#ifndef _ACTION_h
#define _ACTION_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

class ActionClass
{
 public:
	 virtual bool expectChecksum() { return true; }
	 virtual bool shouldRespond() { return true; }
	 
	 virtual void execute(const char * parameters) = 0;
};

#endif
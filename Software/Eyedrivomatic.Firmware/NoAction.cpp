// 
// 
// 

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "NoAction.h"
#include "LoggerService.h"
#include "Response.h"

void NoActionClass::execute(const char * parameters)
{
	//Do nothing... obviously. 
}

NoActionClass NoAction;

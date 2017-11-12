#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "EnableLogAction.h"
#include "LoggerService.h"

void EnableLogActionClass::execute(const char * parameters)
{
	LogSeverity severity = LogSeverity::Debug;
	if (strlen(parameters) > 0) severity = LoggerService.serverityFromName(parameters);
	LoggerService.setLogLevel(severity);
}


EnableLogActionClass EnableLogAction;


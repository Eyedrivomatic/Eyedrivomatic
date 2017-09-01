// 
// 
// 

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "SaveSettingsAction.h"
#include "Settings.h"
#include "LoggerService.h"


void SaveSettingsActionClass::execute(const char * parameters)
{
	Settings.save();
}

SaveSettingsActionClass SaveSettingsAction;

// GetSettingAction.h

#ifndef _SETTINGACTIONBASE_H
#define _SETTINGACTIONBASE_H

#include "Action.h"
#include "State.h"

class SettingActionBaseClass : public ActionClass
{
public:
	virtual void execute(const char * parameters) override = 0;

protected:
	static const char * getSettingName(const char * parameters, size_t & size);
	static bool getHardwareSwitch(const char ** parameters, HardwareSwitch & hardwareSwitch);
};

#endif


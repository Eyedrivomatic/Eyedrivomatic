//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


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


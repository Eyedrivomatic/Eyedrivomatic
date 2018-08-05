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

#ifndef _GETSETTINGACTION_H
#define _GETSETTINGACTION_H

#include "Action.h"
#include "SettingActionBase.h"

class GetSettingActionClass : public SettingActionBaseClass
{
 public:
	virtual void execute(const char * parameters) override;

	static void getCenter(const char * parameters);
	static void getMaxSpeed(const char * parameters);
	static void getOrientation(const char * parameters);
	static void getSwitch(const char * parameters);
	static void getAll(const char * parameters);
};

extern GetSettingActionClass GetSettingAction;

#endif


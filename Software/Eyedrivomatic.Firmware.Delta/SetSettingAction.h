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


// SetSettingAction.h

#ifndef _SETSETTINGACTION_H
#define _SETSETTINGACTION_H

#include "Action.h"
#include "SettingActionBase.h"

class SetSettingActionClass : public SettingActionBaseClass
{
 public:
	virtual void execute(const char * parameters) override;

	static void setXMin(const char * parameters);
	static void setXCenter(const char * parameters);
	static void setXMax(const char * parameters);
	static void setXInvert(const char * parameters);
	static void setYMin(const char * parameters);
	static void setYCenter(const char * parameters);
	static void setYMax(const char * parameters);
	static void setYInvert(const char * parameters);
	static void setSwitch(const char * parameters);
	static void setDefaults(const char * parameters);
};

extern SetSettingActionClass SetSettingAction;

#endif


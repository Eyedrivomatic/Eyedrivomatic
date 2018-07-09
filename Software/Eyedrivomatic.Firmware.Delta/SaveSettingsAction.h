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


// SaveSettingAction.h

#ifndef _SAVESETTINGSACTION_H
#define _SAVESETTINGSACTION_H

#include "Action.h"

class SaveSettingsActionClass : public ActionClass
{
public:
	virtual void execute(const char * parameters) override;
};

extern SaveSettingsActionClass SaveSettingsAction;

#endif


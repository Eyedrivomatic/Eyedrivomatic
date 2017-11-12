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


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


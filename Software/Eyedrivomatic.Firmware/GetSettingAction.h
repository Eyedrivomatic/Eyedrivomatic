// GetSettingAction.h

#ifndef _GETSETTINGACTION_H
#define _GETSETTINGACTION_H

#include "Action.h"
#include "SettingActionBase.h"

class GetSettingActionClass : public SettingActionBaseClass
{
 public:
	virtual void execute(const char * parameters) override;

	static void getXMin(const char * parameters);
	static void getXCenter(const char * parameters);
	static void getXMax(const char * parameters);
	static void getXInvert(const char * parameters);
	static void getYMin(const char * parameters);
	static void getYCenter(const char * parameters);
	static void getYMax(const char * parameters);
	static void getYInvert(const char * parameters);
	static void getSwitch(const char * parameters);
	static void getAll(const char * parameters);
};

extern GetSettingActionClass GetSettingAction;

#endif


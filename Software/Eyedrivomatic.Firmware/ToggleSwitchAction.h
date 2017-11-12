// ToggleSwitchAction.h

#ifndef _TOGGLESWITCHACTION_H
#define _TOGGLESWITCHACTION_H

#include "Action.h"
#include "State.h"
#include "TimerService.h"

class ToggleSwitchActionClass : public ActionClass
{
public:
	virtual void execute(const char * parameters);
	virtual void cancel(HardwareSwitch hardwareSwitch, bool reset);
	virtual void cancel_all(bool reset);

protected:
	static TimerCallback CallbackRoutines[];
	static void switch1_timer_interupt();
	static void switch2_timer_interupt();
	static void switch3_timer_interupt();
};

extern ToggleSwitchActionClass ToggleSwitchAction;

#endif


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
	static void switch4_timer_interupt();
};

extern ToggleSwitchActionClass ToggleSwitchAction;

#endif


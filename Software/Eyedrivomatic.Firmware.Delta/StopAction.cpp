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


// 
// 
// 


#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "StopAction.h"
#include "SendStatusAction.h"
#include "MoveAction.h"
#include "State.h"
#include "ToggleSwitchAction.h"


void StopActionClass::execute(const char * parameters)
{
	//cancel the servo and switch timers. Doesn't reset the servo or switch. That will be done by the state object. Prevents a response.
	MoveAction.cancel(false); 
	ToggleSwitchAction.cancel_all(false); 
	State.reset();
}


StopActionClass StopAction;

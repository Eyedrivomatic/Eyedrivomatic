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

#include "LoggerService.h"
#include "Response.h"
#include "SendStatusAction.h"
#include "State.h"
#include "Settings.h"
#include "ToggleSwitchAction.h"

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "TimerService.h"


#define CancelWithError(msg, ...) Response.SendResponse_P(msg, ##__VA_ARGS__); cancel_all(true); return;

TimerCallback ToggleSwitchActionClass::CallbackRoutines[] =
{
	ToggleSwitchActionClass::switch1_timer_interupt,
	ToggleSwitchActionClass::switch2_timer_interupt,
	ToggleSwitchActionClass::switch3_timer_interupt,
	ToggleSwitchActionClass::switch4_timer_interupt
};

//Parameters:
// D N S
// Where
//  D = Duration in milliseconds. Unsigned value from 0 to 10000;
//  N = Switch number from 1 to 3
//  S = State, ON or OFF (switch closed or open)
// Resets all switch positions if duration or switch number is out of range.
void ToggleSwitchActionClass::execute(const char * parameters)
{
	if (parameters == NULL) { CancelWithError(PSTR("ERROR: MISSING PARAMETERS")); }

	auto startPos = parameters;
	char* endPos;

	auto duration = strtoul(startPos, &endPos, 10);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING DURATION")); }
	if (duration < 0 || duration > 10000) { CancelWithError(PSTR("ERROR: DURATION OUT OF RANGE %d"), duration); }

	auto switchNum = strtol(startPos = endPos, &endPos, 10)-1;
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING SWITCH NUMBER")); }
	if (switchNum < HardwareSwitch::Switch1 || switchNum > HardwareSwitch::Switch4) { CancelWithError(PSTR("ERROR: SWITCH NUMBER OUT OF RANGE %d"), switchNum+1); }

	HardwareSwitch hardwareSwitch = static_cast<HardwareSwitch>(switchNum);
	cancel(hardwareSwitch, false);
	State.setSwitchState(hardwareSwitch, !Settings.DefaultSwitchStates[hardwareSwitch]);

	if (duration == 0) return;
	TimerService.addTimer(CallbackRoutines[hardwareSwitch], static_cast<unsigned long>(duration));
}

void ToggleSwitchActionClass::cancel_all(bool reset)
{
	cancel(HardwareSwitch::Switch1, reset);
	cancel(HardwareSwitch::Switch2, reset);
	cancel(HardwareSwitch::Switch3, reset);
	cancel(HardwareSwitch::Switch4, reset);
}


void ToggleSwitchActionClass::cancel(HardwareSwitch hardwareSwitch, bool reset)
{
	if (hardwareSwitch < HardwareSwitch::Switch1 || hardwareSwitch > HardwareSwitch::Switch4) return;
	TimerService.removeTimer(CallbackRoutines[hardwareSwitch]);
	if (reset) State.setSwitchState(hardwareSwitch, Settings.DefaultSwitchStates[hardwareSwitch]);
}

//static
void ToggleSwitchActionClass::switch1_timer_interupt()
{
	LoggerService.shouldQueueLogs(true);

	ToggleSwitchAction.cancel(HardwareSwitch::Switch1, true);

	LoggerService.shouldQueueLogs(false);
}

void ToggleSwitchActionClass::switch2_timer_interupt()
{
	LoggerService.shouldQueueLogs(true);

	ToggleSwitchAction.cancel(HardwareSwitch::Switch2, true);

	LoggerService.shouldQueueLogs(false);
}

void ToggleSwitchActionClass::switch3_timer_interupt()
{
	LoggerService.shouldQueueLogs(true);

	ToggleSwitchAction.cancel(HardwareSwitch::Switch3, true);

	LoggerService.shouldQueueLogs(false);
}

void ToggleSwitchActionClass::switch4_timer_interupt()
{
	LoggerService.shouldQueueLogs(true);

	ToggleSwitchAction.cancel(HardwareSwitch::Switch4, true);

	LoggerService.shouldQueueLogs(false);
}

ToggleSwitchActionClass ToggleSwitchAction;


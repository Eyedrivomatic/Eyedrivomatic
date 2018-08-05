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
#include "MoveAction.h"
#include "SendStatusAction.h"
#include "Response.h"
#include "State.h"

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#include "TimerService.h"

#define CancelWithError(msg, ...) Response.SendResponse_P( msg, ##__VA_ARGS__); cancel(true); return;

//Parameters:
// D X Y
// Where each pair is a Hex byte and
//  D = Duration in milliseconds. Unsigned value from 0 to 10000;
//  X = Position of X servo from -100.0 to 100.0. 
//  Y = Position of Y servo from -100.0 to 100.0. 
// Resets servo positions if duration or servo position is out of range.
void MoveActionClass::execute(const char * parameters)
{
	if (parameters == NULL) { CancelWithError(PSTR("ERROR: MISSING PARAMETERS")); }

	const char* startPos = parameters;
	char* endPos;

	double xPos = strtof(startPos, &endPos);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING X VALUE")); }
	if (-100.0f > xPos || 100.0f < xPos) { CancelWithError(PSTR("ERROR: X VALUE OUT OF RANGE %.1f"), xPos); }

	if (*endPos == ',') endPos++;

	double yPos = strtof(startPos = endPos, &endPos);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING Y VALUE")); }
	if (-100.0f > yPos || 100.0f < yPos) { CancelWithError(PSTR("ERROR: Y VALUE OUT OF RANGE %.1f"), yPos); }

	long duration = strtoul(startPos = endPos, &endPos, 10);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING DURATION")); }
	if (duration < 0 || duration > 10000) { CancelWithError(PSTR("ERROR: DURATION OUT OF RANGE %d"), duration); }

	LoggerService.debug_P(PSTR("Moving to %.1f,%.1f for %d ms"), xPos, yPos, duration);
	cancel(false);

	State.setPosition(xPos, yPos);

	if (duration == 0) return; 
	TimerService.addTimer(timer_interupt, static_cast<unsigned long>(duration));
}

void MoveActionClass::cancel(bool reset)
{
	TimerService.removeTimer(timer_interupt);
	if (reset) State.resetServoPositions();
}

//static
void MoveActionClass::timer_interupt()
{
	LoggerService.shouldQueueLogs(true);

	MoveAction.cancel(true);

	LoggerService.shouldQueueLogs(false);
}

MoveActionClass MoveAction;


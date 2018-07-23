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
#include "GoAction.h"
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
// Dir Speed Duration
// Where each pair is a Hex byte and
//  Dir = Direction of movement from -180.0 to 180.0. 
//  Speed = Magnitude of movement from 0 to 100. 
//  D = Duration in milliseconds. Unsigned value from 0 to 10000;
// Resets servo positions if duration or servo position is out of range.
void GoActionClass::execute(const char * parameters)
{
	if (parameters == NULL) { CancelWithError(PSTR("ERROR: MISSING PARAMETERS")); }

	const char* startPos = parameters;
	char* endPos;

	auto direction = strtof(startPos, &endPos);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING DIRECTION")); }
	//if (-180.0f > direction || 180.0f < direction) { CancelWithError(PSTR("ERROR: DIRECTION OUT OF RANGE %f"), direction); }
	//direction = fmod(direction, 180);

	auto speed = strtof(startPos = endPos, &endPos);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING SPEED")); }
	if (0 > speed || 100.0f < speed) { CancelWithError(PSTR("ERROR: SPEED OUT OF RANGE %3.1f"), speed); }

	long duration = strtoul(startPos = endPos, &endPos, 10);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING DURATION")); }
	if (duration < 0 || duration > 10000) { CancelWithError(PSTR("ERROR: DURATION OUT OF RANGE %d"), duration); }

	LoggerService.debug_P(PSTR("Moving %.1f, at %.1f for %d ms"), direction, speed, duration);
	cancel(false);

	State.setVector(direction, speed);

	if (duration == 0) return; 
	TimerService.addTimer(timer_interupt, static_cast<unsigned long>(duration));
}

void GoActionClass::cancel(bool reset)
{
	TimerService.removeTimer(timer_interupt);
	if (reset) State.resetServoPositions();
}

//static
void GoActionClass::timer_interupt()
{
	LoggerService.shouldQueueLogs(true);

	GoAction.cancel(true);

	LoggerService.shouldQueueLogs(false);
}

GoActionClass GoAction;


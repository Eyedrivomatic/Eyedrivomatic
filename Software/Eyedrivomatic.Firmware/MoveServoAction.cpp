// 
// 
// 

#include "LoggerService.h"
#include "MoveServoAction.h"
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
//  X = Position of X servo from -100 to 100. 
//  Y = Position of Y servo from -100 to 100. 
// Resets servo positions if duration or servo position is out of range.
void MoveServoActionClass::execute(const char * parameters)
{
	if (parameters == NULL) { CancelWithError(PSTR("ERROR: MISSING PARAMETERS")); }

	const char* startPos = parameters;
	char* endPos;

	long duration = strtoul(startPos, &endPos, 10);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING DURATION")); }
	if (duration < 0 || duration > 10000) { CancelWithError(PSTR("ERROR: DURATION OUT OF RANGE %d"), duration); }

	long xPos = strtol(startPos = endPos, &endPos, 10);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING XPOS")); }
	if (-100 > xPos || 100 < xPos) { CancelWithError(PSTR("ERROR: XPOS OUT OF RANGE %d"), xPos); }

	long yPos = strtol(startPos = endPos, &endPos, 10);
	if (endPos == startPos) { CancelWithError(PSTR("ERROR: MISSING YPOS")); }
	if (-100 > yPos || 100 < yPos) { CancelWithError(PSTR("ERROR: YPOS OUT OF RANGE %d"), yPos); }

	cancel(false);

	State.setServoPositionsRelative(xPos, yPos);

	if (duration == 0) return; 
	TimerService.addTimer(timer_interupt, static_cast<unsigned long>(duration));
}

void MoveServoActionClass::cancel(bool reset)
{
	TimerService.removeTimer(timer_interupt);
	if (reset) State.resetServoPositions();
}

//static
void MoveServoActionClass::timer_interupt()
{
	LoggerService.shouldQueueLogs(true);

	MoveServoAction.cancel(true);

	LoggerService.shouldQueueLogs(false);
}

MoveServoActionClass MoveServoAction;


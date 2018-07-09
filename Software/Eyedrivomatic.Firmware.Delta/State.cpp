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


#include "LoggerService.h"
#include "Response.h"
#include "SendStatusAction.h"
#include "State.h"
#include "Settings.h"

#define SendStatus() SendStatusAction.execute(NULL)

#define SERVO_OFFSET_X 90
#define SERVO_OFFSET_Y 90

#if defined(MOBILITY_CONCEPT_BUILD)
#define SERVO_X 6
#define SERVO_Y 5
#define SWITCH_1 4
#define SWITCH_2 8
#define SWITCH_3 12
#define SERVO_ENABLE 7
#else
#define SERVO_X 9
#define SERVO_Y 8
#define SWITCH_1 7
#define SWITCH_2 5
#define SWITCH_3 3
#define SERVO_ENABLE 6
#endif


const int switchPins[] =
{
	SWITCH_1,
	SWITCH_2,
	SWITCH_3,
};


void StateClass::init()
{
	pinMode(SERVO_ENABLE, OUTPUT);
	pinMode(SERVO_X, OUTPUT);
	pinMode(SERVO_Y, OUTPUT);
	digitalWrite(SERVO_ENABLE, LOW);
	digitalWrite(SERVO_X, LOW);
	digitalWrite(SERVO_Y, LOW);

	pinMode(switchPins[HardwareSwitch::Switch1], OUTPUT);
	pinMode(switchPins[HardwareSwitch::Switch2], OUTPUT);
	pinMode(switchPins[HardwareSwitch::Switch3], OUTPUT);
}

void StateClass::reset()
{
	digitalWrite(SERVO_ENABLE, LOW);
	digitalWrite(switchPins[HardwareSwitch::Switch1], LOW);
	digitalWrite(switchPins[HardwareSwitch::Switch2], LOW);
	digitalWrite(switchPins[HardwareSwitch::Switch3], LOW);
	resetServoPositions(); //queues a status message.
	digitalWrite(SERVO_ENABLE, HIGH);
}

#define MAP(x, in_min, in_max, out_min, out_max) ((map(x, in_min, in_max, out_min*2, out_max*2) + (x >= 0 ? 1 : -1)) / 2)
#define INVERT_US(value) (MAX_PULSE_WIDTH - value + MIN_PULSE_WIDTH)

int AbsoluteMicroseconds(int8_t value, bool invert)
{
	value = constrain(value, 0, 180);

	int uS = static_cast<int>(MAP(value, 0, 180, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH));
	if (invert) uS = INVERT_US(uS);
	return uS;
}

float MicrosecondsToAboslute(int value, bool invert)
{
	if (invert) value = INVERT_US(value);
	return MAP(value, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH, 0, 1800)/10.0;
}

int RelativeMicroseconds(int8_t value, int8_t minValue, int8_t centerValue, int8_t maxValue, bool invert)
{
	value = constrain(value, -100, 100);
	
	return static_cast<int>((value >= 0)
		? MAP(value, 0, 100, AbsoluteMicroseconds(centerValue, invert), AbsoluteMicroseconds(maxValue, invert))
		: MAP(-value, 0, 100, AbsoluteMicroseconds(centerValue, invert), AbsoluteMicroseconds(minValue, invert)));
}

int8_t MicrosecondsToRelative(int us, int8_t minValue, int8_t centerValue, int8_t maxValue, bool invert)
{
	us = constrain(us, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH);

	int centerUs = AbsoluteMicroseconds(centerValue, invert);

	return static_cast<int8_t>((us >= centerUs) != invert
		? MAP(us, centerUs, AbsoluteMicroseconds(maxValue, invert), 0, 100)
		: -MAP(us, centerUs, AbsoluteMicroseconds(minValue, invert), 0, 100));
}

void StateClass::getServoPositionsRelative(int8_t & xPos, int8_t & yPos)
{
	xPos = MicrosecondsToRelative(
		xServo.readMicroseconds(),
		Settings.MinPos_X + SERVO_OFFSET_X, 
		Settings.CenterPos_X + SERVO_OFFSET_X, 
		Settings.MaxPos_X + SERVO_OFFSET_X,
		Settings.Invert_X);

	yPos = MicrosecondsToRelative(
		yServo.readMicroseconds(),
		Settings.MinPos_Y + SERVO_OFFSET_Y, 
		Settings.CenterPos_Y + SERVO_OFFSET_Y, 
		Settings.MaxPos_Y + SERVO_OFFSET_Y,
		Settings.Invert_Y);
}

void StateClass::getServoPositions(float & xPos, float & yPos)
{
	//Servo.read() adds 1 to the pulse width. It messes up the rounding and makes testing difficult.
	//So we need to do the conversion ourselves.
	xPos = MicrosecondsToAboslute(xServo.readMicroseconds(), Settings.Invert_X) - SERVO_OFFSET_X;
	yPos = MicrosecondsToAboslute(yServo.readMicroseconds(), Settings.Invert_Y) - SERVO_OFFSET_Y;
}

void StateClass::setServoPositionsRelative(int8_t xPos, int8_t yPos)
{
	int xUs = RelativeMicroseconds(xPos, 
		Settings.MinPos_X + SERVO_OFFSET_X, 
		Settings.CenterPos_X + SERVO_OFFSET_X, 
		Settings.MaxPos_X + SERVO_OFFSET_X,
		Settings.Invert_X);

	int yUs = RelativeMicroseconds(yPos, 
		Settings.MinPos_Y + SERVO_OFFSET_Y, 
		Settings.CenterPos_Y + SERVO_OFFSET_Y, 
		Settings.MaxPos_Y + SERVO_OFFSET_Y,
		Settings.Invert_Y);

	if (!xServo.attached()) xServo.attach(SERVO_X);
	if (!yServo.attached()) yServo.attach(SERVO_Y);

	xServo.writeMicroseconds(xUs);
	yServo.writeMicroseconds(yUs);

	SendStatus();
}

void StateClass::setServoPositions(int8_t xPos, int8_t yPos)
{
	xPos = constrain(xPos, Settings.MinPos_X, Settings.MaxPos_X);
	yPos = constrain(yPos, Settings.MinPos_Y, Settings.MaxPos_Y);

	if (!xServo.attached()) xServo.attach(SERVO_X);
	if (!yServo.attached()) yServo.attach(SERVO_Y);

	if (Settings.Invert_X) xPos = -xPos;
	if (Settings.Invert_Y) yPos = -yPos;

	xServo.write(SERVO_OFFSET_X + xPos);
	yServo.write(SERVO_OFFSET_Y + yPos);

	SendStatus();
}

void StateClass::resetServoPositions()
{
	setServoPositionsRelative(0, 0);
}

bool StateClass::getSwitchState(HardwareSwitch hardwareSwitch)
{
	if (hardwareSwitch < 0 || hardwareSwitch > sizeof(switchPins)) return false;
	return digitalRead(switchPins[hardwareSwitch]);
}

void StateClass::setSwitchState(HardwareSwitch hardwareSwitch, bool state)
{
	if (hardwareSwitch < 0 || hardwareSwitch > sizeof(switchPins)) return;

	digitalWrite(switchPins[hardwareSwitch], state ? HIGH : LOW);
	SendStatus();
}

size_t StateClass::toString(char * buffer, size_t size)
{
	int8_t xPos, yPos;
	getServoPositionsRelative(xPos, yPos);
	float xPosAbs, yPosAbs;
	getServoPositions(xPosAbs, yPosAbs);

	char xStrAbs[6];
	char yStrAbs[6];

	dtostrf(xPosAbs, 3, 1, xStrAbs);
	dtostrf(yPosAbs, 3, 1, yStrAbs);

	return snprintf_P(buffer, size,
		PSTR("STATUS: SERVO_X=%i(%s),SERVO_Y=%i(%s),%s=%s,%s=%s,%s=%s"), 
		//PSTR("STATUS: SERVO_X=%i(%i),SERVO_Y=%i(%i),%S=%S,%S=%S,%S=%S"),
		xPos, xStrAbs, yPos, yStrAbs,
		//xPos, xServo.readMicroseconds(), yPos, yServo.readMicroseconds(),
		HardwareSwitchNames[HardwareSwitch::Switch1], getSwitchState(HardwareSwitch::Switch1) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch2], getSwitchState(HardwareSwitch::Switch2) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch3], getSwitchState(HardwareSwitch::Switch3) ? OnString : OffString
	);
}


StateClass State;


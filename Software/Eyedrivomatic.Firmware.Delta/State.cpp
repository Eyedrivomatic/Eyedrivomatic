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
#include "ServoPositionConverter.h"


#define SendStatus() SendStatusAction.execute(NULL)
//#define MAP(x, in_min, in_max, out_min, out_max) ((map(x, in_min, in_max, out_min*2, out_max*2) + (x >= 0 ? 1 : -1)) / 2)
#define MAP(x, in_min, in_max, out_min, out_max) (map(x, in_min, in_max, out_min, out_max))


#define SERVO_X 7
#define SERVO_Y 8

#define SERVO_OFFSET_X 90.0f
#define SERVO_OFFSET_Y 90.0f

#define SWITCH_1 7
#define SWITCH_2 5
#define SWITCH_3 3
#define SERVO_ENABLE 6


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

int AngleToMicroseconds(float value)
{
	value = constrain(value, 0, 180);
	return (int)round(MAP(value, 0.0f, 180.0f, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH));
}

float MicrosecondsToAngle(int value)
{
	value = constrain(value, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH);
	auto angle = MAP((float)value, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH, 0, 180);
	return angle;
}

void StateClass::getPosition(float & xPos, float & yPos)
{
	float xServoAngle = MicrosecondsToAngle(xServo.readMicroseconds()) - SERVO_OFFSET_X;
	float yServoAngle = MicrosecondsToAngle(yServo.readMicroseconds()) - SERVO_OFFSET_Y;
	ServoPositionConverter.getCartesianFromServo(xServoAngle, yServoAngle, xPos, yPos);
}

void StateClass::setPosition(float xPos, float yPos)
{
	float xServoAngle, yServoAngle;
	ServoPositionConverter.getServoPosFromCartesian(xPos, yPos, xServoAngle, yServoAngle);

	int xUs = AngleToMicroseconds(xServoAngle + SERVO_OFFSET_X);
	int yUs = AngleToMicroseconds(yServoAngle + SERVO_OFFSET_Y);

	if (!xServo.attached()) xServo.attach(SERVO_X);
	if (!yServo.attached()) yServo.attach(SERVO_Y);

	xServo.writeMicroseconds(xUs);
	yServo.writeMicroseconds(yUs);

	SendStatus();
}

void StateClass::resetServoPositions()
{
	setPosition(0, 0);
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
	float xPos, yPos;
	getPosition(xPos, yPos);
	float xServoAngle = MicrosecondsToAngle(xServo.readMicroseconds()) - SERVO_OFFSET_X;
	float yServoAngle = MicrosecondsToAngle(yServo.readMicroseconds()) - SERVO_OFFSET_Y;

	return snprintf_P(buffer, size,
		PSTR("STATUS: POS=%3.1f,%3.1f(%3.1f,%3.1f),%s=%s,%s=%s,%s=%s"), 
		xPos, yPos, xServoAngle, yServoAngle,
		HardwareSwitchNames[HardwareSwitch::Switch1], getSwitchState(HardwareSwitch::Switch1) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch2], getSwitchState(HardwareSwitch::Switch2) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch3], getSwitchState(HardwareSwitch::Switch3) ? OnString : OffString
	);
}

StateClass State;


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
#include "DeltaPositionConverter.h"


#define SendStatus(vector) SendStatusAction.execute(vector ? "VECTOR" : NULL)


#define SERVO_LEFT 7
#define SERVO_RIGHT 8

#define SWITCH_1 20
#define SWITCH_2 14
#define SWITCH_3 21
#define SWITCH_4 15
#define SERVO_ENABLE 10

#define MAX_MOVE_ABS 16.0L

const int switchPins[] =
{
	SWITCH_1,
	SWITCH_2,
	SWITCH_3,
	SWITCH_4,
};


void StateClass::init()
{
	pinMode(SERVO_ENABLE, INPUT_DISABLE);
	pinMode(SERVO_LEFT, OUTPUT);
	pinMode(SERVO_RIGHT, OUTPUT);
	//digitalWrite(SERVO_ENABLE, LOW);
	digitalWrite(SERVO_LEFT, LOW);
	digitalWrite(SERVO_RIGHT, LOW);

	pinMode(switchPins[HardwareSwitch::Switch1], OUTPUT);
	pinMode(switchPins[HardwareSwitch::Switch2], OUTPUT);
	pinMode(switchPins[HardwareSwitch::Switch3], OUTPUT);
	pinMode(switchPins[HardwareSwitch::Switch4], OUTPUT);
}

void StateClass::reset()
{
	digitalWrite(switchPins[HardwareSwitch::Switch1], LOW);
	digitalWrite(switchPins[HardwareSwitch::Switch2], LOW);
	digitalWrite(switchPins[HardwareSwitch::Switch3], LOW);
	digitalWrite(switchPins[HardwareSwitch::Switch4], LOW);
	resetServoPositions(); //queues a status message.
	//digitalWrite(SERVO_ENABLE, LOW);
	SendStatus(true);
}

void StateClass::getServoEnabled(bool & enable)
{
	enable = true; // digitalRead(SERVO_ENABLE);
}

void StateClass::setServoEnabled(bool enable)
{
	//digitalWrite(SERVO_ENABLE, enable ? HIGH : LOW);
}


int AngleToMicroseconds(double angle)
{
	angle = constrain(angle, -90, 90);
	return static_cast<int>(round(map(angle, 90, -90, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH)));
}

double MicrosecondsToAngle(int uS)
{
	uS = constrain(uS, MIN_PULSE_WIDTH, MAX_PULSE_WIDTH);
	return map(static_cast<double>(uS), MIN_PULSE_WIDTH, MAX_PULSE_WIDTH, 90, -90);
}

void StateClass::getPosition(double & xPos, double & yPos)
{
	if (!leftServo.attached()) leftServo.attach(SERVO_LEFT);
	if (!rightServo.attached()) rightServo.attach(SERVO_RIGHT);

	double leftAngle = MicrosecondsToAngle(leftServo.readMicroseconds());
	double rightAngle = MicrosecondsToAngle(rightServo.readMicroseconds());
	DeltaPositionConverter.getCartesianFromServo(leftAngle, rightAngle, xPos, yPos);
}

void StateClass::setPosition(double xPos, double yPos)
{
	double leftAngle, rightAngle;
	DeltaPositionConverter.getServoPosFromCartesian(xPos, yPos, leftAngle, rightAngle);

	int leftUs = AngleToMicroseconds(leftAngle);
	int rightUs = AngleToMicroseconds(rightAngle);

	if (!leftServo.attached()) leftServo.attach(SERVO_LEFT);
	if (!rightServo.attached()) rightServo.attach(SERVO_RIGHT);

	leftServo.writeMicroseconds(leftUs);
	rightServo.writeMicroseconds(rightUs);

	SendStatus(false);
}

void StateClass::getVector(double & direction, double & speed)
{
	if (!leftServo.attached()) leftServo.attach(SERVO_LEFT);
	if (!rightServo.attached()) rightServo.attach(SERVO_RIGHT);

	double leftAngle = MicrosecondsToAngle(leftServo.readMicroseconds());
	double rightAngle = MicrosecondsToAngle(rightServo.readMicroseconds());
	DeltaPositionConverter.getVectorFromServo(leftAngle, rightAngle, direction, speed);
}

void StateClass::setVector(double direction, double speed)
{
	double leftAngle, rightAngle;
	DeltaPositionConverter.getServoPosFromVector(direction, speed, leftAngle, rightAngle);

	int leftUs = AngleToMicroseconds(leftAngle);
	int rightUs = AngleToMicroseconds(rightAngle);

	if (!leftServo.attached()) leftServo.attach(SERVO_LEFT);
	if (!rightServo.attached()) rightServo.attach(SERVO_RIGHT);

	leftServo.writeMicroseconds(leftUs);
	rightServo.writeMicroseconds(rightUs);
	SendStatus(true);
}


void StateClass::resetServoPositions()
{
	setVector(0, 0);
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
	SendStatus(true);
}

size_t StateClass::toString(bool vector, char * buffer, size_t size)
{
	if (!vector)
	{
		double xPos, yPos;
		getPosition(xPos, yPos);

		return snprintf_P(buffer, size,
			PSTR("STATUS: POS=%3.1f,%3.1f,%s=%s,%s=%s,%s=%s,%s=%s"),
			xPos, yPos,
			HardwareSwitchNames[HardwareSwitch::Switch1], getSwitchState(HardwareSwitch::Switch1) ? OnString : OffString,
			HardwareSwitchNames[HardwareSwitch::Switch2], getSwitchState(HardwareSwitch::Switch2) ? OnString : OffString,
			HardwareSwitchNames[HardwareSwitch::Switch3], getSwitchState(HardwareSwitch::Switch3) ? OnString : OffString,
			HardwareSwitchNames[HardwareSwitch::Switch4], getSwitchState(HardwareSwitch::Switch4) ? OnString : OffString);
	}
	
	double direction, speed;
	getVector(direction, speed);

	return snprintf_P(buffer, size,
		PSTR("STATUS: VECTOR=%3.1f,%3.1f,%s=%s,%s=%s,%s=%s,%s=%s"),
		direction, speed,
		HardwareSwitchNames[HardwareSwitch::Switch1], getSwitchState(HardwareSwitch::Switch1) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch2], getSwitchState(HardwareSwitch::Switch2) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch3], getSwitchState(HardwareSwitch::Switch3) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch4], getSwitchState(HardwareSwitch::Switch4) ? OnString : OffString);
}

void StateClass::getSpeedLimit(double & maxSpeed)
{
	DeltaPositionConverter.getLimits(maxSpeed);
}

void StateClass::getCenterLimit(double & min_x, double & max_x, double & min_y, double & max_y)
{
	DeltaPositionConverter.getLimits(min_x, max_x, min_y, max_y);
}



StateClass State;


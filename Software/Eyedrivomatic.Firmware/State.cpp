// 
// 
// 
#include "LoggerService.h"
#include "Response.h"
#include "SendStatusAction.h"
#include "State.h"
#include "Settings.h"

#define SERVO_X 9
#define SERVO_Y 8

#define SendStatus() SendStatusAction.execute(NULL)

static int switchPins[] =
{
	7, //Switch1
	5, //Switch2
	3, //Switch3
};

#define SERVO_ENABLE 6

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

#define REL_POS(value, minValue, centerValue, maxValue) static_cast<int8_t>((value >= centerValue) \
	? (map(value, centerValue, maxValue, 0, 1000) + 5) / 10\
	: (map(value, centerValue, minValue, 0, -1000) - 5) / 10)\

#define ABS_POS(value, minResult, centerResult, maxResult) static_cast<int8_t>((value >= 0) \
	? (map(value, 0, 100, centerResult*10, maxResult*10) + 5) / 10 \
	: (map(value, 0, -100, centerResult*10, minResult*10) + 5) / 10)\

void StateClass::getServoPositionsRelative(int8_t & xPos, int8_t & yPos)
{
	uint8_t absXPos;
	uint8_t absYPos;

	getServoPositions(absXPos, absYPos);

	xPos = REL_POS(absXPos, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	yPos = REL_POS(absYPos, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);
}

void StateClass::getServoPositions(uint8_t & xPos, uint8_t & yPos)
{
	xPos = xServo.read();
	yPos = yServo.read();
}

void StateClass::setServoPositionsRelative(int8_t xPos, int8_t yPos)
{
	uint8_t absXPos = ABS_POS(xPos, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	uint8_t absYPos = ABS_POS(yPos, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);

	setServoPositions(absXPos, absYPos);
}

void StateClass::setServoPositions(uint8_t xPos, uint8_t yPos)
{
	xPos = max(min(xPos, Settings.MaxPos_X), Settings.MinPos_X);
	yPos = max(min(yPos, Settings.MaxPos_Y), Settings.MinPos_Y);

	if (!xServo.attached()) xServo.attach(SERVO_X);
	if (!yServo.attached()) yServo.attach(SERVO_Y);

	xServo.write(xPos);
	yServo.write(yPos);

	SendStatus();
}

void StateClass::resetServoPositions()
{
	setServoPositions(Settings.CenterPos_X, Settings.CenterPos_Y);
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
	uint8_t xPosAbs, yPosAbs;
	getServoPositions(xPosAbs, yPosAbs);

	return snprintf_P(buffer, size,
		PSTR("STATUS: SERVO_X=%i(%03u),SERVO_Y=%i(%03u),%S=%S,%S=%S,%S=%S"), 
		xPos, xPosAbs, yPos, yPosAbs,
		HardwareSwitchNames[HardwareSwitch::Switch1], getSwitchState(HardwareSwitch::Switch1) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch2], getSwitchState(HardwareSwitch::Switch2) ? OnString : OffString,
		HardwareSwitchNames[HardwareSwitch::Switch3], getSwitchState(HardwareSwitch::Switch3) ? OnString : OffString
	);
}


StateClass State;


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


// State.h

#pragma once

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
	#include <Servo.h>
#else
	#include "WProgram.h"
#endif

enum HardwareSwitch { Switch1, Switch2, Switch3, Switch4 };

const char SwitchName_1[] PROGMEM = "SWITCH 1";
const char SwitchName_2[] PROGMEM = "SWITCH 2";
const char SwitchName_3[] PROGMEM = "SWITCH 3";
const char SwitchName_4[] PROGMEM = "SWITCH 4";
const char * const HardwareSwitchNames[] = { SwitchName_1, SwitchName_2, SwitchName_3, SwitchName_4 };

const char OnString[] PROGMEM = "ON";
const char OffString[] PROGMEM = "OFF";

class StateClass
{
public:
	void init();
	void reset();

	void getPosition(double & xPos, double & yPos);
	void setPosition(double xPos, double yPos);

	void getVector(double & direction, double & speed);
	void setVector(double direction, double speed);

	void resetServoPositions();

	bool getSwitchState(HardwareSwitch hardwareSwitch);
	void setSwitchState(HardwareSwitch hardwareSwitch, bool state);

	void getSpeedLimit(double & max_speed);
	void getCenterLimit(double & min_x, double & max_x, double & min_y, double & max_y);

	size_t toString(bool vector, char * buffer, size_t size);

private:
	Servo leftServo;  // servo object to control the x servo
	Servo rightServo;  // servo object to control the y servo
};

extern StateClass State;


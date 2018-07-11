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

#include "ServoPositionConverter.h"

#include "LoggerService.h"
#include "Settings.h"


ServoPositionConverterClass::ServoPositionConverterClass()
{
}


ServoPositionConverterClass::~ServoPositionConverterClass()
{
}

#define MAP(x, in_min, in_max, out_min, out_max) (map(x, in_min, in_max, out_min, out_max))


float posToServo(float value, float minValue, float centerValue, float maxValue)
{
	value = constrain(value, -100, 100);

	return value >= 0
		? MAP(value, 0.0f, 100.0f, centerValue, maxValue)
		: MAP(-value, 0.0f, 100.0f, centerValue, minValue);
}

float servoToPos(float value, float minValue, float centerValue, float maxValue)
{
	value = constrain(value, 0, 180);

	return value >= centerValue
		? MAP(value, centerValue, maxValue, 0, 100)
		: MAP(-value, centerValue, minValue, 0, -100);
}

void ServoPositionConverterClass::getServoPosFromCartesian(float x, float y, float & servo_x, float & servo_y)
{
	servo_x = posToServo(x, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	servo_y = posToServo(y, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);
}

void ServoPositionConverterClass::getCartesianFromServo(float servo_x, float servo_y, float & x, float & y)
{
	x = servoToPos(servo_x, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	y = servoToPos(servo_y, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);
}

void ServoPositionConverterClass::getServoPosFromVector(float theta, float mag, float & servo_x, float & servo_y)
{
	servo_x = theta;
	servo_y = mag;
}

void ServoPositionConverterClass::getVectorFromServo(float servo_x, float servo_y, float & theta, float mag)
{
	mag = servo_x;
	theta = servo_y;
}

ServoPositionConverterClass ServoPositionConverter;
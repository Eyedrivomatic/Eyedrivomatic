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

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif


#include "GimbalPositionConverter.h"

#include "LoggerService.h"
#include "Settings.h"

#define SERVO_OFFSET_X 90.0f
#define SERVO_OFFSET_Y 90.0f

#define SERVO_MAX_ANGLE 22
#define SERVO_MIN_ANGLE -22


GimbalPositionConverterClass::GimbalPositionConverterClass()
{
}


GimbalPositionConverterClass::~GimbalPositionConverterClass()
{
}

#define MAP(x, in_min, in_max, out_min, out_max) (map(x, in_min, in_max, out_min, out_max))


float toAbsolutePosition(float value, float minValue, float centerValue, float maxValue)
{
	value = constrain(value, -100, 100);

	return value >= 0
		? MAP(value, 0.0f, 100.0f, centerValue, maxValue)
		: MAP(-value, 0.0f, 100.0f, centerValue, minValue);
}

float toRelativePosition(float value, float minValue, float centerValue, float maxValue)
{
	value = constrain(value, 0, 180);

	return value >= centerValue
		? MAP(value, centerValue, maxValue, 0, 100)
		: MAP(-value, centerValue, minValue, 0, -100);
}

void GimbalPositionConverterClass::getServoPosFromCartesian(float x, float y, float & servo_x, float & servo_y)
{
	servo_x = toAbsolutePosition(x, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X) - SERVO_OFFSET_X;
	servo_y = toAbsolutePosition(y, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y) - SERVO_OFFSET_Y;
}

void GimbalPositionConverterClass::getCartesianFromServo(float servo_x, float servo_y, float & x, float & y)
{
	x = toRelativePosition(servo_x + SERVO_OFFSET_X, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	y = toRelativePosition(servo_y + SERVO_OFFSET_Y, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);
}

void GimbalPositionConverterClass::getServoPosFromVector(float theta, float mag, float & servo_x, float & servo_y)
{
	servo_x = theta;
	servo_y = mag;
}

void GimbalPositionConverterClass::getVectorFromServo(float servo_x, float servo_y, float & theta, float mag)
{
	mag = servo_x;
	theta = servo_y;
}

void GimbalPositionConverterClass::getLimits(int8_t & servo_x_min, int8_t & servo_x_max, int8_t & servo_y_min, int8_t & servo_y_max)
{
	servo_x_min = SERVO_MIN_ANGLE;
	servo_x_max = SERVO_MAX_ANGLE;

	servo_y_min = SERVO_MIN_ANGLE;
	servo_y_max = SERVO_MAX_ANGLE;
}

GimbalPositionConverterClass GimbalPositionConverter;
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


// ServoPositionConverter.h
#pragma once

class DeltaPositionConverterClass
{
public:
	DeltaPositionConverterClass();
	~DeltaPositionConverterClass();

	void getServoPosFromCartesian(float x, float y, float & servo_x, float & servo_y);
	void getCartesianFromServo(float servo_x, float servo_y, float & x, float & y);

	void getServoPosFromVector(float theta, float mag, float & servo_x, float & servo_y);
	void getVectorFromServo(float servo_x, float servo_y, float & theta, float & mag);

	void getLimits(int8_t & min_x, int8_t & max_x, int8_t & min_y, int8_t & max_y);
};

extern DeltaPositionConverterClass DeltaPositionConverter;


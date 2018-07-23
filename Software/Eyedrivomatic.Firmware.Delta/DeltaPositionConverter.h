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

	void getServoPosFromCartesian(double x, double y, double & leftAngle, double & rightAngle);
	void getCartesianFromServo(double rightAngle, double leftAngle, double & x, double & y);

	void getServoPosFromVector(double direction, double speed, double & leftAngle , double & rightAngle);
	void getVectorFromServo(double leftAngle, double rightAngle, double & direction, double & speed);

	void getLimits(double & min_x, double & max_x, double & min_y, double & max_y);
	void getLimits(double & maxSpeed);
};

extern DeltaPositionConverterClass DeltaPositionConverter;


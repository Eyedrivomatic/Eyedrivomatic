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


#include "DeltaPositionConverter.h"

#include "LoggerService.h"
#include "Settings.h"

#define ARM_LENGTH 24 //length in millimeters of arm attached to servo.
#define FOREARM_LENGTH 35 //in millimeters of arm attached to joystick.
#define ENDPOINT_OFFSET_X 20 //distance in millimeters from joystick axis to forearm attachment point
#define ENDPOINT_OFFSET_Y 20 //distance in millimeters from joystick axis to forearm attachment point
#define SERVO_OFFSET_X 16 //distance between servo rotation point and centerline.
#define SERVO_OFFSET_Y -30 //distance between the imaginary line connecting servo rotation points and the center of the joystick.

#define MAX_POS 50
#define MIN_POS -50

DeltaPositionConverterClass::DeltaPositionConverterClass()
{
}


DeltaPositionConverterClass::~DeltaPositionConverterClass()
{
}

float toAbsolutePosition(float value, float minValue, float centerValue, float maxValue)
{
	value = constrain(value, -100, 100);

	return value >= 0
		? map(value, 0, 100, centerValue, maxValue)
		: map(-value, 0, 100, centerValue, minValue);
}

float toRelativePosition(float value, float minValue, float centerValue, float maxValue)
{
	value = constrain(value, 0, 180);

	return value >= centerValue
		? map(value, centerValue, maxValue, 0, 100)
		: map(-value, centerValue, minValue, 0, -100);
}

struct point 
{
	float x;
	float y;
};

/// Calculates the intersection of two circles. 
/// Returns a float[2][2] if there is a solution or NULL otherwise.
/// The result is ordered left to right.
/// based on the math here:
/// http://math.stackexchange.com/a/1367732
///  and the algorithm here by Jared Updike:
/// https://gist.github.com/jupdike/bfe5eb23d1c395d8a0a1a4ddd94882ac
///
point * calculateCircleIntersection(point p1, float r1, point p2, float r2)
{
	static point result[2];

	auto dx = p1.x - p2.x;
	auto dy = p1.y - p2.y;
	auto R = sqrtf(sq(dx) + sq(dy));
	if (r1 + r2 > R || abs(r1 - r2) > R)
	{
		// no intersection
		LoggerService.error_P(PSTR("Circles [%3.3f, %3.3f, r%3.3f], [%3.3f, %3.3f, r%3.3f] do not intersect."), p1.x, p1.y, r1, p2.x, p2.y, r2);
		return NULL;
	}

	// intersection(s) should exist
	auto r1sq = sq(r1);
	auto r2sq = sq(r2);
	auto R2 = sq(R);
	auto R4 = sq(R2);
	auto a = (r1sq - r2sq) / (2 * R2);
	auto b = sqrt(2 * (r1sq + r2sq) / R2 - sq(r1sq - r2sq) / R4 - 1);

	auto fx = (p1.x + p2.x) / 2 + a * (p2.x - p1.x);
	auto gx = b * (p2.y - p1.y) / 2;
	result[0].x = fx - gx;
	result[1].x = fx + gx;

	auto fy = (p1.y + p2.y) / 2 + a * (p1.y - p2.y);
	auto gy = b * (p1.x - p2.x) / 2;

	// note if gy == 0 and gx == 0 then the circles are tangent and there is only one solution
	// but that one solution will just be duplicated as the code is currently written
	result[0].y = fy - gy;
	result[1].y = fy + gy;

	return result;
}

const float calculateAngle(point target, point servo, point joystick)
{
	auto intersections = calculateCircleIntersection(servo, ARM_LENGTH, target, FOREARM_LENGTH);
	if (intersections == NULL) return 0;

	auto m = (servo.x < 0) ? intersections[0] : intersections[1];
	auto angle = degrees(atan((m.x - servo.x) / (m.y - servo.y)));
	return (servo.x < 0 ? 180 - angle : angle);
}

void calculatePosition(float leftAngle, float rightAngle, point servo, point joystick, point & position)
{
	auto leftRad = radians(180 - leftAngle);
	point left;
	left.x = cosf(leftRad) * ARM_LENGTH - servo.x;
	left.y = sinf(leftRad) * ARM_LENGTH + servo.y;

	auto rightRad = radians(rightAngle);
	point right;
	right.x = cosf(rightRad) * ARM_LENGTH + servo.x;
	right.y = sinf(rightRad) * ARM_LENGTH + servo.y;

	//account for joystick offset.
	left.x += joystick.x; left.y -= joystick.y;
	right.x -= joystick.x; right.y -= joystick.y;

	auto intersections = calculateCircleIntersection(left, FOREARM_LENGTH, right, FOREARM_LENGTH);

	if (intersections == NULL)
	{
		LoggerService.error_P(PSTR("Servo angles [%3.3f, %3.3f] do not yield a solution."), leftAngle, rightAngle);
		position.x = 0; position.y = 0;
		return;
	}

	//x should be the same. But rounding errors may make them a little different. Let's average.
	position.x = (intersections[0].x + intersections[1].x) / 2;
	position.y = max(intersections[0].y, intersections[1].y);
}


void DeltaPositionConverterClass::getServoPosFromCartesian(float x, float y, float & leftServoAngle, float & rightServoAngle)
{
	auto runtime = millis();
	point target;
	target.x = toAbsolutePosition(x, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	target.y = toAbsolutePosition(y, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);

	point servoOffset;
	servoOffset.x = -SERVO_OFFSET_X;
	servoOffset.y = SERVO_OFFSET_Y;

	point joystickOffset;
	joystickOffset.x = -ENDPOINT_OFFSET_X;
	joystickOffset.y = ENDPOINT_OFFSET_Y;

	rightServoAngle = calculateAngle(target, servoOffset, joystickOffset);

	servoOffset.x = -servoOffset.x;
	joystickOffset.y = -joystickOffset.x;
	leftServoAngle = 180 - calculateAngle(target, servoOffset, joystickOffset);
	
	runtime = millis() - runtime;
	LoggerService.debug_P(PSTR("getServoPosFromCartesian took [%d] milliseconds to run."), runtime);
}


void DeltaPositionConverterClass::getCartesianFromServo(float servo_left, float servo_right, float & x, float & y)
{
	auto runtime = millis();
	point servoOffset;
	servoOffset.x = -SERVO_OFFSET_X;
	servoOffset.y = SERVO_OFFSET_Y;

	point joystickOffset;
	joystickOffset.x = -ENDPOINT_OFFSET_X;
	joystickOffset.y = ENDPOINT_OFFSET_Y;

	point position;

	calculatePosition(servo_left, servo_right, servoOffset, joystickOffset, position);
	
	x = toRelativePosition(position.x, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	y = toRelativePosition(position.y, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);

	runtime = millis() - runtime;
	LoggerService.debug_P(PSTR("getCartesianFromServo took [%d] milliseconds to run."), runtime);
}

void DeltaPositionConverterClass::getServoPosFromVector(float theta, float mag, float & servo_x, float & servo_y)
{
	servo_x = theta;
	servo_y = mag;
}

void DeltaPositionConverterClass::getVectorFromServo(float servo_x, float servo_y, float & theta, float & mag)
{
	mag = servo_x;
	theta = servo_y;
}


void DeltaPositionConverterClass::getLimits(int8_t& min_x, int8_t & max_x, int8_t & min_y, int8_t & max_y)
{
	float temp;
	float min,max;
	getCartesianFromServo(0, 180, min, temp);
	getCartesianFromServo(180, 0, max, temp);
	min_x = static_cast<int8_t>(ceil(min));
	max_x = static_cast<int8_t>(floor(max));

	getCartesianFromServo(0, 0, temp, min);
	getCartesianFromServo(180, 180, temp, max);
	min_y = static_cast<int8_t>(ceil(min));
	max_y = static_cast<int8_t>(floor(max));
}

DeltaPositionConverterClass DeltaPositionConverter;
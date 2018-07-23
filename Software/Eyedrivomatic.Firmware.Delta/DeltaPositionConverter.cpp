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

#define ARM_LENGTH 24.0L //length in millimeters of arm attached to servo.
#define FOREARM_LENGTH 43.0L //in millimeters of arm attached to joystick.
#define ENDPOINT_OFFSET_X 10.0L //distance in millimeters from joystick axis to forearm attachment point
#define ENDPOINT_OFFSET_Y 0.0L //distance in millimeters from joystick axis to forearm attachment point
#define SERVO_OFFSET_X 16.0L //distance between servo rotation point and centerline.
#define SERVO_OFFSET_Y -41.5L //distance between the imaginary line connecting servo rotation points and the center of the joystick.

#define MAX_MOVE_ABS 16.0L

DeltaPositionConverterClass::DeltaPositionConverterClass()
{
}


DeltaPositionConverterClass::~DeltaPositionConverterClass()
{
}

double toAbsolutePosition(double value, double minValue, double centerValue, double maxValue)
{
	value = constrain(value, -100, 100);

	return value >= 0
		? map(value, 0, 100, centerValue, maxValue)
		: map(value, 0, -100, centerValue, minValue);
}

double toRelativePosition(double value, double minValue, double centerValue, double maxValue)
{
	value = constrain(value, minValue, maxValue);

	auto result = value >= centerValue
		? map(value, centerValue, maxValue, 0.0L, 100.0L)
		: map(value, centerValue, minValue, 0.0L, -100.0L);

	//LoggerService.debug_P(PSTR("toRelativePosition: value=[%3.3f], min=[%3.3f], center=[%3.3f], max=[%3.3f], result=[%3.3f],"),
	//	value, minValue, centerValue, maxValue, result);
	return result;
}

struct point
{
	double x;
	double y;
};

/// http://paulbourke.net/geometry/circlesphere/tvoght.c
point * calculateCircleIntersection(point p1, double r1, point p2, double r2)
{
	static point result[2];

	double dx = p2.x - p1.x;
	double dy = p2.y - p1.y;

	double d = hypot(dx, dy); // Suggested by Keith Briggs
	if (r1 + r2 < d || abs(r1 - r2) > d)
	{
		// no intersection
		LoggerService.error_P(PSTR("Circles [%3.3f, %3.3f, r%3.3f], [%3.3f, %3.3f, r%3.3f] do not intersect."), p1.x, p1.y, r1, p2.x, p2.y, r2);
		return NULL;
	}

	/* Determine the distance from point p1 to point pmid. */
	double a = (sq(r1) - sq(r2) + sq(d)) / (2.0 * d);

	/* 'pmid' is the point where the line through the circle
	* intersection points crosses the line between the circle
	* centers.
	*/
	point pmid;
	pmid.x = p1.x + (dx * a / d);
	pmid.y = p1.y + (dy * a / d);

	/* Determine the distance from point 2 to either of the
	*  intersection points. */
	double h = sqrt(sq(r1) - sq(a));


	/* Now determine the offsets of the intersection points from
	* point 2.
	*/
	double rx = dy * (h / d);
	double ry = -dx * (h / d);

	/* Determine the absolute intersection points. */
	result[0].x = pmid.x - rx;
	result[1].x = pmid.x + rx;
	result[0].y = pmid.y - ry;
	result[1].y = pmid.y + ry;

	//LoggerService.debug_P(PSTR("Circles [%3.3f, %3.3f, r%3.3f], [%3.3f, %3.3f, r%3.3f] intersect at [%3.3f, %3.3f] and [%3.3f, %3.3f]."),
	//	p1.x, p1.y, r1, p2.x, p2.y, r2, result[0].x, result[0].y, result[1].x, result[1].y);

	return result;
}



void calculatePosition(double leftAngle, double rightAngle, point servoOffset, point joystickOffset, point & position)
{
	//LoggerService.debug_P(PSTR("calculatePosition: leftAngle=[%3.3f], rightAngle=[%3.3f], servoOffset=[%3.3f, %3.3f], joystickOffset=[%3.3f, %3.3f]"),
	//	leftAngle, rightAngle, servoOffset.x, servoOffset.y, joystickOffset.x, joystickOffset.y);

	auto leftRad = radians(180-leftAngle);
	point left;
	left.x = (cos(leftRad) * ARM_LENGTH);
	left.y = -(sin(leftRad) * ARM_LENGTH);

	auto rightRad = radians(rightAngle);
	point right;
	right.x = (cos(rightRad) * ARM_LENGTH);
	right.y = (sin(rightRad) * ARM_LENGTH);

	//LoggerService.debug_P(PSTR("calculatePosition: left delta=[%3.3f, %3.3f], right delta=[%3.3f, %3.3f]"), left.x, left.y, right.x, right.y);

	//account for servo offset.
	left.x -= servoOffset.x; left.y += servoOffset.y;
	right.x += servoOffset.x; right.y += servoOffset.y;

	//account for joystick offset.
	left.x += joystickOffset.x; left.y -= joystickOffset.y;
	right.x -= joystickOffset.x; right.y -= joystickOffset.y;

	//LoggerService.debug_P(PSTR("calculatePosition: left=[%3.3f, %3.3f], right=[%3.3f, %3.3f]"), left.x, left.y, right.x, right.y);
	auto intersections = calculateCircleIntersection(left, FOREARM_LENGTH, right, FOREARM_LENGTH);

	if (intersections == NULL)
	{
		LoggerService.error_P(PSTR("Servo angles [%3.3f, %3.3f] do not yield a solution."), leftAngle, rightAngle);
		position.x = 0; position.y = 0;
		return;
	}

	//The top solution should be correct.
	int i = intersections[0].y > intersections[1].y ? 0 : 1;
	position.x = intersections[i].x;
	position.y = intersections[i].y;
}


const double calculateAngle(point target, point origin)
{
	auto intersections = calculateCircleIntersection(origin, ARM_LENGTH, target, FOREARM_LENGTH);
	if (intersections == NULL) return 0;

	auto m = (origin.x < 0) ? intersections[0] : intersections[1];
	
	m.x -= origin.x;
	m.y -= origin.y;

	if (origin.x < 0)
	{
		m.x = -m.x;
		m.y = -m.y;
	}

	auto deg = degrees(atan2(m.y, m.x));

	//LoggerService.debug_P(PSTR("calculateAngle [%3.3f,%3.3f] = %3.3f deg"), m.x, m.y, deg);
	return deg;
}


void DeltaPositionConverterClass::getServoPosFromCartesian(double x, double y, double & leftAngle, double & rightAngle)
{
	auto runtime = millis();
	point target;
	target.x = toAbsolutePosition(x, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	target.y = toAbsolutePosition(y, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);

	//LoggerService.debug_P(PSTR("getServoPosFromCartesian: absolute pos=[%3.3f, %3.3f]"), target.x, target.y);

	point servoOffset;

	target.y += ENDPOINT_OFFSET_Y;
	target.x -= ENDPOINT_OFFSET_X;
	servoOffset.x = -SERVO_OFFSET_X;
	servoOffset.y = SERVO_OFFSET_Y;
	leftAngle = calculateAngle(target, servoOffset);

	servoOffset.x = SERVO_OFFSET_X;
	target.x += 2 * ENDPOINT_OFFSET_X;
	rightAngle = calculateAngle(target, servoOffset);

	runtime = millis() - runtime;
	LoggerService.debug_P(PSTR("getServoPosFromCartesian took [%d] milliseconds to run. Result: pos=[%3.3f, %3.3f], left angle=[%3.3f], right angle=[%3.3f]"), 
		runtime, x, y, leftAngle, rightAngle);
}


void DeltaPositionConverterClass::getCartesianFromServo(double leftAngle, double rightAngle, double & x, double & y)
{
	auto runtime = millis();
	point servoOffset;
	servoOffset.x = SERVO_OFFSET_X;
	servoOffset.y = SERVO_OFFSET_Y;

	point joystickOffset;
	joystickOffset.x = ENDPOINT_OFFSET_X;
	joystickOffset.y = ENDPOINT_OFFSET_Y;

	point position;

	calculatePosition(leftAngle, rightAngle, servoOffset, joystickOffset, position);

	//LoggerService.debug_P(PSTR("getCartesianFromServo: absolute pos=[%3.3f, %3.3f]"), position.x, position.y);

	x = toRelativePosition(position.x, Settings.MinPos_X, Settings.CenterPos_X, Settings.MaxPos_X);
	y = toRelativePosition(position.y, Settings.MinPos_Y, Settings.CenterPos_Y, Settings.MaxPos_Y);

	runtime = millis() - runtime;
	LoggerService.debug_P(PSTR("getCartesianFromServo took [%d] milliseconds to run. Result: left angle=[%3.3f], right angle=[%3.3f], pos=[%3.3f, %3.3f]"), runtime, leftAngle, rightAngle, x, y);
}

void DeltaPositionConverterClass::getServoPosFromVector(double direction, double speed, double & leftServo, double & rightServo)
{
	auto x = sin(radians(direction)) * speed; //switch sin/cos because 0 direction is 90 angle
	auto y = cos(radians(direction)) * speed;
	getServoPosFromCartesian(x, y, leftServo, rightServo);
}

void DeltaPositionConverterClass::getVectorFromServo(double leftServo, double rightServo, double & direction, double & speed)
{
	double x, y;
	getCartesianFromServo(leftServo, rightServo, x, y);

	direction = 90L - degrees(atan2(y, x));
	speed = hypot(x, y);

	//LoggerService.debug_P(PSTR("getVectorFromServo: vector=[%3.6f, %3.6f]"), direction, speed);

	if (direction > 180.0005L) direction -= 360;
	else if (direction < -179.9995L) direction += 360;
}


void DeltaPositionConverterClass::getLimits(double & min_x, double & max_x, double & min_y, double & max_y)
{
	min_x = -MAX_MOVE_ABS;
	max_x = MAX_MOVE_ABS;
	min_y = -MAX_MOVE_ABS;
	max_y = MAX_MOVE_ABS;
	//double temp;
	//double min,max;
	//getCartesianFromServo(0, 180, min, temp);
	//getCartesianFromServo(180, 0, max, temp);
	//min_x = static_cast<int8_t>(ceil(min));
	//max_x = static_cast<int8_t>(floor(max));

	//getCartesianFromServo(0, 0, temp, min);
	//getCartesianFromServo(180, 180, temp, max);
	//min_y = static_cast<int8_t>(ceil(min));
	//max_y = static_cast<int8_t>(floor(max));
}

void DeltaPositionConverterClass::getLimits(double & maxSpeed)
{
	maxSpeed = MAX_MOVE_ABS;
}

DeltaPositionConverterClass DeltaPositionConverter;
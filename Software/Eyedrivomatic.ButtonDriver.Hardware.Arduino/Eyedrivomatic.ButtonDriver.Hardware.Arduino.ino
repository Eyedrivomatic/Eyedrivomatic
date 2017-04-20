/*
Eyedrivomatic for Arduino - Version 4.1
This program is intended for use as part of the 'Eyedrivomatic System' for
controlling an electric wheelchair using soley the user's eyes.

For more information about Eyedrivomatic, see https://hackaday.io/project/5426-eye-controlled-wheelchair

Copyright (C)2015 Patrick Joyce

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 3
For more information see http://www.gnu.org/licenses/

Many thanks to Carlos William Galdino whose instructable set me on the right path.
http://www.instructables.com/id/Arduino-USB-comunication-Example-Program/  */

#include <EEPROM.h>
#include <Servo.h>

#define DiagReducerOff 0
#define DiagReducerOn 1

int EYEDRIVE = 199; // the arduino saves the settings info in it's eeprom memory. These variables are
int eyedrive = (EEPROM.read(0));// used to check whether there is pre saved settings data in the memory.

int receivedUSB; //will be used to read serial port;
int driveSwitchStateNS = 0;//x servo state variable 0= not driving at that moment 1= driving
int driveSwitchStateEW = 0;//y servo state variable 0= not driving at that moment 1= driving

enum ContinueState
{
	NotContinued = 0,
	DiagonalContinue = 1,
	NsContinue = 2,
	EwContinue = 3
};

ContinueState continueState = ContinueState::NotContinued; //a variable for whether the continue button has recently been pressed
					   //0=not continued recently, 1= just continued diagonally, 2 = just continued ns, 3 = just continued ew

enum NsState { yCenter = 0, yN = 1, yS = 2 };
NsState joystickStateNS = NsState::yCenter; //variable for the x servo joystick position 0=nothing 1=n 2=s

enum EwState { xCenter = 0, xE = 1, xW = 2};
EwState joystickStateEW = EwState::xCenter; //variable for the y servo joystick position 0=nothing 1=e 2=w

int speedState;//the variable for the global speed setting
int speedY = 0;//the amount the servos actually move is derived by adding a number defined by
int speedX = 22;
int diagSpeedX = 0;
int nudgeSpeedMin = 9;
int nudgeSpeedMax = 21;

int output1 = 7; //these initialise the output pins on the arduino that control the output relays
int output2 = 6;
int output3 = 5;
int output4 = 4;

int outputServoY = 8;
int outputServoX = 9;

enum direction
{
	noDirection = 0,
	nwDirection = 1,
	nDirection = 2,
	neDirection = 3,
	eDirection = 4,
	seDirection = 5,
	sDirection = 6,
	swDirection = 7,
	wDirection = 8
};

int lastDirButtPress = direction::noDirection; // a variable for the most recent direction button to be pressed
						  //  0=nothing 1=nw 2=n 3=ne 4=e 5=se 6=s 7=sw 8=w

						  //The following two variables are part of a  feature included in the code that bypasses all
						  //the safety features of the program. Do not use this feature unless you're in an entirely
						  //safe environment, where a completely out of control wheelchair would pose no threat to
						  //any person or property. If you turn the safety features off, eyedrivomatic will accept
						  //repeated presses of the same button, and if the eyegaze malfunctioned then the chair would
						  //not stop, which would be potentially dangerous

boolean safetyBypassToggle = false;

#define SAFE 0
#define UNSAFE 1
int safetyBypassState; //0 = safe, 1 = unsafe

unsigned long durationTimeNS = 3000; //the x and y servos are independent of each other
unsigned long durationElapsedTimeNS = 0; //these variables govern how long each servo will
unsigned long durationTimeEW = 3000; // operate for, before requiring another button press
unsigned long durationTimeEWalt;
unsigned long durationElapsedTimeEW = 0;
int durationTimeNSstate = 30;
int durationTimeEWstate = 30;

int diagonalReducer = DiagReducerOff;
int diagonalReducerX = 0;
int diagonalReducerY = 0;
int normalDiagonalReducerX = 0;
int normalDiagonalReducerY = 0;
int flipDiagonalReducerX = 0;
int flipDiagonalReducerY = 0;

int nudgeDuration = 6;
int nudgeSpeed = nudgeSpeedMin + 4;
int nudgeState = 0;
int nudgeDurationElapsedTime;

Servo yservo;  // create servo object to control the x servo
Servo xservo;  // create servo object to control the y servo

int ypos = 90;//the position of the x servo in degrees
int yMidpos = 90;// the middle position of the x servo
int xpos = 90;//the position of the y servo in degrees
int xMidpos = 82;//the middle position of the y servo

boolean isConnected = false;
const unsigned long STATUS_REFRESH_DELAY = 200; //the delay between status updates in milliseconds
unsigned long statusRefreshMs = 0L;

void setup() {

	pinMode(output1, OUTPUT);//initialise the output (relay) pins
	pinMode(output2, OUTPUT);
	pinMode(output3, OUTPUT);
	pinMode(output4, OUTPUT);

	digitalWrite(output1, LOW);// set all the relays to open (off)
	digitalWrite(output2, LOW);
	digitalWrite(output3, LOW);
	digitalWrite(output4, LOW);

	pinMode(output3, OUTPUT);
	pinMode(output4, OUTPUT);
	digitalWrite(outputServoY, LOW);
	digitalWrite(outputServoX, LOW);

	if (eyedrive != EYEDRIVE) {
		reinitialize();
		EEPROM.write(0, 199);
	}
	else {

		speedState = EEPROM.read(2);
		safetyBypassState = EEPROM.read(3);
		int foo = EEPROM.read(5); //Why are we saving this?
		driveSwitchStateNS = EEPROM.read(6);
		yMidpos = EEPROM.read(7);
		joystickStateNS = static_cast<NsState>(EEPROM.read(8));
		durationTimeNSstate = EEPROM.read(9);
		xMidpos = EEPROM.read(10);
		driveSwitchStateEW = EEPROM.read(11);
		diagonalReducer = EEPROM.read(12);
		joystickStateEW = static_cast<EwState>(EEPROM.read(13));
		durationTimeEWstate = EEPROM.read(14);
		nudgeSpeed = EEPROM.read(15) + nudgeSpeedMin;
		nudgeDuration = EEPROM.read(16);
	}

	//Don't attach until yMidPos and xMidPos are known to prevent servo glitch during startup.
	yservo.attach(outputServoY);  // attaches the servo on pin 8 to the servo object
	xservo.attach(outputServoX);  // attaches the servo on pin 9 to the servo object
	yservo.write(yMidpos);//sends the x servo to it's mid position
	xservo.write(xMidpos);// sends the y servo to it's mid position

	if (nudgeSpeed < nudgeSpeedMin) nudgeSpeed = nudgeSpeedMin;
	if (nudgeSpeed > nudgeSpeedMax) nudgeSpeed = nudgeSpeedMax;
		

	durationTimeNS = (durationTimeNSstate * 100);
	durationTimeEWalt = (durationTimeEWstate * 100);
	durationTimeEW = durationTimeEWalt;

	if (safetyBypassState == SAFE) {
		safetyBypassToggle = false;
	}
	else {
		safetyBypassToggle = true;
	}
	if (speedState == 1) {
		speedY = 0;
		diagSpeedX = 8;
		normalDiagonalReducerX = 3;
		normalDiagonalReducerY = 13;
	}
	if (speedState == 2) {
		speedY = 4;
		diagSpeedX = 14;
		normalDiagonalReducerX = 3;
		normalDiagonalReducerY = 18;
	}
	if (speedState == 3) {
		speedY = 8;
		diagSpeedX = 20;
		normalDiagonalReducerX = 3;
		normalDiagonalReducerY = 22;
	}
	if (speedState == 4) {
		speedY = 12;
		diagSpeedX = 26;
		normalDiagonalReducerX = 3;
		normalDiagonalReducerY = 23;
	}

	if (diagonalReducer == DiagReducerOn) {
		flipDiagonalReducerX = 0;
		flipDiagonalReducerY = 0;
	}
	else {
		flipDiagonalReducerX = diagonalReducerX;
		flipDiagonalReducerY = diagonalReducerY;
	}

	Serial.begin(9600);   //Begin arduino - pc communication
						  //establishContact();  // send a byte to establish contact with the pc until the pc program responds

}

void loop() {

	//in the loop the following code determines whether the x and y servos should be operating
	//and operates them if they should. If the duration timer has run out, it sends the
	//relevant servo to it's mid position

	if ((millis() - durationElapsedTimeNS) > durationTimeNS) {
		driveSwitchStateNS = 0;
		joystickStateNS = NsState::yCenter;
	}

	if (driveSwitchStateNS == 0)
	{
		ypos = yMidpos;
	}
	yservo.write(ypos);

	if ((millis() - durationElapsedTimeEW) > durationTimeEW) {
		driveSwitchStateEW = 0;
		joystickStateEW = EwState::xCenter;
	}

	if (driveSwitchStateEW == 0)
	{
		xpos = xMidpos;
	}

	xservo.write(xpos);

	SendStatus();

	if (safetyBypassToggle == false) {
		safetyBypassState = SAFE;
	}
	else {
		safetyBypassState = UNSAFE;
	}
	// This following section implements the safety feature bypass option.

	if (safetyBypassToggle == true) {
		if (lastDirButtPress == direction::nwDirection || lastDirButtPress == direction::neDirection || lastDirButtPress == direction::seDirection || lastDirButtPress == direction::swDirection) {
			continueState = ContinueState::DiagonalContinue;
		}
		if (lastDirButtPress == direction::nDirection || lastDirButtPress == direction::sDirection) {
			continueState = ContinueState::NsContinue;
		}
		if (lastDirButtPress == direction::eDirection || lastDirButtPress == direction::wDirection) {
			continueState = ContinueState::EwContinue;
		}
	}

	//respond to new connections with an initial string.
	if (millis() - statusRefreshMs > STATUS_REFRESH_DELAY)
	{
		if (Serial)
		{
			if (!isConnected)
			{
				delay(300);
				Serial.println();
				Serial.println("0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");   // send an initial string (6 ZEROS)

				isConnected = true;
			}

			SendStatus();
			//TODO: Send a status update. 
		}
		else
		{
			isConnected = false;
		}

		statusRefreshMs = millis();
	}

}

void SendStatus() {

	if (Serial.availableForWrite() < 32)
	{
		// not enough space in the send buffer.
		return;
	}

	durationTimeNSstate = (durationTimeNS / 100);
	durationTimeEWstate = (durationTimeEW / 100);
	//send the relevant information to the pc
	Serial.print(0);
	Serial.print(",");
	Serial.print(speedState);
	Serial.print(",");
	Serial.print(safetyBypassState);
	Serial.print(",");
	Serial.print(lastDirButtPress);
	Serial.print(",");
	Serial.print(continueState);
	Serial.print(",");
	Serial.print(driveSwitchStateNS);
	Serial.print(",");
	Serial.print(yservo.read());
	Serial.print(",");
	Serial.print(joystickStateNS);
	Serial.print(",");
	Serial.print(durationTimeNSstate);
	Serial.print(",");
	Serial.print(xservo.read());
	Serial.print(",");
	Serial.print(driveSwitchStateEW);
	Serial.print(",");
	Serial.print(diagonalReducer);
	Serial.print(",");
	Serial.print(joystickStateEW);
	Serial.print(",");
	Serial.print((durationTimeEWalt / 100));
	Serial.print(",");
	Serial.print(nudgeSpeed-nudgeSpeedMin);
	Serial.print(",");
	Serial.println(nudgeDuration); //the last data has to be "println"
}

void serialEvent() {

	//the serial event function governs what happens when the arduino receives an instruction from the pc

	receivedUSB = Serial.read(); //read the serial port
	switch (receivedUSB) { // sorts the received instructions

	case 33: //if the pc sends a byte to request information
			 //This is done automatically every STATUS_REFRESH_DELAY ms now.
		break;
	case 34:  // toggle the relay for output socket 1 for two tenths of a second - wheelchair on / off button
		digitalWrite(output1, HIGH);
		delay(200);
		digitalWrite(output1, LOW);
		break;
	case 35: // toggle the relay for output socket 2 (mode) for two tenths of a second
		digitalWrite(output3, HIGH);
		delay(200);
		digitalWrite(output3, LOW);
		break;
	case 36:// toggle the relay for output socket 2 for two tenths of a second five times
		for (int wDirection = 0; wDirection < 5; wDirection++) {
			digitalWrite(output3, HIGH);
			delay(200);
			digitalWrite(output3, LOW);
			delay(200);
		}
		break;
	case 48: // toggle the relay for output socket 3 for two tenths of a second
		digitalWrite(output4, HIGH);
		delay(200);
		digitalWrite(output4, LOW);
		break;
	case 37: //nw direction button press (the eight possible directions are expressed as compass marks. nw = north west)
		if (lastDirButtPress != direction::nwDirection || continueState != ContinueState::NotContinued) {
			if (joystickStateNS != NsState::yCenter || joystickStateEW != EwState::xW) {
				ypos = (yMidpos + ((speedY - normalDiagonalReducerX) + flipDiagonalReducerX));
				xpos = (xMidpos + ((diagSpeedX - normalDiagonalReducerY) + flipDiagonalReducerY));
				continueState = ContinueState::NotContinued;
				joystickStateNS = NsState::yN;
				joystickStateEW = EwState::xW;
				driveSwitchStateNS = 1;
				driveSwitchStateEW = 1;
				durationElapsedTimeNS = millis();
				durationElapsedTimeEW = millis();
				durationTimeEW = durationTimeNS;
			}
			else {
				if (continueState == ContinueState::DiagonalContinue) {
					durationElapsedTimeNS = millis();
					durationElapsedTimeEW = millis();
					continueState = ContinueState::NotContinued;
				}
			}
			lastDirButtPress = direction::nwDirection;
		}
		break;
	case 38: //n direction button press
		if (lastDirButtPress != direction::nDirection || continueState != ContinueState::NotContinued) {
			if (lastDirButtPress == direction::nwDirection || lastDirButtPress == direction::neDirection || lastDirButtPress == direction::seDirection || lastDirButtPress == direction::swDirection) {
				xpos = xMidpos;
			}
			if (joystickStateNS != NsState::yN) {
				ypos = (yMidpos + speedY);
				joystickStateNS = NsState::yN;
				continueState = ContinueState::NotContinued;
				driveSwitchStateNS = 1;
				durationElapsedTimeNS = millis();
			}
			else {
				if (continueState == ContinueState::NsContinue) {
					durationElapsedTimeNS = millis();
					continueState = ContinueState::NotContinued;
				}
			}
			lastDirButtPress = 2;
		}
		break;
	case 39: //ne direction button press
		if (lastDirButtPress != direction::neDirection || continueState != ContinueState::NotContinued) {
			if (joystickStateNS != NsState::yN || joystickStateEW != EwState::xE) {
				ypos = (yMidpos + ((speedY - normalDiagonalReducerX) + flipDiagonalReducerX));
				xpos = (xMidpos - ((diagSpeedX - normalDiagonalReducerY) + flipDiagonalReducerY));
				continueState = ContinueState::NotContinued;
				joystickStateNS = NsState::yN;
				joystickStateEW = EwState::xE;
				driveSwitchStateNS = 1;
				driveSwitchStateEW = 1;
				durationElapsedTimeNS = millis();
				durationElapsedTimeEW = millis();
				durationTimeEW = durationTimeNS;
			}
			else {
				if (continueState == ContinueState::DiagonalContinue) {
					durationElapsedTimeNS = millis();
					durationElapsedTimeEW = millis();
					durationTimeEW = durationTimeNS;
					continueState = ContinueState::NotContinued;
				}
			}
			lastDirButtPress = direction::neDirection;
		}
		break;
	case 40: //e direction button press
		if (lastDirButtPress != direction::eDirection || continueState != ContinueState::NotContinued) {
			if (lastDirButtPress == direction::nwDirection || lastDirButtPress == direction::neDirection || lastDirButtPress == direction::seDirection || lastDirButtPress == direction::swDirection) {
				ypos = yMidpos;
			}
			if (joystickStateEW != EwState::xE) {
				xpos = (xMidpos - speedX);
				joystickStateEW = EwState::xE;
				continueState = ContinueState::NotContinued;
				driveSwitchStateEW = 1;
				durationElapsedTimeEW = millis();
				durationTimeEW = durationTimeEWalt;
			}
			else {
				if (continueState == ContinueState::EwContinue) {
					durationElapsedTimeEW = millis();
					durationTimeEW = durationTimeEWalt;
					continueState = ContinueState::NotContinued;
				}
			}
			lastDirButtPress = direction::eDirection;
		}
		break;
	case 41: //se direction button press
		if (lastDirButtPress != direction::seDirection || continueState != ContinueState::NotContinued) {
			if (joystickStateNS != NsState::yS || joystickStateEW != 1) {
				ypos = (yMidpos - ((speedY - normalDiagonalReducerX) + flipDiagonalReducerX));
				xpos = (xMidpos - ((diagSpeedX - normalDiagonalReducerY) + flipDiagonalReducerY));
				continueState = ContinueState::NotContinued;
				joystickStateNS = NsState::yS;
				joystickStateEW = EwState::xE;
				driveSwitchStateNS = 1;
				driveSwitchStateEW = 1;
				durationElapsedTimeNS = millis();
				durationElapsedTimeEW = millis();
				durationTimeEW = durationTimeNS;
			}
			else {
				if (continueState == ContinueState::DiagonalContinue) {
					durationElapsedTimeNS = millis();
					durationElapsedTimeEW = millis();
					durationTimeEW = durationTimeNS;
					continueState = ContinueState::NotContinued;
				}
			}
			lastDirButtPress = direction::seDirection;
		}
		break;
	case 42: //s direction button press
		if (lastDirButtPress != direction::sDirection || continueState != ContinueState::NotContinued) {
			if (lastDirButtPress == direction::nwDirection || lastDirButtPress == direction::neDirection || lastDirButtPress == direction::seDirection || lastDirButtPress == direction::swDirection) {
				xpos = xMidpos;
			}
			if (joystickStateNS != NsState::yS) {
				ypos = (yMidpos - speedY);
				joystickStateNS = NsState::yS;
				continueState = ContinueState::NotContinued;
				driveSwitchStateNS = 1;
				durationElapsedTimeNS = millis();
			}
			else {
				if (continueState == ContinueState::NsContinue) {
					durationElapsedTimeNS = millis();
					continueState = ContinueState::NotContinued;
				}
			}
			lastDirButtPress = direction::sDirection;
		}
		break;
	case 43: //sw direction button press
		if (lastDirButtPress != direction::swDirection || continueState != ContinueState::NotContinued) {
			if (joystickStateNS != NsState::yS || joystickStateEW != EwState::xW) {
				ypos = (yMidpos - ((speedY - normalDiagonalReducerX) + flipDiagonalReducerX));
				xpos = (xMidpos + ((diagSpeedX - normalDiagonalReducerY) + flipDiagonalReducerY));
				continueState = ContinueState::NotContinued;
				joystickStateNS = NsState::yS;
				joystickStateEW = EwState::xW;
				driveSwitchStateNS = 1;
				driveSwitchStateEW = 1;
				durationElapsedTimeNS = millis();
				durationElapsedTimeEW = millis();
				durationTimeEW = durationTimeNS;
			}
			else {
				if (continueState == ContinueState::DiagonalContinue) {
					durationElapsedTimeNS = millis();
					durationElapsedTimeEW = millis();
					durationTimeEW = durationTimeNS;
					continueState = ContinueState::NotContinued;
				}
			}
			lastDirButtPress = direction::swDirection;
		}
		break;
	case 44: //w direction button press
		if (lastDirButtPress != direction::wDirection || continueState != ContinueState::NotContinued) {
			if (lastDirButtPress == direction::nwDirection || lastDirButtPress == direction::neDirection || lastDirButtPress == direction:: seDirection || lastDirButtPress == direction::swDirection) {
				ypos = yMidpos;
			}
			if (joystickStateEW != EwState::xW) {
				xpos = (xMidpos + speedX);
				joystickStateEW = EwState::xW;
				continueState = ContinueState::NotContinued;
				driveSwitchStateEW = 1;
				durationElapsedTimeEW = millis();
				durationTimeEW = durationTimeEWalt;
			}
			else {
				if (continueState == ContinueState::EwContinue) {
					durationElapsedTimeEW = millis();
					durationTimeEW = durationTimeEWalt;
					continueState = ContinueState::NotContinued;
				}
			}
			lastDirButtPress = direction::wDirection;
		}
		break;
	case 47: // continue button press
		if (continueState == ContinueState::NotContinued) {
			if (driveSwitchStateNS != 0 && driveSwitchStateEW == 0) {
				if (lastDirButtPress == direction::nDirection || lastDirButtPress == direction::sDirection) {
					durationElapsedTimeNS = millis();
					continueState = ContinueState::NsContinue;
				}
			}
			if (driveSwitchStateEW != 0 && driveSwitchStateNS == 0) {
				if (lastDirButtPress == direction::eDirection || lastDirButtPress == direction::wDirection) {
					durationElapsedTimeEW = millis();
					durationTimeEW = durationTimeEWalt;
					continueState = ContinueState::EwContinue;
				}
			}
			if (driveSwitchStateNS != 0 || driveSwitchStateEW != 0) {
				if (lastDirButtPress == direction::nwDirection || lastDirButtPress == direction::neDirection || lastDirButtPress == direction::seDirection || lastDirButtPress == direction::swDirection) {
					durationElapsedTimeNS = millis();
					durationElapsedTimeEW = millis();
					durationTimeEW = durationTimeNS;
					continueState = ContinueState::DiagonalContinue;
				}
			}

		}
		else {
			continueState = ContinueState::NotContinued;
			driveSwitchStateNS = 0;
			driveSwitchStateEW = 0;
			durationElapsedTimeNS = 0;
			durationElapsedTimeEW = 0;
		}
		break;
	case 45:// safety bypass toggle (this allows repeated button presses )
			//  DANGER this feature is inherently dangerous. Use with extreme caution
		safetyBypassToggle = !safetyBypassToggle;
		break;
	case 46: //nudge left
		if (driveSwitchStateNS != 0) {
			xpos = xMidpos + nudgeSpeed;
			joystickStateEW = EwState::xW;
			driveSwitchStateEW = 1;
			durationElapsedTimeEW = millis();
			durationTimeEW = (nudgeDuration * 100);
			nudgeState = 1;
		}
		break;
	case 77: //nudge right
		if (driveSwitchStateNS != 0) {
			xpos = xMidpos - nudgeSpeed;
			joystickStateEW = EwState::xE;
			driveSwitchStateEW = 1;
			durationElapsedTimeEW = millis();
			durationTimeEW = (nudgeDuration * 100);
			nudgeState = 1;
		}
		break;
	case 49:  //stop
		lastDirButtPress = direction::noDirection; // This resets the safety lockout that stops you repeating the same button
		continueState = ContinueState::NotContinued;
		driveSwitchStateNS = 0;
		joystickStateNS = NsState::yCenter;
		durationElapsedTimeNS = 0;
		driveSwitchStateEW = 0;
		joystickStateEW = EwState::xCenter;
		durationElapsedTimeEW = 0;
		break;
	case 50: // trim N
		yMidpos = (yMidpos + 1);
		break;
	case 51: // trim E
		xMidpos = (xMidpos - 1);
		break;
	case 52: // trim S
		yMidpos = (yMidpos - 1);
		break;
	case 53: // trim W
		xMidpos = (xMidpos + 1);
		break;
	case 55: //   speed = 1
		speedState = 1;
		speedY = 9;
		diagSpeedX = 17;
		normalDiagonalReducerX = 3;
		normalDiagonalReducerY = 13;
		diagonalReducerX = 0;
		diagonalReducerY = 10;

		if (diagonalReducer == DiagReducerOn) {
			flipDiagonalReducerX = 0;
			flipDiagonalReducerY = 0;
		}
		else {
			flipDiagonalReducerX = diagonalReducerX;
			flipDiagonalReducerY = diagonalReducerY;
		}
		break;
	case 56: //  speed = 2
		speedState = 2;
		speedY = 13;
		diagSpeedX = 23;
		normalDiagonalReducerX = 3;
		normalDiagonalReducerY = 18;
		diagonalReducerX = 0;
		diagonalReducerY = 10;

		if (diagonalReducer == DiagReducerOn) {
			flipDiagonalReducerX = 0;
			flipDiagonalReducerY = 0;
		}
		else {
			flipDiagonalReducerX = diagonalReducerX;
			flipDiagonalReducerY = diagonalReducerY;
		}
		break;
	case 57: //  speed = 3
		speedState = 3;
		speedY = 17;
		diagSpeedX = 29;
		normalDiagonalReducerX = 3;
		normalDiagonalReducerY = 22;
		diagonalReducerX = 0;
		diagonalReducerY = 10;

		if (diagonalReducer == DiagReducerOn) {
			flipDiagonalReducerX = 0;
			flipDiagonalReducerY = 0;
		}
		else {
			flipDiagonalReducerX = diagonalReducerX;
			flipDiagonalReducerY = diagonalReducerY;
		}
		break;
	case 58: //  speed = 4
		speedState = 4;
		speedY = 21;
		diagSpeedX = 35;
		normalDiagonalReducerX = 3;
		normalDiagonalReducerY = 23;
		diagonalReducerX = 0;
		diagonalReducerY = 10;

		if (diagonalReducer == DiagReducerOn) {
			flipDiagonalReducerX = 0;
			flipDiagonalReducerY = 0;
		}
		else {
			flipDiagonalReducerX = diagonalReducerX;
			flipDiagonalReducerY = diagonalReducerY;
		}
		break;
	case 60: // duration ns = half
		durationTimeNS = 500;
		break;
	case 61: // duration ns = 1
		durationTimeNS = 1000;
		break;
	case 62: // duration ns = 2
		durationTimeNS = 2000;
		break;
	case 63: // duration ns = 3
		durationTimeNS = 3000;
		break;
	case 64: // duration ns = 4
		durationTimeNS = 4000;
		break;
	case 54: // duration ns = 5
		durationTimeNS = 5000;
		break;
	case 65: // duration ns = 6
		durationTimeNS = 6000;
		break;
	case 66: // duration ew = half
		durationTimeEW = 500;
		durationTimeEWalt = 500;
		break;
	case 67: // duration ew = 1
		durationTimeEW = 1000;
		durationTimeEWalt = 1000;
		break;
	case 68: // duration ew = 1.5
		durationTimeEW = 1500;
		durationTimeEWalt = 1500;
		break;
	case 69: // duration ew = 2
		durationTimeEW = 2000;
		durationTimeEWalt = 2000;
		break;
	case 70: // duration ew =
		durationTimeEW = 3000;
		durationTimeEWalt = 3000;
		break;
	case 71: // duration ew = 4
		durationTimeEW = 4000;
		durationTimeEWalt = 4000;
		break;
	case 72: //duration ew = 5
		durationTimeEW = 5000;
		durationTimeEWalt = 5000;
		break;
	case 73: // nudge duration up 300 millisecs
		if (nudgeDuration < 30) {
			nudgeDuration = (nudgeDuration + 3);
		}
		break;
	case 74: // nudge duration down 300 millisecs
		if (nudgeDuration > 0) {
			nudgeDuration = (nudgeDuration - 3);
		}
		break;
	case 75: //
		EEPROM.write(1, 0);
		EEPROM.write(2, speedState);
		EEPROM.write(3, safetyBypassState);
		EEPROM.write(5, continueState);
		EEPROM.write(6, driveSwitchStateNS);
		EEPROM.write(7, yMidpos);
		EEPROM.write(8, joystickStateNS);
		EEPROM.write(9, durationTimeNSstate);
		EEPROM.write(10, xMidpos);
		EEPROM.write(11, driveSwitchStateEW);
		EEPROM.write(12, diagonalReducer);
		EEPROM.write(13, joystickStateEW);
		EEPROM.write(14, (durationTimeEWalt / 100));
		EEPROM.write(15, nudgeSpeed);
		EEPROM.write(16, nudgeDuration);
		break;
	case 59: //nudgeSpeed up one
		if (nudgeSpeed < nudgeSpeedMax - 4)  nudgeSpeed += 4;
		break;
	case 76: //nudgeSpeed down one
		if (nudgeSpeed > nudgeSpeedMin + 4) nudgeSpeed -= 4;
		break;
	case 78: // Diagonal reducer toggle
		if (diagonalReducer == DiagReducerOff) {
			diagonalReducer = DiagReducerOn;
			flipDiagonalReducerX = 0;
			flipDiagonalReducerY = 0;
		}
		else {
			diagonalReducer = DiagReducerOff;
			flipDiagonalReducerX = diagonalReducerX;
			flipDiagonalReducerY = diagonalReducerY;
		}
		break;
	}
}

void establishContact() {  //This function is used to start the communication
	while (Serial.available() <= 0) {
		Serial.println("0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0");   // send an initial string (6 ZEROS)
															 //IT HAS TO BE EXACLY THE SAME LENGHT OF THE SIGNAL YOU ARE SENDING TO THE PC
															 //WE ARE SENDING (DATA 0,1,2,3,4,5) -- WITCH GIVE US 6 DATA
		delay(300);
	}
}

void reinitialize() {
	EEPROM.write(1, 0); //manoeuvreState
	EEPROM.write(2, 1); //speedState
	EEPROM.write(3, 0); //safetyBypassState
	EEPROM.write(4, 0); //lastDirButtPress
	EEPROM.write(5, 0); //continueState
	EEPROM.write(6, 0); //driveSwitchStateNS
	EEPROM.write(7, 90); //yMidpos
	EEPROM.write(8, 0); //joystickStateNS
	EEPROM.write(9, 20); //durationTimeNSstate
	EEPROM.write(10, 90); //xMidpos
	EEPROM.write(11, 0); //driveSwitchStateEW
	EEPROM.write(12, 1); //
	EEPROM.write(13, 0); //joystickStateEW
	EEPROM.write(14, 20); //durationTimeEWstate
	EEPROM.write(15, 0); //
	EEPROM.write(16, 6); //ySpeedTrim
}

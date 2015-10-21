  /*
    Eyedrivomatic for Arduino - Version 4
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

int EYEDRIVE = 199; // the arduino saves the settings info in it's eeprom memory. These variables are 
int eyedrive = (EEPROM.read(0));// used to check whether there is pre saved settings data in the memory. 

int receivedUSB; //will be used to read serial port;
int driveSwitchStateNS = 0;//x servo state variable 0= not driving at that moment 1= driving 
int driveSwitchStateEW = 0;//y servo state variable 0= not driving at that moment 1= driving 
int continueState = 0; //a variable for whether the continue button has recently been pressed 
//0=not continued recently, 1= just continued diagonally, 2 = just continued ns, 3 = just continued ew
int joystickStateNS = 0; //variable for the x servo joystick position 0=nothing 1=n 2=s 
int joystickStateEW = 0;//variable for the y servo joystick position 0=nothing 1=e 2=w

int speedState;//the variable for the global speed setting 
int speedX = 0;//the amount the servos actually move is derived by adding a number defined by 
int speedY = 0;// the global speed setting called speedX or speedY to the mid position of the
int speedAdd = 9;// servo in question, then adding a constant called speedAdd. 
int speedAddDiag = 5;//This allows us to better control the speed of the two servos
//  in relation to each other, and the direction of travel. 

int output1 = 7; //these initialise the output pins on the arduino that control the output relays 
int output2 = 6;
int output3 = 5; 
int output4 = 4; 

int lastDirButtPress = 0; // a variable for the most recent direction button to be pressed 
//  0=nothing 1=nw 2=n 3=ne 4=e 5=se 6=s 7=sw 8=w

//The following two variables are part of a  feature included in the code that bypasses all
//the safety features of the program. Do not use this feature unless you're in an entirely 
//safe environment, where a completely out of control wheelchair would pose no threat to
//any person or property. If you turn the safety features off, eyedrivomatic will accept 
//repeated presses of the same button, and if the eyegaze malfunctioned then the chair would 
//not stop, which would be potentially dangerous 
 
boolean safetyBypassToggle = false;
int safetyBypassState; //0 = safe, 1 = unsafe 

unsigned long durationTimeNS = 3000; //the x and y servos are independent of each other 
unsigned long durationElapsedTimeNS = 0; //these variables govern how long each servo will 
unsigned long durationTimeEW = 3000; // operate for, before requiring another button press 
unsigned long durationTimeEWalt; 
unsigned long durationElapsedTimeEW = 0; 
int durationTimeNSstate = 30; 
int durationTimeEWstate = 30; 

int diagonalReducer = 1; 
int diagonalReducerX = 0; 
int diagonalReducerY = 0; 
int normalDiagonalReducerX = 0; 
int normalDiagonalReducerY = 0; 
int flipDiagonalReducerX = 0; 
int flipDiagonalReducerY = 0; 

int leftRightReducer = 22; 

int nudgeDuration = 6; 
int nudgeSpeed = 4; 
int nudgeState = 0; 
int nudgeDurationElapsedTime; 

Servo xservo;  // create servo object to control the x servo 
Servo yservo;  // create servo object to control the y servo

int xpos = 90;//the position of the x servo in degrees 
int xMIDpos = 90;// the middle position of the x servo 
int ypos = 90;//the position of the y servo in degrees 
int yMIDpos = 82;//the middle position of the y servo 

void setup() {    

  Serial.begin(9600);   //Begin arduino - pc communication
  pinMode(output1, OUTPUT);//initialise the output (relay) pins 
  pinMode(output2, OUTPUT);
  pinMode(output3, OUTPUT);
  pinMode(output4, OUTPUT);
  
  digitalWrite(output1, LOW);// set all the relays to open (off)
  digitalWrite(output2, LOW);
  digitalWrite(output3, LOW);
  digitalWrite(output4, LOW);
 

    if (eyedrive != EYEDRIVE) {
      reinitialize();
      EEPROM.write(0, 199);
  }
  else {
  
        speedState = EEPROM.read(2); 
        safetyBypassState = EEPROM.read(3); 
        continueState = EEPROM.read(5); 
        driveSwitchStateNS = EEPROM.read(6); 
        xMIDpos = EEPROM.read(7); 
        joystickStateNS = EEPROM.read(8); 
        durationTimeNSstate = EEPROM.read(9); 
        yMIDpos = EEPROM.read(10); 
        driveSwitchStateEW = EEPROM.read(11); 
        diagonalReducer = EEPROM.read(12); 
        joystickStateEW = EEPROM.read(13); 
        durationTimeEWstate = EEPROM.read(14);
        nudgeSpeed = EEPROM.read(15); 
        nudgeDuration = EEPROM.read(16); 
}
if ((nudgeSpeed == 0) || (nudgeSpeed == 4) || (nudgeSpeed == 8) || (nudgeSpeed == 12)){
  }
else{
  nudgeSpeed = 0; 
}
  xservo.attach(8);  // attaches the servo on pin 7 to the servo object 
  yservo.attach(9);  // attaches the servo on pin 7 to the servo object 
  xservo.write(xMIDpos);//sends the x servo to it's mid position 
  yservo.write(yMIDpos);// sends the y servo to it's mid position 
  
durationTimeNS = (durationTimeNSstate * 100); 
durationTimeEWalt = (durationTimeEWstate * 100); 
durationTimeEW = durationTimeEWalt; 

if (safetyBypassState == 0){
  safetyBypassToggle = false; 
}
else {
  safetyBypassToggle = true;   
}
if(speedState == 1){
 speedX = 0; 
 speedY = 8; 
 normalDiagonalReducerX = 3; 
 normalDiagonalReducerY = 13;
}
if(speedState == 2){
 speedX = 4; 
 speedY = 14; 
 normalDiagonalReducerX = 3; 
 normalDiagonalReducerY = 18;
}
if(speedState == 3){
 speedX = 8; 
 speedY = 20; 
 normalDiagonalReducerX = 3; 
 normalDiagonalReducerY = 22;
}
if(speedState == 4){
 speedX = 12; 
 speedY = 26; 
 normalDiagonalReducerX = 3; 
 normalDiagonalReducerY = 23; 
}

        if (diagonalReducer == 2){
          flipDiagonalReducerX = 0; 
          flipDiagonalReducerY = 0; 
        }
        else {
          flipDiagonalReducerX = diagonalReducerX; 
          flipDiagonalReducerY = diagonalReducerY;  
        }
  establishContact();  // send a byte to establish contact with the pc until the pc program responds
 
}

void loop() {
  
  //in the loop the following code determines whether the x and y servos should be operating 
  //and operates them if they should. If the duration timer has run out, it sends the 
  //relevant servo to it's mid position 
  
  if ((millis () - durationElapsedTimeNS) > durationTimeNS){
    driveSwitchStateNS = 0; 
    joystickStateNS = 0;
  }
  if (driveSwitchStateNS == 1){
    xservo.write(xpos);
  }
  else{ 
     xservo.write(xMIDpos);
  }
  if ((millis () - durationElapsedTimeEW) > durationTimeEW){
    driveSwitchStateEW = 0; 
    joystickStateEW = 0;
  }
  if (driveSwitchStateEW == 1){
    yservo.write(ypos);
  }
  else{ 
    yservo.write(yMIDpos);
    }
  
  if (safetyBypassToggle == false){
    safetyBypassState = 0; 
  }
  else {
    safetyBypassState = 1; 
  }
  // This following section implements the safety feature bypass option. 
  
  if (safetyBypassToggle == true){
    if (lastDirButtPress == 1 ||lastDirButtPress == 3 ||lastDirButtPress == 5 || lastDirButtPress == 7){
    continueState = 1; 
    }
    if (lastDirButtPress == 2 ||lastDirButtPress == 6){
    continueState = 2; 
    }
    if (lastDirButtPress == 4 ||lastDirButtPress == 8){
    continueState = 3; 
    }
  }
}
    
void serialEvent(){
  
  //the serial event function governs what happens when the arduino receives an instruction from the pc 
  
      receivedUSB = Serial.read(); //read the serial port      
      switch(receivedUSB) { // sorts the received instructions 
        
       case 33: //if the pc sends a byte to request information 
        durationTimeNSstate = (durationTimeNS / 100); 
        durationTimeEWstate = (durationTimeEW / 100); 
        delay (2); //Delay for stabilizing the signal
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
        Serial.print(xMIDpos); 
        Serial.print(",");
        Serial.print(joystickStateNS); 
        Serial.print(",");
        Serial.print(durationTimeNSstate); 
        Serial.print(",");
        Serial.print(yMIDpos); 
        Serial.print(",");
        Serial.print(driveSwitchStateEW); 
        Serial.print(",");
        Serial.print(diagonalReducer); 
        Serial.print(",");
        Serial.print(joystickStateEW); 
        Serial.print(",");
        Serial.print((durationTimeEWalt / 100)); 
        Serial.print(",");
        Serial.print(nudgeSpeed); 
        Serial.print(",");
        Serial.println(nudgeDuration); //the last data has to be "println"  
        break; 
       case 34:  // toggle the relay for output socket 1 for two tenths of a second - wheelchair on / off button 
        digitalWrite(output1, HIGH);
        delay (200);
        digitalWrite(output1, LOW);
        break;
       case 35: // toggle the relay for output socket 2 (mode) for two tenths of a second
        digitalWrite(output3, HIGH);
        delay (200);
        digitalWrite(output3, LOW);
        break;
       case 36:// toggle the relay for output socket 2 for two tenths of a second five times 
        for (int w = 0; w<5; w++){
        digitalWrite(output3, HIGH);
        delay (200);
        digitalWrite(output3, LOW);
        delay (200);}
        break;
       case 48: // toggle the relay for output socket 3 for two tenths of a second 
        digitalWrite(output4, HIGH);
        delay (200);
        digitalWrite(output4, LOW);
        break;
       case 37: //nw direction button press (the eight possible directions are expressed as compass marks. nw = north west)
        if (lastDirButtPress != 1 || continueState != 0){
        if (joystickStateNS != 1 || joystickStateEW != 2){
        xpos = (xMIDpos + ((speedX - normalDiagonalReducerX) + flipDiagonalReducerX));
        ypos = (yMIDpos + ((speedY - normalDiagonalReducerY) + flipDiagonalReducerY));
        continueState = 0;
        joystickStateNS = 1;
        joystickStateEW = 2;
        driveSwitchStateNS = 1; 
        driveSwitchStateEW = 1; 
        durationElapsedTimeNS = millis(); 
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeNS; 
        }
        else {
          if (continueState == 1){
        durationElapsedTimeNS = millis(); 
        durationElapsedTimeEW = millis(); 
        continueState = 0;
          }
        }
        lastDirButtPress = 1; 
        }
        break;
       case 38: //n direction button press 
        if (lastDirButtPress != 2 || continueState != 0){
        if (lastDirButtPress == 1 || lastDirButtPress == 3 || lastDirButtPress == 5 || lastDirButtPress == 7){
          ypos = yMIDpos; 
        }
        if (joystickStateNS != 1){
        xpos = (xMIDpos + speedX);
        joystickStateNS = 1;
        continueState = 0;
        driveSwitchStateNS = 1;
        durationElapsedTimeNS = millis(); 
        }
        else {
          if (continueState == 2){
        durationElapsedTimeNS = millis(); 
        continueState = 0;
          }
        }
        lastDirButtPress = 2; 
        }
        break;
       case 39: //ne direction button press 
        if (lastDirButtPress != 3 || continueState != 0){
        if (joystickStateNS != 1 || joystickStateEW != 1){
        xpos = (xMIDpos + ((speedX - normalDiagonalReducerX) + flipDiagonalReducerX));
        ypos = (yMIDpos - ((speedY - normalDiagonalReducerY) + flipDiagonalReducerY));
        continueState = 0;
        joystickStateNS = 1;
        joystickStateEW = 1;
        driveSwitchStateNS = 1; 
        driveSwitchStateEW = 1; 
        durationElapsedTimeNS = millis(); 
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeNS; 
        }
        else {
          if (continueState == 1){
        durationElapsedTimeNS = millis(); 
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeNS; 
        continueState = 0;
          }
        }
        lastDirButtPress = 3; 
        }
        break;
       case 40: //e direction button press 
        if (lastDirButtPress != 4 || continueState != 0){
        if (lastDirButtPress == 1 || lastDirButtPress == 3 || lastDirButtPress == 5 || lastDirButtPress == 7){
          xpos = xMIDpos; 
        }
        if (joystickStateEW != 1){
        ypos = (yMIDpos - leftRightReducer);
        joystickStateEW = 1;
        continueState = 0;
        driveSwitchStateEW = 1;
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeEWalt; 
        }
        else {
          if (continueState == 3){
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeEWalt; 
        continueState = 0;
          }
        }
        lastDirButtPress = 4; 
        }
        break; 
       case 41: //se direction button press 
        if (lastDirButtPress != 5 || continueState != 0){
        if (joystickStateNS != 2 || joystickStateEW != 1){
        xpos = (xMIDpos - ((speedX - normalDiagonalReducerX) + flipDiagonalReducerX));
        ypos = (yMIDpos - ((speedY - normalDiagonalReducerY) + flipDiagonalReducerY));
        continueState = 0;
        joystickStateNS = 2;
        joystickStateEW = 1;
        driveSwitchStateNS = 1; 
        driveSwitchStateEW = 1; 
        durationElapsedTimeNS = millis(); 
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeNS; 
        }
        else {
          if (continueState == 1){
        durationElapsedTimeNS = millis(); 
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeNS; 
        continueState = 0;
          }
        }
        lastDirButtPress = 5; 
        }
        break;
       case 42: //s direction button press 
        if (lastDirButtPress != 6 || continueState != 0){
        if (lastDirButtPress == 1 || lastDirButtPress == 3 || lastDirButtPress == 5 || lastDirButtPress == 7){
          ypos = yMIDpos; 
        }
        if (joystickStateNS != 2){
        xpos = (xMIDpos - speedX);
        joystickStateNS = 2;
        continueState = 0;
        driveSwitchStateNS = 1;
        durationElapsedTimeNS = millis(); 
        }
        else {
          if (continueState == 2){
        durationElapsedTimeNS = millis(); 
        continueState = 0;
          }
        }
        lastDirButtPress = 6; 
        }
        break;
       case 43: //sw direction button press 
        if (lastDirButtPress != 7 || continueState != 0){
        if (joystickStateNS != 2 || joystickStateEW != 2){
        xpos = (xMIDpos - ((speedX - normalDiagonalReducerX) + flipDiagonalReducerX));
        ypos = (yMIDpos + ((speedY - normalDiagonalReducerY) + flipDiagonalReducerY));
        continueState = 0;
        joystickStateNS = 2;
        joystickStateEW = 2;
        driveSwitchStateNS = 1; 
        driveSwitchStateEW = 1; 
        durationElapsedTimeNS = millis(); 
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeNS; 
        }
        else {
          if (continueState == 1){
        durationElapsedTimeNS = millis(); 
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeNS; 
        continueState = 0;
          }
        }
        lastDirButtPress = 7; 
        }
        break;
       case 44: //w direction button press 
        if (lastDirButtPress != 8 || continueState != 0){
        if (lastDirButtPress == 1 || lastDirButtPress == 3 || lastDirButtPress == 5 || lastDirButtPress == 7){
          xpos = xMIDpos; 
        }
        if (joystickStateEW != 2){
        ypos = (yMIDpos + leftRightReducer);
        joystickStateEW = 2;
        continueState = 0;
        driveSwitchStateEW = 1;
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeEWalt; 
        }
        else {
          if (continueState == 3){
        durationElapsedTimeEW = millis(); 
        durationTimeEW = durationTimeEWalt; 
        continueState = 0;
          }
        }
        lastDirButtPress = 8; 
        }
        break;
       case 47: // continue button press 
          if (continueState == 0){
            if (driveSwitchStateNS != 0 && driveSwitchStateEW == 0){
              if (lastDirButtPress == 2 || lastDirButtPress == 6){
        durationElapsedTimeNS = millis();     
        continueState = 2;
              }
            }
            if (driveSwitchStateEW != 0 && driveSwitchStateNS == 0){
              if (lastDirButtPress == 4 || lastDirButtPress == 8){
        durationElapsedTimeEW = millis();     
        durationTimeEW = durationTimeEWalt; 
        continueState = 3;
              }
            }
            if (driveSwitchStateNS != 0 || driveSwitchStateEW != 0){
              if (lastDirButtPress == 1 || lastDirButtPress == 3 || lastDirButtPress == 5 || lastDirButtPress == 7){
        durationElapsedTimeNS = millis();     
        durationElapsedTimeEW = millis();  
        durationTimeEW = durationTimeNS;    
        continueState = 1;
              }
            }
            
          }
          else {
        continueState = 0;   
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
        if (driveSwitchStateNS != 0){
        ypos = (yMIDpos + (nudgeSpeed + speedAdd));
        joystickStateEW = 2;
        driveSwitchStateEW = 1;
        durationElapsedTimeEW = millis();     
        durationTimeEW = (nudgeDuration * 100); 
        nudgeState = 1; 
        }       
        break;
       case 77: //nudge right 
        if (driveSwitchStateNS != 0){
        ypos = (yMIDpos - (nudgeSpeed + speedAdd));
        joystickStateEW = 1;
        driveSwitchStateEW = 1;
        durationElapsedTimeEW = millis();     
        durationTimeEW = (nudgeDuration * 100); 
        nudgeState = 1; 
        }
        break;
       case 49:  //stop 
        lastDirButtPress = 0; // This resets the safety lockout that stops you repeating the same button 
        continueState = 0;
        driveSwitchStateNS = 0; 
        joystickStateNS = 0;
        durationElapsedTimeNS = 0; 
        driveSwitchStateEW = 0; 
        joystickStateEW = 0;
        durationElapsedTimeEW = 0; 
        break;
       case 50: // trim N
        xMIDpos = (xMIDpos + 1); 
        break;
       case 51: // trim E
        yMIDpos = (yMIDpos - 1); 
        break;
       case 52: // trim S
        xMIDpos = (xMIDpos - 1); 
        break;
       case 53: // trim W
        yMIDpos = (yMIDpos + 1); 
        break;
       case 55: //   speed = 1
        speedState = 1; 
        speedX = 9; 
        speedY = 17; 
        normalDiagonalReducerX = 3; 
        normalDiagonalReducerY = 13;
        diagonalReducerX = 0; 
        diagonalReducerY = 10; 
        
        if (diagonalReducer == 2){
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
        speedX = 13; 
        speedY = 23; 
        normalDiagonalReducerX = 3; 
        normalDiagonalReducerY = 18;
        diagonalReducerX = 0; 
        diagonalReducerY = 10; 
        
        if (diagonalReducer == 2){
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
        speedX = 17; 
        speedY = 29; 
        normalDiagonalReducerX = 3; 
        normalDiagonalReducerY = 22;
        diagonalReducerX = 0; 
        diagonalReducerY = 10; 
        
        if (diagonalReducer == 2){
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
        speedX = 21; 
        speedY = 35; 
        normalDiagonalReducerX = 3; 
        normalDiagonalReducerY = 23;
        diagonalReducerX = 0; 
        diagonalReducerY = 10; 
        
        if (diagonalReducer == 2){
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
        if (nudgeDuration < 30){
           nudgeDuration = (nudgeDuration + 3); 
        }
        break;
       case 74: // nudge duration down 300 millisecs 
        if (nudgeDuration > 0){
           nudgeDuration = (nudgeDuration - 3); 
       }
        break;
       case 75: // 
        EEPROM.write(1, 0); 
        EEPROM.write(2, speedState); 
        EEPROM.write(3, safetyBypassState); 
        EEPROM.write(5, continueState); 
        EEPROM.write(6, driveSwitchStateNS); 
        EEPROM.write(7, xMIDpos); 
        EEPROM.write(8, joystickStateNS); 
        EEPROM.write(9, durationTimeNSstate); 
        EEPROM.write(10, yMIDpos); 
        EEPROM.write(11, driveSwitchStateEW); 
        EEPROM.write(12, diagonalReducer); 
        EEPROM.write(13, joystickStateEW); 
        EEPROM.write(14, (durationTimeEWalt /100)); 
        EEPROM.write(15, nudgeSpeed); 
        EEPROM.write(16, nudgeDuration);  
        break;
       case 59: //nudgeSpeed up one 
        if (nudgeSpeed < 12){
          nudgeSpeed = (nudgeSpeed +4); 
        }
        break;
       case 76: //nudgeSpeed down one 
        if (nudgeSpeed > 0){
          nudgeSpeed = (nudgeSpeed - 4); 
        }
        break;
        case 78: // Diagonal reducer toggle
        if (diagonalReducer == 1){
          diagonalReducer = 2; 
          flipDiagonalReducerX = 0; 
          flipDiagonalReducerY = 0; 
        }
        else {
          diagonalReducer = 1;       
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

void reinitialize(){
        EEPROM.write(1, 0); //manoeuvreState
        EEPROM.write(2, 1); //speedState
        EEPROM.write(3, 0); //safetyBypassState
        EEPROM.write(4, 0); //lastDirButtPress
        EEPROM.write(5, 0); //continueState
        EEPROM.write(6, 0); //driveSwitchStateNS
        EEPROM.write(7, 90); //xMIDpos
        EEPROM.write(8, 0); //joystickStateNS
        EEPROM.write(9, 20); //durationTimeNSstate
        EEPROM.write(10, 90); //yMIDpos
        EEPROM.write(11, 0); //driveSwitchStateEW
        EEPROM.write(12, 1); //
        EEPROM.write(13, 0); //joystickStateEW
        EEPROM.write(14, 20); //durationTimeEWstate
        EEPROM.write(15, 0); //
        EEPROM.write(16, 6); //ySpeedTrim
}

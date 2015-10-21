 /*
    Eyedrivomatic for pc - Version 4
    This program is intended for use as part of the 'Eyedrivomatic System' for 
    controlling an electric wheelchair using soley the user's eyes. 

    This program accepts keyboard input in order to control the users chair. 
    The users own AAC software package can be configured to provide the necessary 
    text input with an interface in that package suited to the users needs. 
    
    For more information on the Eyedrivomatic project see, https://hackaday.io/project/5426-eye-controlled-wheelchair
    
    
    Copyright (C)2015 Patrick Joyce

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License version 3
    For more information on this licence see http://www.gnu.org/licenses/
 
 Many thanks to Carlos William Galdino whose instructable set me on the right path. 
 http://www.instructables.com/id/Arduino-USB-comunication-Example-Program/  */

int driveSwitchState = 0;
int lastBut = 0;
int ySpeedTrim; 

//variables received from the Arduino 
int data0;    //data 1 received from Arduino manoeuvreState
int data1;    //data 2 received from Arduino speedState
int data2;    //data 3 received from Arduino safetyBypassState
int data3;    //data 4 received from Arduino lastDirbuttpress 
int data4;    //data 5 received from Arduino continueState
int data5;    //data 6 received from Arduino driveSwitchStateNS
int data6;    //data7 received from Arduino xMIDpos
int data7;    //data8 received from Arduino joystickStateNS
int data8;    //data9 received from Arduino durationTimeNSstate
int data9;    //data10 received from Arduino yMIDpos
int data10;    //data11 received from Arduino driveSwitchStateEW
int data11;    //data12 received from Arduino 0
int data12;    //data13 received from Arduino joystickStateEW
int data13;    //data14 received from Arduino durationTimeEWstate
int data14;    //data15 received from Arduino nudgeSpeed  
int data15;    //data16 received from Arduino nudgeDuration

boolean firstContact = false;

import processing.serial.*;//start serial communication
Serial Port;

void setup (){ 
  
  size (1260,790);    //Initialise the window's size
  if (frame != null) {
    frame.setResizable(true);
  }
  frame.setTitle("Eyedrivomatic Pure 4E");
  Port = new Serial(this, "COM16", 9600); //starts the Serial port - COM4
  Port.bufferUntil('\n'); //clean the buffer    
}

void draw (){ //starts loop

  background(243, 242, 162); // Background colour 
  
if ((data4 == 0) && (data3 == 0)){
    PImage pushAny = loadImage("8-5.png"); 
    image(pushAny,585,116); // icon for push any next 
}
if ((data4 != 0) && (data3 != 0)){
    PImage pushAny = loadImage("8-5.png"); 
    image(pushAny,585,116); //  icon for push direction next
}
if ((data4 == 0) && (data3 != 0)){
    PImage pushContinue = loadImage("8-1.png"); 
    image(pushContinue,585,116); //  icon for push continue next
}
if ((data5 == 0) && (data10 == 0)  && (data4 == 0) && (data3 != 0)){
    PImage pushReset = loadImage("8-4.png"); 
    image(pushReset,585,116); //  icon for push continue next
}
    PImage yPic = loadImage("9-1.png"); // Draw a box for the y servo status information 
    image(yPic,696,7);
    PImage xPic = loadImage("8-6.png"); // Draw a box for the x servo status information 
    image(xPic,472,7);
    
if (data7 == 1){
    PImage nTPic = loadImage("n.png"); 
    image(nTPic,697,7);
}
if (data7 == 2){
    PImage sTPic = loadImage("s.png"); 
    image(sTPic,697,7);
}
if (data12 == 1){
    PImage eTPic = loadImage("e.png"); 
    image(eTPic,470,7);
}
if (data12 == 2){
    PImage wTPic = loadImage("w.png"); 
    image(wTPic,470,7);
}
    
    
  switch (data3){
    case 0: 
    PImage readyPicB = loadImage("9-1.png"); 
    image(readyPicB,584,7);
    break;
    case 1: 
    PImage nwPic = loadImage("1-1.png"); 
    image(nwPic,584,7);
    break;
    case 2: 
    PImage nPic = loadImage("3-1.png"); 
    image(nPic,584,7);
    break;
    case 3: 
    PImage nePic = loadImage("5-1.png"); 
    image(nePic,584,7);
    break;
    case 4: 
    PImage ePic = loadImage("5-3.png"); 
    image(ePic,584,7);
    break;
    case 5: 
    PImage sePic = loadImage("5-5.png"); 
    image(sePic,584,7);
    break;
    case 6: 
    PImage sPic = loadImage("3-5.png"); 
    image(sPic,584,7);
    break;
    case 7: 
    PImage swPic = loadImage("1-5.png"); 
    image(swPic,584,7);
    break;
    case 8: 
    PImage wPic = loadImage("1-3.png"); 
    image(wPic,584, 7);
    break;
  }

  PFont f1 = loadFont("Gautami-20.vlw"); //fonts
  PFont f2 = loadFont("Gautami-28.vlw"); //fonts
  PFont f3 = loadFont("Gautami-Bold-28.vlw"); //fonts
  
  textFont(f1);
  fill(0);
  text("Safety Bypass Status", 150, 64);
  text("X Servo Centre Point           Degrees", 850, 55);
  text("Nudge Speed                        ", 850, 95);
  text("Nudge Duration                             millisecs", 850, 135);
  text("Y Servo Centre Point           Degrees", 850, 175);
  text("seconds", 491, 105);
  text("seconds", 715, 105);
  text("Y Servo", 493, 130);
  text("X Servo", 717, 130);
  
  textFont(f2);
  fill(0);
  text((data8 * 100), 705, 34);// NS duration information 
  text((data13 * 100), 481, 34);// EW duration information 
  text((data9), 1023, 175);// y servo trim information   
  text((data15 * 100), 1023, 135);// nudge duration information  
  if (data14 == 0){
  text(("SLOW"), 1023, 95);// nudge speed information   
  }
  if (data14 == 4){
  text(("WALK"), 1023, 95);// nudge speed information   
  }
  if (data14 == 8){
  text(("FAST"), 1023, 95);// nudge speed information   
  }
  if (data14 == 12){
  text(("MANIC"), 1023, 95);// nudge speed information   
  }
  text((data6), 1023, 55);// x servo trim information 
  
// Speed Status 
if (data1 == 1){
  textFont(f3);
  fill(255, 0, 4);
  text("SLOW", 598, 34);
}
if (data1 == 2){
  textFont(f3);
  fill(0);
  text("WALK", 599, 34);
}
if (data1 == 3){
  textFont(f3);
  fill(0);
  text("FAST", 604, 34);
}
if (data1 == 4){
  textFont(f3);
  fill(0, 115, 206);
  text("MANIC", 595, 34);
}

// Safety bypass toggle indicator 
if (data2  == 0){
    PImage safePic = loadImage("6-6.png"); 
    image(safePic,332,7);
}
else {
    PImage dangerPic = loadImage("6-5.png"); 
    image(dangerPic,332,7);
}

// diagonal reducer status indicator 
if (data11 == 1){
    PImage fullPic = loadImage("9-4.png"); 
    image(fullPic,5,7);
}
else {
    PImage reducedPic = loadImage("9-3.png"); 
    image(reducedPic,5,7);
}
}

void keyPressed(){ 
  
  switch (key) {
      case 'a':
      Port.write(34);  // output relay 1 
      break;
      case 'b':
      Port.write(35);// output relay 2 (mode on my wheelchair) 
      break;
      case 'c':
      Port.write(36); // output relay 2 mode x 5
      break;
      case 'd':
      lastBut = 1;
      Port.write(37); //nw direction button press 
      break;
      case 'e':
      lastBut = 2;
      Port.write(38);  //n direction button press 
      break;
      case 'f':
      lastBut = 3;
      Port.write(39);  //ne direction button press 
      break;
      case 'g':
      lastBut = 4;
      Port.write(40);  //e direction button press 
      break;
      case 'h':
      lastBut = 5;
      Port.write(41);  //se direction button press 
      break;
      case 'i':
      lastBut = 6;
      Port.write(42);  //s direction button press 
      break;
      case 'j':
      lastBut = 7;
      Port.write(43);  //sw direction button press 
      break;
      case 'k':
      lastBut = 8;
      Port.write(44);  //w direction button press 
      break;
      case 'l':
      Port.write(45); // safety bypass toggle 
      break;
      case 'm':
      Port.write(46); // nudge left 
      break;
      case '+':
      Port.write(77); //right nudge 
      break;
      case 'n':
      Port.write(47); //continue button press 
      break;
      case 'o':
      Port.write(48); //aux
      break;
      case 'p':
      Port.write(49); //stop 
      break;
      case 'q':
      Port.write(50); //trim n
      break;
      case 'r':
      Port.write(51); //trim e
      break;
      case 's':
      Port.write(52); //trim s
      break;
      case 't':
      Port.write(53); //trim w
      break;
      case 'v':
      Port.write(55); // speed 1
      break;
      case 'w':
      Port.write(56); //  speed 2
      break;
      case 'x':
      Port.write(57); //  speed 3
      break;
      case 'y':
      Port.write(58); //  speed 4
      break;
      case '1':
      Port.write(60); // ns duration =  half 
      break;
      case '2':
      Port.write(61); // ns duration =  1
      break;
      case '3':
      Port.write(62); // ns duration =  2
      break;
      case '4':
      Port.write(63); // ns duration =  3
      break;
      case '5':
      Port.write(64); // ns duration =  4
      break;
      case 'u':
      Port.write(54); // ns duration =  5
      break;
      case '6':
      Port.write(65); // ns duration =  6
      break;
      case '7':
      Port.write(66); // ew duration =  half 
      break;
      case '8':
      Port.write(67); // ew duration =  1
      break;
      case '9':
      Port.write(68); // ew duration =  1.5
      break;
      case '0':
      Port.write(69); // ew duration =  2
      break;
      case '.':
      Port.write(70); // ew duration =  3
      break;
      case ',':
      Port.write(71); //  ew duration =  4
      break;
      case '/':
      Port.write(72); //  ew duration =  5
      break;
      case ';':
      Port.write(73); // ySpeedTrim up one 
      break;
      case '#':
      Port.write(74); // ySpeedTrim down one 
      break;
      case '[':
      Port.write(75); //save settings and exit 
      delay (500); 
      exit(); 
      break;
      case 'z':
      Port.write(59); //overCorrect up one degree 
      break;
      case '@':
      Port.write(76); //overCorrect down one degree 
      break;
      case '£':
      Port.write(78); //diagonal reducer toggle 
      break;
  }
}

void serialEvent (Serial Port) {    //receive the USB data from Arduino 


    String receivedArduino = Port.readStringUntil('\n'); // read the buffer
    
    receivedArduino = trim(receivedArduino); //erase ALL the possible spaces between the letters 
 
    int data[] = int(split(receivedArduino, ',')); //this function splits the string and put a comma between the data
    //it also convert the string in "int"
    
    data0 = data[0];    //data 0 received from Arduino
    data1 = data[1];    //data 1 received from Arduino
    data2 = data[2];    //data 3 received from Arduino
    data3 = data[3];    //data 4 received from Arduino
    data4 = data[4];    //data 5 received from Arduino
    data5 = data[5];    //data 6 received from Arduino
    data6 = data[6];    //data7 received from Arduino speedStateNS
    data7 = data[7];    //data8 received from Arduino joystickStateNS
    data8 = data[8];    //data9 received from Arduino durationTimeNS
    data9 = data[9];    //data10 received from Arduino continueStateNS
    data10 = data[10];    //data11 received from Arduino driveSwitchStateEW
    data11 = data[11];    //data12 received from Arduino speedStateEW
    data12 = data[12];    //data13 received from Arduino diagonalReducer
    data13 = data[13];    //data14 received from Arduino durationTimeEW
    data14 = data[14];    //data15 received from Arduino continueStateEW
    data15 = data[15];    //data16 received from Arduino spare 
    // add two linefeed after all the sensor values are printed:
    println();
    println();
    
    
    Port.write("!"); // send a byte (! - 33) to ask for more data 
    
    
}
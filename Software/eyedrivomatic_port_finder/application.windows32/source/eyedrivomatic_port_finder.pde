/*
Eyedrivomatic Port Finder

This program is used during the initial setup of eyedrivomatic on your pc.
When you plug in a brain box to your pc, your pc assigns a com port number to that particular 
brain box. This app finds that number and stores it on your pc, ready for the main eyedrivomatic 
app to use.

You only have to run this program once, for any particular pc/brain box combination.

This app was written by, and is used with kind permission of W.A. Smith, http://startingelectronics.com
It was modified for eyedrivomatic by Steve Thomas 

*/
import processing.serial.*;

Serial ser_port;                // for serial port
PFont fnt;                      // for font
int num_ports;
boolean device_detected = false;
String[] port_list;
String detected_port = "";

void setup() {
    size(400, 200);                         // size of application window
    background(0);                          // black background
    fnt = createFont("Arial", 16, true);    // font displayed in window
    
    println(Serial.list());
    
    // get the number of detected serial ports
    num_ports = Serial.list().length;
    // save the current list of serial ports
    port_list = new String[num_ports];
    for (int i = 0; i < num_ports; i++) {
        port_list[i] = Serial.list()[i];
    }
}

void draw()
{
    background(0);
    // display instructions to user
    textFont(fnt, 14);
    text("1. Brain box must be unplugged from the PC.", 20, 30);
    text("   (unplug brain box and restart this application if not)", 20, 50);
    text("2. Plug the brain box into a USB port.", 20, 80);
    
    // see if Arduino or serial device was plugged in
    if ((Serial.list().length > num_ports) && !device_detected) {
        device_detected = true;
        // determine which port the device was plugge into
        boolean str_match = false;
        if (num_ports == 0) {
            detected_port = Serial.list()[0];
        }
        else {
            for (int i = 0; i < Serial.list().length; i++) {  // go through the current port list
                for (int j = 0; j < num_ports; j++) {             // go through the saved port list
                    if (Serial.list()[i].equals(port_list[j])) {
                        break;
                    }
                    if (j == (num_ports - 1)) {
                        str_match = true;
                        detected_port = Serial.list()[i];
                    }
                }
            }
        }
    }
    // calculate and display serial port name
    if (device_detected) {
        text("Brain box detected:", 20, 110);
        textFont(fnt, 18);
        text(detected_port, 20, 150);
        text("You may exit this app", 20, 190);
        writeConfig(detected_port);
    }
}


void writeConfig(String value) { //writes a value to the config file
  
  JSONObject json;
  String dir =  System.getenv("APPDATA");

  json = new JSONObject();
  json.setString("portName", value);
  saveJSONObject(json, dir + "/eyedrivomatic/config.json");
} 
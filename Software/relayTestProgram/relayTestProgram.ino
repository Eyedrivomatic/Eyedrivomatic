
int relaypin1 = 4; 
int relaypin2 = 5; 
int relaypin3 = 6; 
int relaypin4 = 7; 

void setup() {
  // initialize digital pin 13 as an output.
  pinMode(relaypin1, OUTPUT);
  pinMode(relaypin2, OUTPUT);
  pinMode(relaypin3, OUTPUT);
  pinMode(relaypin4, OUTPUT);
}
void loop() {
  digitalWrite(relaypin1, HIGH);   // turn the LED on (HIGH is the voltage level)
  delay(1000);              // wait for a second
  digitalWrite(relaypin1, LOW);    // turn the LED off by making the voltage LOW
  delay(1000);              // wait for a second
  digitalWrite(relaypin2, HIGH);   // turn the LED on (HIGH is the voltage level)
  delay(1000);              // wait for a second
  digitalWrite(relaypin2, LOW);    // turn the LED off by making the voltage LOW
  delay(1000);              // wait for a second
  digitalWrite(relaypin3, HIGH);   // turn the LED on (HIGH is the voltage level)
  delay(1000);              // wait for a second
  digitalWrite(relaypin3, LOW);    // turn the LED off by making the voltage LOW
  delay(1000);              // wait for a second
  digitalWrite(relaypin4, HIGH);   // turn the LED on (HIGH is the voltage level)
  delay(1000);              // wait for a second
  digitalWrite(relaypin4, LOW);    // turn the LED off by making the voltage LOW
  delay(1000);              // wait for a second
}

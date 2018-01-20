#include <ArduinoJson.h>
#include <Adafruit_MotorShield.h>

Adafruit_MotorShield AFMS = Adafruit_MotorShield(); 
Adafruit_DCMotor *motorsLeft = AFMS.getMotor(1);
Adafruit_DCMotor *motorsRight = AFMS.getMotor(4);

const unsigned int pulseTimeout = 50000; // in µsec

const int trigPin = 7;
const int echoPin1 = 6;
const int echoPin2 = 5;
const int echoPin3 = 4;
const int echoPin4 = 3;
const int echoPin5 = 2;

unsigned long int lastMillis = 0;
unsigned long int lastReceived = 0;

void setup() {
  // initialize serial communication:
  Serial.begin(115200);

  // initialize motor shield
  AFMS.begin();
  
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin1, INPUT);
  pinMode(echoPin2, INPUT);
  pinMode(echoPin3, INPUT);
  pinMode(echoPin4, INPUT);
  pinMode(echoPin5, INPUT);

  motorsLeft->setSpeed(0);
  motorsRight->setSpeed(0); 
  motorsLeft->run(RELEASE);
  motorsRight->run(RELEASE);
}

void loop() {

  if (Serial.available()) {
    StaticJsonBuffer<200> jsonBuffer;
    
    String json = Serial.readStringUntil('\n');
    JsonObject& root = jsonBuffer.parseObject(json);
    
    if (!root.success()) {
      Serial.println("parseObject() failed");
      return;
    }

    int motorSpeedLeft = root["speeds"][0];
    int motorSpeedRight = root["speeds"][1];

    motorsLeft->setSpeed(abs(motorSpeedLeft));
    motorsRight->setSpeed(abs(motorSpeedRight));
    if (motorSpeedLeft < 0) {
      motorsLeft->run(FORWARD);      
    } else if (motorSpeedLeft > 0) {
      motorsLeft->run(BACKWARD);
    }

    if (motorSpeedRight < 0) {
      motorsRight->run(FORWARD);      
    } else if (motorSpeedRight > 0) {
      motorsRight->run(BACKWARD);
    }

    lastReceived = millis();
  }

  if (millis() - lastMillis >= 50) {
    lastMillis = millis();
    
    long duration1, cm1;
    long duration2, cm2;
    long duration3, cm3;
    long duration4, cm4;
    long duration5, cm5;
  
    digitalWrite(trigPin, LOW);
    delayMicroseconds(2);
    digitalWrite(trigPin, HIGH);
    delayMicroseconds(10);
    digitalWrite(trigPin, LOW);
  
    duration1 = pulseIn(echoPin1, HIGH, pulseTimeout);
    duration2 = pulseIn(echoPin2, HIGH, pulseTimeout);
    duration3 = pulseIn(echoPin3, HIGH, pulseTimeout);
    duration4 = pulseIn(echoPin4, HIGH, pulseTimeout);
    duration5 = pulseIn(echoPin5, HIGH, pulseTimeout);
  
    // convert the time into a distance
    cm1 = microsecondsToCentimeters(duration1);
    cm2 = microsecondsToCentimeters(duration2);
    cm3 = microsecondsToCentimeters(duration3);
    cm4 = microsecondsToCentimeters(duration4);
    cm5 = microsecondsToCentimeters(duration5);

    StaticJsonBuffer<200> jsonBuffer;
  
    // send json document with sensor readings
    JsonObject& root = jsonBuffer.createObject();
    JsonArray& sensors = root.createNestedArray("sensors");
    sensors.add(cm1);
    sensors.add(cm2);
    sensors.add(cm3);
    sensors.add(cm4);
    sensors.add(cm5);

    root.printTo(Serial);
    Serial.println();

    if (millis() - lastReceived >= 1000) {
      motorsLeft->setSpeed(0);
      motorsRight->setSpeed(0);
    }
  }
}

long microsecondsToCentimeters(long microseconds) {
  // The speed of sound is 340 m/s or 29 microseconds per centimeter.
  // The ping travels out and back, so to find the distance of the object we
  // take half of the distance travelled.
  return microseconds / 29 / 2;
}

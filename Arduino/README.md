# NEATrobot
The code running on the Arduino which queries the ultrasonic sensors and the distance is then calculated and sent to the RPi via UART and formatted using JSON. The motor speeds are then received from the RPi and sent to the motor shield over I2C and subsequently the motors are set to that speed using PWM.

## Libraries used
- [Arduino JSON](https://github.com/bblanchon/ArduinoJson)
- [Motorshield driver](https://github.com/adafruit/Adafruit_Motor_Shield_V2_Library)

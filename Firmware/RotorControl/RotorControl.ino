#include <Servo.h>

Servo esc_1;
Servo esc_2;
Servo esc_3;
Servo esc_4;
Servo esc_5;
Servo esc_6;

int rotors[6] = {0, 0, 0, 0, 0, 0};

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  Serial.begin(115200);
  esc_1.attach(2, 1000, 2000);
  esc_2.attach(3, 1000, 2000);
  esc_3.attach(4, 1000, 2000);
  esc_4.attach(5, 1000, 2000);
  esc_5.attach(6, 1000, 2000);
  esc_6.attach(7, 1000, 2000);

  Serial.setTimeout(100);
}

void loop() {
  if (Serial.available() > 0)
  {
    digitalWrite(LED_BUILTIN, HIGH);
    if (Serial.read() == 255)
    {
      rotors[0] = Serial.read(); rotors[1] = Serial.read();
      rotors[2] = Serial.read(); rotors[3] = Serial.read();
      rotors[4] = Serial.read(); rotors[5] = Serial.read();
    }
    esc_1.write(rotors[0]);
    esc_2.write(rotors[1]);
    esc_3.write(rotors[2]);
    esc_4.write(rotors[3]);
    esc_5.write(rotors[4]);
    esc_6.write(rotors[5]);
  }
  digitalWrite(LED_BUILTIN, LOW);
}

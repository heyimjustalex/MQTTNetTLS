import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BCM)
pin = 24
GPIO.setup(pin, GPIO.OUT, initial= GPIO.LOW)

freq = 1000*2

print("Buzzer-test")

try:
  while True:
    GPIO.output(pin, GPIO.HIGH)
    time.sleep(1/freq)
    GPIO.output(pin, GPIO.LOW)
    time.sleep(1/freq)

except KeyboardInterrupt:
  GPIO.cleanup()

# Hardware setup

The client is using MQ-135 air quality sensor and is interfacing it through MCP3008 ADC. Datasheets for these two components can be found at:
MQ-135: https://pdf1.alldatasheet.com/datasheet-pdf/download/1132552/HANWEI/MQ135.html
MCP3008: https://www.farnell.com/datasheets/808965.pdf
RaspberryPi3 pinout will also come in handy: https://raspberrytips.com/raspberry-pi-gpio-pinout/#:~:text=The%20Raspberry%20Pi%20Pinout%20Diagram%201%20Introduction%20All,the%20top-left%20corner.%20...%203%20Full%20diagram%20

The ADC connects to Raspberry pi via an SPI interface. The pin connections are as follows:

MCP3008 | RaspberryPI

9:D_GND - 6:GND  
10:CS - 24:SPI_C0  
11:D_IN - 19:SPI0_MOSI  
12:D_OUT - 21:SPI0_MISO  
13:CLK - 23:SPI_CLK  
14:A_GND - 6:GND  
15:VREF - 1:3.3V  
16:VDD - 1:3.3V  
  
Additionally, the Client is hardcoded to only read from channel 0 of ADC, so sensor output must go into channel 0  (pin 1) of the ADC.

The sensor itself has to be either powered by RaspberryPi (5 V on a different line then ADC 3.3V VDD), or can be powered by an external power supply. GND has to go to common GND and the output has to go to pin 1 of ADC.

using Client.Sensor;
using Client.SensorBase;
using Client.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.SensorService
{
    internal class SensorBuzzerService
    {
        ISensorGetSetCheckData buzzer;
        public SensorBuzzerService() {

            buzzer = new Buzzer();        
        }

        public SensorData getBuzzerState()
        {
            return buzzer.get();
        }

        public bool isBuzzerEnabled()
        {
            return buzzer.check();
        }

        public void setBuzzerState(bool state)
        {
            string isOn = state ? "TRUE" : "FALSE";
            SensorData data = new SensorData("BUZZER", isOn);
            buzzer.set(data);          
        }
    }
}

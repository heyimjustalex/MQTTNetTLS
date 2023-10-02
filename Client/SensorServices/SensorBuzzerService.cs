using Client.Sensor;
using Client.SensorBase;
using Client.Sensors;
using Client.SensorServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.SensorService
{
    internal class SensorBuzzerService : ISensorService
    {
        ISensorGetSetCheckData buzzer;
        public SensorBuzzerService() {

            buzzer = new Buzzer();        
        }

        public SensorData get()
        {
            return buzzer.get();
        }

        public bool check()
        {
            return buzzer.check();
        }

        public void set(bool state)
        {
            if (buzzer.check() != state)
            {
                string isOn = state ? "TRUE" : "FALSE";
                SensorData data = new SensorData("BUZZER", isOn);
                buzzer.set(data);
            }
            
        }
    }
}

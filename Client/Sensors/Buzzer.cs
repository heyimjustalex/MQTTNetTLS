using Client.Sensor;
using Client.SensorBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Sensors
{
    internal class Buzzer : ISensorGetSetCheckData
    {
        String state;
        public Buzzer() {
            state = "FALSE";

            // States are supposed to be TRUE for buzzing and FALSE for no buzzing (WITH GREAT LETTERS TRUE and FALSE)
            // YOU CAN REMOVE STATE VARIABE CUZ IT'S ONLY FOR MY TESTS
        }

        public bool check()
        {

            return get().ParameterValue == "TRUE";
        }

        public SensorData get()
        {
            // Here you get state from buzzer pins  
            // string state = doesHardwareSayMyBuzzerIsBuzzing() == true ? "TRUE" : "FALSE"
          
            return new SensorData("BUZZER", state);
        }
     

        public void set(SensorData sensorData)
        {   
                // Here you implement setting the buzzer pins ON, so the buzzer starts buzzing
                state = sensorData.ParameterValue; 
                      
        }


    }
}

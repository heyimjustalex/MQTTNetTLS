using Client.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Sensors
{
    internal class Buzzer : ISensorSetData, ISensorGetData
    {
        String state;
        public Buzzer() {
            state = "TRUE";

            // States are supposed to be TRUE for buzzing and FALSE for no buzzing (WITH GREAT LETTERS TRUE and FALSE)
            // YOU CAN REMOVE STATE VARIABE CUZ IT'S ONLY FOR MY TESTS
        } 
        public SensorData get()
        {
            // Here you get state from buzzer pins  
            // string state = doesHardwareSayMyBuzzerIsBuzzing() == true ? "TRUE" : "FALSE"
          
            return new SensorData("BUZZER", state);
        }

        public bool isBuzzing()
        {
            return get().ParameterValue == "TRUE";
        }

        public void set(SensorData sensorData)
        {   
                // Here you implement setting the buzzer pins ON, so the buzzer starts buzzing
                state = sensorData.ParameterValue; 
                      
        }


    }
}

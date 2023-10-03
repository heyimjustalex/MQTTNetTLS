using Client.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Sensors
{
    internal class SmokeDetector : ISensorGetData
    {
       
        public SensorData get()
        {
            // Instead of generating random values you need to implement getting SmokeDetector state and returning data in form of
            // return new SensorData("SMOKE", "TRUE" or "FALSE);
            //return new SensorData("SMOKE", "FALSE");
            string username = Environment.GetEnvironmentVariable("USERNAME");
            string dockerMockSensorState = null;
            if (username != null)
            {
                dockerMockSensorState = Environment.GetEnvironmentVariable(username+"_MockedSmokeSensorState");
              
                    
            }


            if(dockerMockSensorState!=null)
            {
            
                if (dockerMockSensorState=="TRUE")
                {
                    return new SensorData("SMOKE", "TRUE");
                }
                else if(dockerMockSensorState=="FALSE")
                {
                    return new SensorData("SMOKE", "FALSE");

                }
                else {
                    Random randomizer = new Random();
                    var isThereSmokeBool = randomizer.Next() % 4 == 0 ? "TRUE" : "FALSE";
                    return new SensorData("SMOKE", isThereSmokeBool);
                }
            }
           

        // if no docker env variable set just go random

           Random random = new Random();         
           var isThereSmoke= random.Next() % 8 == 0 ? "TRUE" : "FALSE";
           return new SensorData("SMOKE", isThereSmoke);
        }
    }
}

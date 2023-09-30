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
         //   return new SensorData("SMOKE", "TRUE");
            Random random = new Random();           
            var isThereSmoke= random.Next() % 2 == 0 ? "TRUE" : "FALSE";
            return new SensorData("SMOKE", isThereSmoke);
        }
    }
}

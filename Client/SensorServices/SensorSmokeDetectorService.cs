using Client.Sensor;
using Client.Sensors;
using Client.SensorServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.SensorService
{
    internal class SensorSmokeDetectorService : ISensorService
    {
        ISensorGetData smokeDetector;

        public SensorSmokeDetectorService() {

            smokeDetector = new SmokeDetector();            
        
        }
        public SensorData get()
        {
            return smokeDetector.get();
        }
    }
}

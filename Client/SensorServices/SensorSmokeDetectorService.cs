using Client.Sensor;
using Client.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.SensorService
{
    internal class SensorSmokeDetectorService
    {
        ISensorGetData smokeDetector;

        public SensorSmokeDetectorService() {

            smokeDetector = new SmokeDetector();            
        
        }

        public SensorData getSmokeDetectorState()
        {
            return smokeDetector.get();
        }
    }
}

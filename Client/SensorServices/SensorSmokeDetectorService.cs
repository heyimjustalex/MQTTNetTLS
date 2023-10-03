using Client.SensorBase;
using Client.Sensors;
using Client.SensorServices;

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

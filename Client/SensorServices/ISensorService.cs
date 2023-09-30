using Client.Sensor;

namespace Client.SensorServices
{
    internal interface ISensorService
    {
        public SensorData get();
    }
}

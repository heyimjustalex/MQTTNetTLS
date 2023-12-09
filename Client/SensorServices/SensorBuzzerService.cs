using Client.SensorBase;
using Client.Sensors;
using Client.SensorServices;

namespace Client.SensorService
{
    internal class SensorBuzzerService : ISensorService
    {
        Buzzer buzzer;
        public SensorBuzzerService() {

            buzzer = new Buzzer();        
        }

        public SensorData get()
        {
            return new SensorData("Buzzer", "Is not sensor");
        }

        public bool check()
        {
            return true;
        }

        public void set(bool state)
        {
            buzzer.set(state);
        }
    }
}

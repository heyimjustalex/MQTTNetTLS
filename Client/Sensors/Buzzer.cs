using Client.SensorBase;

namespace Client.Sensors
{
    internal class Buzzer : ISensorGetSetCheckData
    {
        String _state;
        public Buzzer() {
            _state = "FALSE";

            // States are supposed to be TRUE for buzzing and FALSE for no buzzing (WITH GREAT LETTERS TRUE and FALSE (and are strings))

        }

        public bool check()
        {
            return get().ParameterValue == "TRUE";
        }
        public SensorData get()
        {
            // Here you get state from buzzer pins  
            // _state = doesHardwareSayMyBuzzerIsBuzzing() == true ? "TRUE" : "FALSE"
            // WHEN YOU'RE DONE IMPLEMENTING YOU CAN JUST REMOVE _state VARIABLE, IT'S USELESS IF get() GETS DATA DIRECLTY FROM HARDWARE

            return new SensorData("BUZZER", _state);
        }    
        public void set(SensorData sensorData)
        {   
                // Here you implement setting the buzzer pins ON, so the buzzer starts buzzing
                _state = sensorData.ParameterValue;                       
        }
    }
}

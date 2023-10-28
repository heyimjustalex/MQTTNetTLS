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
            // just leave it like that
            return new SensorData("BUZZER", _state);
        }    
        public void set(SensorData sensorData)
        {   
                // here you save your state in the object
                // and keep it like that cuz in get you have to get this value from _state
                _state = sensorData.ParameterValue;


            // Here you have to implement setting the buzzer pins ON/OFF, so the buzzer starts buzzing
            // I would do sth like

            if(sensorData.ParameterValue == "TRUE")
            {
                // set buzzer hardware pin ON
            }
            else
            {
                // set buzzer hardware pin off
            }

            // this should be enough, cuz set and get functions are invoked within other code. No need to implement anything else in SmokeDetector
        }
    }
}

using Client.SensorBase;
using System.Device.Gpio;
using System.Diagnostics;

// Buzzer is not a sensor so it shouldn't implement sensor methods
namespace Client.Sensors
{
    internal class Buzzer
    {
        // static GpioController controller;
        Process activeProcess;
        const string buzz_script = "Client/Sensors/square_wave.py";
        const string python_path = "/usr/bin/python3.9";
        bool state_;
        public Buzzer() {
            bool state_ = false;
            // States are supposed to be TRUE for buzzing and FALSE for no buzzing (WITH GREAT LETTERS TRUE and FALSE (and are strings))
        }

        // Sets the buzzer on or off depending on parameter (true is on)
        // If trying to set to the same state as before, early return
        public void set(bool state)
        {   
            Console.WriteLine("buzzer.set called");
            if(state == state_) return;
            state_ = state;
            if(state){
                send_square_wave();
            }
            if(!state){
                activeProcess.Kill();
            }
        }

        private void send_square_wave(){
            Console.WriteLine("Start python send sqr wave script");
            try{
                activeProcess = Process.Start(python_path, buzz_script);
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
            }
        }
    }
}

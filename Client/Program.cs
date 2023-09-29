using System;
using System.Threading.Tasks;
using Client.MqttManager;
using Client.SensorService;

namespace Program
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SensorBuzzerService buzzerService = new SensorBuzzerService();
            SensorSmokeDetectorService smokeDetectorService = new SensorSmokeDetectorService();
            MqttClientConfiguration configuration = new MqttClientConfiguration(id: "alarm1", ipAddress: "localhost", port: 8883, username:"client1", password:"password1");
            MqttManager mqttManager = new MqttManager(configuration);


            await mqttManager.start();
                       
            Console.WriteLine("Client program has started. Press ENTER to end process");
            Console.ReadLine();
        }

    }

}

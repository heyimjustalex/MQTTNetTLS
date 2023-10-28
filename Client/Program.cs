using Client.Configuration;
using Client.MQTTCommunicationController;
using Client.SensorService;

namespace Program
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SensorBuzzerService buzzerService = new SensorBuzzerService();
            SensorSmokeDetectorService smokeDetectorService = new SensorSmokeDetectorService();                   

            string username =  Environment.GetEnvironmentVariable("USERNAME");
            string password=  Environment.GetEnvironmentVariable("PASSWORD");
            string clientID = Environment.GetEnvironmentVariable("CLIENT_ID");

            string dockerMockSensorState = null;

            // If it works in docker you can mock the return value by setting state client1_MockedSmokeSensorState = TRUE or FALSE or RANDOM
            if (username != null)
            {
                dockerMockSensorState = Environment.GetEnvironmentVariable(username + "_MockedSmokeSensorState");
            }

            if (dockerMockSensorState != null)
            {
                Console.WriteLine($"Smoke sensor management mocked by docker ENV State:{dockerMockSensorState}");
            }

            // Because broker uses WPF (it cannot be started on Linux) it needs Windows ENV variable to set its IP
            // If you want to set IP of wifi network adapter just use Powershell script provided in the solution AND RESTART VISUAL STUDIO
          
            string brokerIP = Environment.GetEnvironmentVariable("BROKER_IP_ADDRESS");

            if (brokerIP == null)
            {
                brokerIP = "localhost";
            }  
 
            Console.WriteLine($"Data loaded through ENV: brokerIP:{brokerIP}, username:{username}, password:{password}, clientID:{clientID}");

            MqttClientConfiguration configuration = new MqttClientConfigurationBuilder()
                .WithPort(8883)
                .WithIpAddress(brokerIP)
                .WithId(clientID)
                .WithUsername(username)
                .WithPassword(password)
                .WithTopicsClientEnqueuesTo(new string[] { "alarm/fromClient" })
                .WithTopicsClientSubscribesTo(new string[] { "alarm/fromBroker" })
                .Build();
                   

            MQTTCommunicationController mqttManager = new MQTTCommunicationController(configuration, buzzerService, smokeDetectorService);

            await mqttManager.start();

            Console.WriteLine("Client program has started. Press ENTER to end process");
            Console.ReadLine();
        }

    }

}

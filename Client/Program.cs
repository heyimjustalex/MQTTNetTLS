using Client.MqttManager;
using Client.MqttManager.Configuration;
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

                MqttClientConfiguration configuration = new MqttClientConfigurationBuilder()
                .WithPort(8883)
                .WithIpAddress("192.168.5.166")
                .WithId(clientID)
                .WithUsername(username)
                .WithPassword(password)
                .WithTopicsClientEnqueuesTo(new string[] { "alarm/fromClient" })
                .WithTopicsClientSubscribesTo(new string[] { "alarm/fromBroker" })
                .Build();
                   

            MqttManager mqttManager = new MqttManager(configuration, buzzerService, smokeDetectorService);

            await mqttManager.start();

            Console.WriteLine("Client program has started. Press ENTER to end process");
            Console.ReadLine();
        }

    }

}

using System;
using System.Threading.Tasks;
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

            //MqttClientConfiguration configuration = new MqttClientConfigurationBuilder()
            // .WithPort(8883)
            // .WithIpAddress("localhost")
            // .WithId("alarm1")
            // .WithUsername("client1")
            // .WithPassword("password1")
            // .WithTopicsClientEnqueuesTo(new string[] { "alarm/fromClient" })
            // .WithTopicsClientSubscribesTo(new string[] { "alarm/fromBroker" })
            // .Build();
          string username =  Environment.GetEnvironmentVariable("USERNAME");
          string password=  Environment.GetEnvironmentVariable("PASSWORD");
          string clientID = Environment.GetEnvironmentVariable("CLIENT_ID");

          Console.WriteLine($"{username}{password}{clientID}");

         MqttClientConfiguration configuration = new MqttClientConfigurationBuilder()
         .WithPort(8883)
         .WithIpAddress("192.168.5.166")
         .WithId(clientID)
         .WithUsername(username)
         .WithPassword(password)
         .WithTopicsClientEnqueuesTo(new string[] { "alarm/fromClient" })
         .WithTopicsClientSubscribesTo(new string[] { "alarm/fromBroker" })
         .Build();

           //MqttClientConfiguration configuration = new MqttClientConfigurationBuilder()
           //.WithPort(8883)
           //.WithIpAddress("127.0.0.2")
           //.WithId("alarm3")
           //.WithUsername("client3")
           //.WithPassword("password3")
           //.WithTopicsClientEnqueuesTo(new string[] { "alarm/fromClient" })
           //.WithTopicsClientSubscribesTo(new string[] { "alarm/fromBroker" })
           //.Build();

            MqttManager mqttManager = new MqttManager(configuration, buzzerService, smokeDetectorService);

            await mqttManager.start();

            Console.WriteLine("Client program has started. Press ENTER to end process");
            Console.ReadLine();
        }

    }

}

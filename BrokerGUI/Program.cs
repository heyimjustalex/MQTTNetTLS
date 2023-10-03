using System;
using System.Net;
using System.Threading.Tasks;
using Broker.Configuration;
using Broker.Database;
using Broker.PKI;
using Broker.Repository;
using Broker.Service;
using Broker.MqttManager;
using System.Reflection.PortableExecutable;
using System.Threading;

namespace BrokerGUI
{
    class Program
    {
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.11.0")]
        public static void Main()
        {
            UI.App app = new UI.App();

            app.InitializeComponent();
            app.Run();
        }

        public static async Task<Task> Broker(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting new broker");
            IClientDB clientDB = new ClientDB();
            IClientRepository clientRepository = new ClientRepository(clientDB);
            IClientAccountService clientService = new ClientAccountService(clientRepository);

            string serverCertPath = "../../../PKI/Broker/broker1.pfx";
            string keyCertPath = "../../../PKI/Broker/key1.pem";

            var certificate = PKIUtilityStatic.ReadCertificateWithPrivateKey(serverCertPath, keyCertPath, "password");

            
            string brokerIP = Environment.GetEnvironmentVariable("MY_IP_ADDRESS");
            
            if(brokerIP == null ) {
                brokerIP = "localhost";
            }

            Console.WriteLine($"Using {brokerIP} as broker IP address");

            MqttBrokerConfiguration configuration = new MqttBrokerConfigurationBuilder()
            .WithPort(8883)
            .WithIpAddress(brokerIP)
            .WithId("broker1")
            .WithTopicsBrokerEnqueuesTo(new string[] { "alarm/fromBroker" })
            .WithTopicsBrokerSubscribesTo(new string[] { "alarm/fromClient" })
            .WithCertificate(certificate)
            .Build();

            MqttManager mqttManager = new MqttManager(configuration, clientService);
      
           
            await  mqttManager.start(cancellationToken);

            mqttManager.kill();
            Console.WriteLine("Broker has been killed");
            Thread.Sleep(1000);
            Console.Clear();
            return Task.CompletedTask;
         
        }
    }
}

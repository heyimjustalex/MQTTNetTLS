using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;

namespace Client
{
    class Program
    {
        private static bool isConnected = false;
        public static X509Certificate2 ReadCertificateFromFile(string filePath, string password = null)
        {
            try
            {
                return new X509Certificate2(filePath, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CA certificate: {ex.Message}");
                return null;
            }
        }
        private static bool OnCertificateValidation(MqttClientCertificateValidationEventArgs args)
        {

            X509Certificate2 serverCertificate = new X509Certificate2(args.Certificate);
            X509Certificate2 CACertificate = ReadCertificateFromFile("../../../PKI/CA/RootCA.cer");

            try
            {
                args.Chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                args.Chain.ChainPolicy.CustomTrustStore.Add(CACertificate);
                args.Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                var chain = args.Chain.Build(serverCertificate);

                args.Chain.ChainStatus.ToList().ForEach(x => { Console.WriteLine(x.Status.ToString()); });
                if (chain)
                {

                    Console.WriteLine("OnCertificateValidation: Building certificate chain success");
                    return true;
                }
                else
                {
                    Console.WriteLine("OnCertificateValidation: Building certificate chain FAILURE");
                    return false;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                return false;
            };



        }
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();          


            while (true)
            {
                using (var managedMqttClient = mqttFactory.CreateManagedMqttClient())
                {                  
              
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithClientId("alarm1")
                        .WithTcpServer("localhost", 8883)
                        .WithCleanSession()
                        .WithCredentials("alarm1","password")
                        .WithKeepAlivePeriod(new TimeSpan(0,0,5)) // how much time before assuming connection failure
                        
                        .WithTlsOptions(new MqttClientTlsOptionsBuilder()
                                        .WithSslProtocols(SslProtocols.Tls12)
                                        .WithCertificateValidationHandler(OnCertificateValidation)
                                        .WithAllowRenegotiation(true)
                                        .WithCipherSuitesPolicy(System.Net.Security.EncryptionPolicy.RequireEncryption)

                                        .Build())
                        .Build();

                    var mqttManagedClientOptions = new ManagedMqttClientOptionsBuilder()
                                                    .WithClientOptions(mqttClientOptions)
                                                    //.WithPendingMessagesOverflowStrategy(MQTTnet.Server.MqttPendingMessagesOverflowStrategy.DropOldestQueuedMessage)
                                                    
                                                    .Build();

                    managedMqttClient.DisconnectedAsync += OnDisconnectAsync;
                    managedMqttClient.ConnectedAsync += OnConnectAsync;
                    managedMqttClient.ConnectingFailedAsync += OnConnectFailedAsync;


                    await StartAsync(managedMqttClient, mqttManagedClientOptions);                   

                }
            }

            Console.Read();
        }
              
        private static async Task StartAsync(IManagedMqttClient managedMqttClient, ManagedMqttClientOptions mqttManagedClientOptions)
        {
            Console.WriteLine("Task StartAsync: Trying to start client");
          
                try
                {
                    await managedMqttClient.StartAsync(mqttManagedClientOptions);
                    Console.WriteLine("Task StartAsync: I made try to connect");

              
            }
                catch (Exception ex)
                {
                    Console.WriteLine($"Task StartAsync: Failed to start MQTT client: {ex.Message}");
                 
                }
            
        }
        public static async Task OnDisconnectAsync(MqttClientDisconnectedEventArgs args)
        {
            Console.WriteLine("Task OnDisconnectAsync: OnDisconnectAsync was called because connection was lost. Waiting 10s...");

            await Task.Delay(TimeSpan.FromSeconds(10));

            Console.WriteLine("Task OnDisconnectAsync: Exiting OnDisconnectAsync");
        }

        public static async Task OnConnectAsync(MqttClientConnectedEventArgs args)
        {
            Console.WriteLine("Task OnConnectAsync: Success connecting to broker. Waiting 10s ...");

            // here start publish logic

            await Task.Delay(TimeSpan.FromSeconds(-1));

            Console.WriteLine("Task OnConnectAsync: Ending OnConnectAsync");
        }


        public static async Task OnConnectFailedAsync(ConnectingFailedEventArgs args)
        {
            Console.WriteLine("Task OnConnectFailedAsync: Calling OnConnectFailedAsync... Waiting 10s... ");

            // Implement your custom reconnection logic here.
            await Task.Delay(TimeSpan.FromSeconds(10)); // Example delay.

            Console.WriteLine("Task OnConnectFailedAsync: End of OnConnectFailedAsync");
        }



    }

}

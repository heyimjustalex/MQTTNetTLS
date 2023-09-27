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
using MQTTnet.Exceptions;

namespace Client
{
    class Client
    {
        MqttFactory mqttFactory;
        public IManagedMqttClient managedMqttClient;
        ManagedMqttClientOptions mqttManagedClientOptions;
        
        public Client()
        {
            mqttFactory = new MqttFactory();
            managedMqttClient = mqttFactory.CreateManagedMqttClient();
            initConfiguration();     

        }

        private void initConfiguration()
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                 .WithClientId("alarm1")
                 .WithTcpServer("localhost", 8883)
                 .WithCleanSession()
                 .WithCredentials("alarm1", "password")
                 .WithKeepAlivePeriod(new TimeSpan(0, 0, 30)) // how much time before assuming connection failure

                 .WithTlsOptions(new MqttClientTlsOptionsBuilder()
                                 .WithSslProtocols(SslProtocols.Tls12)
                                 .WithCertificateValidationHandler(OnCertificateValidation)
                                 .WithAllowRenegotiation(true)
                                 .WithCipherSuitesPolicy(System.Net.Security.EncryptionPolicy.RequireEncryption)

                                 .Build())
                 .Build();

            mqttManagedClientOptions = new ManagedMqttClientOptionsBuilder()
                                            .WithClientOptions(mqttClientOptions)
                                            //.WithPendingMessagesOverflowStrategy(MQTTnet.Server.MqttPendingMessagesOverflowStrategy.DropOldestQueuedMessage)

                                            .Build();

        //    managedMqttClient.DisconnectedAsync += OnDisconnectAsync;
            managedMqttClient.ConnectedAsync += OnConnectAsync;
         //   managedMqttClient.ConnectingFailedAsync += OnConnectFailedAsync;

        }

        private async Task OnDisconnectAsync(MqttClientDisconnectedEventArgs args)
        {
            if (args.Exception != null)
            {
                if (args.Exception is MqttCommunicationException)
                {
                    Console.WriteLine($"Task OnDisconnectAsync: Disconnected due to socket error (probably server is off)");
                }
                else
                {
                    Console.WriteLine($"Task OnDisconnectAsync: Disconnected due to an exception: {args.Exception}");

                }
            }
            else
            {
                Console.WriteLine("Task OnDisconnectAsync: Disconnected for an unknown reason.");
            }

            // Attempt to reconnect only if the client is not already connected
            if (!managedMqttClient.IsConnected && !managedMqttClient.IsStarted)
            {
                Console.WriteLine("Task OnDisconnectAsync: Reconnecting...");

                try
                {
                    // Add a brief delay before attempting to reconnect
                    await Task.Delay(TimeSpan.FromSeconds(10));
                  
                    // Reconnect the MQTT client
                    await managedMqttClient.StartAsync(mqttManagedClientOptions);

                    Console.WriteLine("Task OnDisconnectAsync: Reconnected to the MQTT broker.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Task OnDisconnectAsync: Failed to reconnect: {ex.Message}");
                }
            }
        }

        private async Task OnConnectAsync(MqttClientConnectedEventArgs args)
        {
            Console.WriteLine("Task OnConnectAsync: Success connecting to broker. Waiting 100s ...");

            // here start publish logic

            await Task.Delay(TimeSpan.FromSeconds(10));

            Console.WriteLine("Task OnConnectAsync: Ending OnConnectAsync");
        }

        private async Task OnConnectFailedAsync(ConnectingFailedEventArgs args)
        {
            if (args.Exception != null)
            {
                if (args.Exception is MqttCommunicationException)
                {
                    Console.WriteLine($"Task OnConnectFailedAsync: Disconnected due to socket error (probably server is off)");
                }
                else
                {
                    Console.WriteLine($"Task OnConnectFailedAsync: Connection attempt failed due to an exception: {args.Exception}");
                }
            }
            else
            {
                Console.WriteLine("Task OnConnectFailedAsync: Connection attempt failed for an unknown reason.");
            }

            // Attempt to reconnect only if the client is not already connected if (args.Exception is MqttCommunicationException)
            if (!managedMqttClient.IsConnected && !managedMqttClient.IsStarted)
            {
                Console.WriteLine("Task OnConnectFailedAsync: Reconnecting...");

                try
                {
                    // Add a brief delay before attempting to reconnect
                    await Task.Delay(TimeSpan.FromSeconds(10));

                    // Reconnect the MQTT client
                    await managedMqttClient.StartAsync(mqttManagedClientOptions);

                    Console.WriteLine("Task OnConnectFailedAsync: Reconnected to the MQTT broker.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Task OnConnectFailedAsync: Failed to reconnect: {ex.Message}");
                }
            }
        }

        public async Task start()
        {         
                await managedMqttClient.StartAsync(mqttManagedClientOptions);            
        }
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
           
     
     




    }


    class Program
    {
        static async Task Main(string[] args)
        {
            Client client = new Client();
            await client.start();
            var c = true;
            var t = client.managedMqttClient.IsStarted;
            var g = client.managedMqttClient.IsConnected;
            
            Console.WriteLine("Client program has ended"+t.ToString()+g.ToString());
            Console.ReadLine();
        }

    }

}

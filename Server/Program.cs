using MQTTnet; 
using MQTTnet.Server;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Server
{
    internal class Program
    {
        public static async Task onNewClientConnection(ClientConnectedEventArgs e)
        {
            Console.WriteLine("New Client, calling OnNewClient ");                     
        }



        public static async Task onInterceptingPublishAsync(InterceptingPublishEventArgs args)
        {
            var message = args.ApplicationMessage;
            var topic = message.Topic;
            var payload = Encoding.UTF8.GetString(message.Payload);

            Console.WriteLine($"Received publish request on topic '{topic}': Payload = '{payload}'");

            // Implement your custom logic here.

            // Continue processing the publish request.
            await Task.CompletedTask;
        }
        public static X509Certificate2 ReadCertificateWithPrivateKey(string fileCertPath, string keyPath, string password)
        {
            try
            {
                // Read the private key from key.pem file
                string keyPem = File.ReadAllText(keyPath);

                // Create an RSA key from the private key PEM and password
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportFromEncryptedPem(keyPem, password);
                    // Load the certificate from fileCertPath
                    var certificate = new X509Certificate2(fileCertPath, password);

                    var certificate2 = certificate.CopyWithPrivateKey(rsa);
                    // Associate the private key with the certificate


                    return certificate2;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading certificate with private key: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();


            string serverCertPath = "../../../PKI/Server/server.pfx";
            string keyCertPath = "../../../PKI/Server/key.pem";
            // WITH THIS DOES NOT WORK
            var certificate = ReadCertificateWithPrivateKey(serverCertPath, keyCertPath, "password");


            // WITH THIS DOES WORK

            // var certificate2 = CreateSelfSignedCertificate("1.3.6.1.5.5.7.3.1");

            // var mqttServerOptions = new MqttServerOptionsBuilder().WithEncryptionSslProtocol(System.Security.Authentication.SslProtocols.Tls12).WithEncryptionCertificate(certificate).WithEncryptedEndpoint().Build();

            var mqttServerOptions = new MqttServerOptionsBuilder()
                .WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx))
                .WithEncryptionSslProtocol(System.Security.Authentication.SslProtocols.Tls12)
           
                .WithEncryptedEndpoint()
            
               
                .Build();



            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                mqttServer.ClientConnectedAsync += onNewClientConnection;
                mqttServer.InterceptingPublishAsync += onInterceptingPublishAsync;
             
                try {
               
                    mqttServer.StartAsync().GetAwaiter().GetResult(); 
                    // Expire does not work it's a bug 
                    var message = new MqttApplicationMessageBuilder().WithTopic("alarm/fromBroker").WithMessageExpiryInterval(1000).WithRetainFlag(true).WithPayload("Hello, World from server!").Build();

                    try
                    {
                        Console.WriteLine("SENDING MESSAGE");
                        await mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message));
                        Console.WriteLine("Message sent");
                        Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception here
                        Console.WriteLine($"Error while injecting MQTT message: {ex.Message}");
                        throw;
                    }



                } catch (Exception ex) {
                    Console.WriteLine($"Error when client connecting: {ex.Message} {ex.StackTrace}");
                }

                Console.WriteLine("Waitng for connections \n Press Enter to exit.");
                Console.ReadLine();

                await mqttServer.StopAsync();
            }
        }
    }
}

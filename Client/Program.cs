using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Internal;
using System;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static bool OnCertificateValidation(MqttClientCertificateValidationEventArgs args)
        {
            return true;
         
        }
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            var mqttClient = mqttFactory.CreateMqttClient();
            bool connected = false;

            while (!connected)
            {
                using (var mqtttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost", 8883)
                        .WithTlsOptions(o => o.WithCertificateValidationHandler(OnCertificateValidation)).Build();

                    using (var timeout = new CancellationTokenSource(5000))
                    {
                        try
                        {
                            var response = await mqtttClient.ConnectAsync(mqttClientOptions, timeout.Token);
                            Console.WriteLine("The MQTT client is connected.");
                            Console.WriteLine(response.ResultCode);
                            connected = true; 
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("EXCEPTION CLIENT CANNOT CONNECT");
                            Console.WriteLine(ex);
                            Console.WriteLine(ex.StackTrace);

                            // Delay for 2 seconds before retrying
                            await Task.Delay(2000);
                        }
                    }
                }
            }

            Console.Read();
        }


    }

}

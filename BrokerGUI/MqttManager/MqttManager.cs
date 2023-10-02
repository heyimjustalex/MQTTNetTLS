using Broker.Configuration;
using MQTTnet;
using MQTTnet.Server;
using Broker.Repository;
using Broker.Service;
using System.Security.Cryptography.X509Certificates;
using Broker.PKI;
using System.Threading.Tasks;
using System;
using MQTTnet.Client;
using System.Security.Authentication;
using System.Text;
using System.Net;
using System.Collections.Generic;
using Broker.Entity;
using System.Threading;
using Broker.Database;
using Server.Sensor;
using BrokerGUI;

namespace Broker.MqttManager
{
    class MqttManager
    {
        MqttFactory _mqttFactory;
        MqttServer _mqttServer; 
        MqttBrokerConfiguration _mqttBrokerConfiguration;
        MqttServerOptions _serverOptionsConfiguration;
        List<Client> _currentlyConnectedClients;
        IMessagePublisherService _messagePublisherService;
        IClientService _clientService;              

        public MqttManager(MqttBrokerConfiguration configuration, IClientService clientService)
        {
            _currentlyConnectedClients = new List<Client>();    
            _mqttFactory = new MqttFactory();
            _messagePublisherService = new MessagePublisherService();
            _mqttBrokerConfiguration = configuration;
            _serverOptionsConfiguration = generateBrokerConfigurationOptions();
            _clientService = clientService;
            _mqttServer = _mqttFactory.CreateMqttServer(_serverOptionsConfiguration);
            initBrokerFunctionHandlers();       
        }     

        private MqttServerOptions generateBrokerConfigurationOptions()
        {
            return new MqttServerOptionsBuilder()
                .WithEncryptedEndpointPort(_mqttBrokerConfiguration.Port)
          
                .WithEncryptedEndpointBoundIPAddress(IPAddress.Parse(_mqttBrokerConfiguration.IpAddress))
                .WithEncryptionCertificate(_mqttBrokerConfiguration.Certificate.Export(X509ContentType.Pfx))
                .WithEncryptionSslProtocol(System.Security.Authentication.SslProtocols.Tls12)
                .WithEncryptedEndpoint()
                .Build();        
         }

        private void initBrokerFunctionHandlers()
        {
            _mqttServer.ClientConnectedAsync += onNewClientConnection;
            _mqttServer.InterceptingPublishAsync += onInterceptingPublishAsync;
            _mqttServer.ValidatingConnectionAsync += onClientConnectionValidation;
            _mqttServer.ClientDisconnectedAsync += onClientDisconnect;
        }
        private async Task onClientDisconnect(ClientDisconnectedEventArgs e)
        {
            Console.WriteLine($"Client {e.ClientId} disconnected");
            int i = 0;
            UI.ClientManager.RemoveClientByID(e.ClientId);
            foreach (Client client in _currentlyConnectedClients)
            {
                if(client.clientId== e.ClientId) {
                    _currentlyConnectedClients.RemoveAt(i);     
                }
                i++;
            }        


        }
        private async Task onClientConnectionValidation(ValidatingConnectionEventArgs e)
        {

            string clientId = e.ClientId;            
            string username = e.UserName;           
     
            string password = e.Password?.ToString() ?? string.Empty;
       

            if (clientId == null || username == null || password == null)
            {
                return;
            }

            if (username != null)
            {
                if (!_clientService.authenticate(username, password))
                {
                    Console.WriteLine($"Authenticating with username: {username} FAILED");
                    return;
                }
            }

            Console.WriteLine($"Client {username} {clientId} authenticated, adding it to the currently connected clients");
            // here update the window new client authenticated
            _currentlyConnectedClients.Add(new Client(clientId, username, password));
            UI.ClientManager.AddClient(new UI.ClientGUI(clientId,username,"TRUE")); 

        }
        private async Task onNewClientConnection(ClientConnectedEventArgs e)
        {
            Console.WriteLine("New Client, calling OnNewClient ");          
        }

        private async Task enqueueToAllSpecifiedTopics(List<SensorData> sensorData)
        {        
            string json = System.Text.Json.JsonSerializer.Serialize(new MessageMQTT(DateTime.Now, "broker", sensorData));
                     
            foreach (var topic in _mqttBrokerConfiguration.TopicsBrokerEnqueuesTo)
            {
                var message = new MqttApplicationMessageBuilder()
                  .WithTopic(topic)
                  .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                  .WithMessageExpiryInterval(1000)
                  .WithRetainFlag(true)
                  .WithPayload(json)
                  .Build();                
                   await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message));

            }
        }



        private async Task onInterceptingPublishAsync(InterceptingPublishEventArgs args)
        {
            var message = args.ApplicationMessage;
            var clientId = args.ClientId;

            var payloadSegment = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);


            var r = args;
            var topic = message.Topic;           
            var payload = Encoding.UTF8.GetString(message.Payload);

          

            Console.WriteLine($"Received publish request on topic '{topic}': Payload = '{payload}' {clientId}");

            // Implement your custom logic here.

            // Continue processing the publish request.
            await Task.CompletedTask;
        }

        public void kill()
        {
            _mqttServer.Dispose();
        }
        public async Task start(CancellationToken cancellationToken)
        {
            try
            {
               await _mqttServer.StartAsync();

                // Expire does not work it's a bug 
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("alarm/fromBroker")                    
                    .WithMessageExpiryInterval(1000)
                    .WithRetainFlag(true)
                    .WithPayload("[{\"ParameterName\":\"BUZZER\",\"ParameterValue\":\"TRUE\"}]")
                    .Build();


                int i = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (i % 60 == 0)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] I'm broker and I'm working properly");
                        i = 0;
                    }
                    i++;
                    List<SensorData> sensorDatas = new List<SensorData>();
                    sensorDatas.Add(new SensorData("BUZZER", "TRUE"));
                    await enqueueToAllSpecifiedTopics(sensorDatas);
                   // await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message));
                    Thread.Sleep(1000);
                }

                  //  try
                //   {
                //        Console.WriteLine("SENDING MESSAGE");
                //        await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message));
                //        Console.WriteLine("MESSAGE SENT TO CLIENTS");
                //        await Task.Delay(TimeSpan.FromSeconds(2));
                //        //Console.ReadLine();
                //    }
                //    catch (Exception ex)
                //    {
                //        // Handle the exception here
                //        Console.WriteLine($"Error while injecting MQTT message: {ex.Message}");
                //        throw;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when client connecting: {ex.Message} {ex.StackTrace}");
            }
  
        }

        public async Task publishMessage(string topic, string payload, string brokerId)
        {
            await _messagePublisherService.publishMessageAsync(_mqttServer, topic, payload, brokerId);
        }    
        


        public async Task DisconnectClientAsync(string clientId)
        {
            if (_mqttServer.IsStarted)
            {
               
                await _mqttServer.DisconnectClientAsync(clientId,MQTTnet.Protocol.MqttDisconnectReasonCode.AdministrativeAction);
            }
            else
            {
             
                Console.WriteLine("MQTT server is not started.");
            }
        }
    }
}

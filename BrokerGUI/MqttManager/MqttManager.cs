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
            var password2 = e;
            string password = e.Password?.ToString() ?? string.Empty;
            Console.WriteLine(password2.ToString());

            Console.WriteLine(password2.ToString());

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

            //update windows looks that new client has connected            
          
        }
    




        private async Task onInterceptingPublishAsync(InterceptingPublishEventArgs args)
        {
            var message = args.ApplicationMessage;
            var topic = message.Topic;
            var payload = Encoding.UTF8.GetString(message.Payload);

            Console.WriteLine($"Received publish request on topic '{topic}': Payload = '{payload}'");

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

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine("SENDING MESSAGE");
                        await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message));
                        Console.WriteLine("MESSAGE SENT TO CLIENTS");
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        //Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception here
                        Console.WriteLine($"Error while injecting MQTT message: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when client connecting: {ex.Message} {ex.StackTrace}");
            }

           // Console.WriteLine("Waitng for connections \n Press Enter to exit.");
          //  Console.ReadLine();
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

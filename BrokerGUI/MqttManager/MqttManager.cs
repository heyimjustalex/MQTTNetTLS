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
using Newtonsoft.Json;
using MQTTnet.Protocol;
using UI;

namespace Broker.MqttManager
{
    class MqttManager
    {
        MqttFactory _mqttFactory;
        MqttServer _mqttServer; 
        MqttBrokerConfiguration _mqttBrokerConfiguration;
        MqttServerOptions _serverOptionsConfiguration;
        List<Client> _currentlyConnectedValidatedClients;
        IMessagePublisherService _messagePublisherService;
        IClientAccountService _clientService;              

        public MqttManager(MqttBrokerConfiguration configuration, IClientAccountService clientService)
        {
            _currentlyConnectedValidatedClients = new List<Client>();    
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
            UI.GUIClientManager.RemoveClientByID(e.ClientId);
            foreach (Client client in _currentlyConnectedValidatedClients)
            {
                if(client.clientId== e.ClientId) {
                    _currentlyConnectedValidatedClients.RemoveAt(i);     
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
                    e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    e.ReasonString = "Invalid client identifier.";
                    
                    return ;
                }
            }
            
            Console.WriteLine($"onClientConnectionValidatio: ClientId: {clientId}, Username: {username} authenticated");
            // initialize that no smoke is detected
            // after milisecond message will come and update the state of this collection initial list that is inside client
            // no worries

            SensorData initialSmokeState = new SensorData("SMOKE", "FALSE");
            List<SensorData> initalList = new List<SensorData> { initialSmokeState };
            Client currentClient = new Client(clientId, username);
            currentClient.currentSensorDatas = initalList;
            _currentlyConnectedValidatedClients.Add(currentClient);
            UI.GUIClientManager.AddClient(new UI.ClientGUI(clientId, username, "FALSE","FALSE"));
            

        }
        private async Task onNewClientConnection(ClientConnectedEventArgs e)
        {
            Console.WriteLine("onNewClientConnection: New Client has been detected ");   
            
        }

        public void kill()
        {
            _mqttServer.Dispose();
        }
        private async Task enqueueToAllSpecifiedTopics(List<SensorData> sensorData)
        {
            var mqttMessage = new MessageMQTT(DateTime.Now, "broker", sensorData);
            string json = System.Text.Json.JsonSerializer.Serialize(mqttMessage);
            Console.WriteLine($"BROKER: enqueue to alarm/fromBroker: {mqttMessage.ToString()}");
                     
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

        private Client? GetClientByClientId(string clientId)
        {
            foreach (Client client in _currentlyConnectedValidatedClients)
            {
                if(client.clientId == clientId)
                {
                    return client;
                }
            }
            return null;
        }
        private SensorData getSmokeDetectorData(List<SensorData> sensorDatas) {

            SensorData ret = sensorDatas[0];
            foreach (SensorData sensorData in sensorDatas)
            {
                if(sensorData.ParameterName=="SMOKE")
                {
                    ret = sensorData;
                }
            }
            return ret;
        }

        private List<SensorData> updateBuzzerSensor(List<SensorData> sensorDatas, string buzzerParameterValue)
        {
     
            foreach (SensorData sensorData in sensorDatas)
            {
                if(sensorData.ParameterName=="BUZZER")
                {             
                    sensorData.ParameterValue = buzzerParameterValue;
                    return sensorDatas;
                }
            }

            //if there was no buzzer found in the collention of elements
            sensorDatas.Add(new SensorData("BUZZER", buzzerParameterValue));

            return sensorDatas;
        }

        private List<SensorData> updateSmokerSensor(List<SensorData> sensorDatas, string smokeParameterValue)
        {
            foreach (SensorData sensorData in sensorDatas)
            {
                if (sensorData.ParameterName == "SMOKE")
                {
                    sensorData.ParameterValue = smokeParameterValue;
                    return sensorDatas;
                }
            }

            //if there was no smoke found in the collention of elements
            sensorDatas.Add(new SensorData("SMOKE", smokeParameterValue));

            return sensorDatas;
        }

        private List<SensorData> rewriteListModifyingParameter(List<SensorData> sensorDatas, string ParamName, string ParamValue) {
            
            foreach (SensorData sensorData in sensorDatas)
            {
                if(sensorData.ParameterName==ParamName)
                {
                    sensorData.ParameterValue = ParamValue; 
                }
            }           
            return sensorDatas;        
        
        }

        private bool doAnyClientsHaveSmoke(List<Client> clients) {

            bool isSmokeOn = false;
            foreach(Client client in clients)
            {
                foreach (SensorData sensorData in client.currentSensorDatas)
                {
                    if(sensorData.ParameterName == "SMOKE" && sensorData.ParameterValue=="TRUE")
                    {
                        isSmokeOn = true;
                        break;
                    }

                }
            }
            return isSmokeOn;
        
        }



        private async Task onInterceptingPublishAsync(InterceptingPublishEventArgs e)
        {
            var applicationMessage = e.ApplicationMessage;

            if (applicationMessage == null) 
            { 
                return; 
            }

            var topic = applicationMessage.Topic;       

            var payloadText = string.Empty;
            if (e.ApplicationMessage.PayloadSegment.Count <= 0)
            {
                return;
            }

            payloadText = Encoding.UTF8.GetString(
                   e.ApplicationMessage.PayloadSegment.Array,
                   e.ApplicationMessage.PayloadSegment.Offset,
                   e.ApplicationMessage.PayloadSegment.Count);

            MessageMQTT message = JsonConvert.DeserializeObject<MessageMQTT>(payloadText);

            // here update the window new client authenticated
            //_currentlyConnectedValidatedClients.Add(new Client(clientId, username, password));
            //UI.UI.GUIClientManager.AddClient(new UI.ClientGUI(clientId, username, "TRUE"));
            if(message == null) {
                return;
            }

            if (message.From != "broker")
            {
                Console.WriteLine($"Received publish request on topic '{topic}': {message.ToString()}");

                string clientId = message.From;

                if(message.SensorDatas.Count>0)
                {
                    SensorData smokeSensorData = getSmokeDetectorData(message.SensorDatas);
                    //If client reports fire
                    if (smokeSensorData.ParameterValue == "TRUE")
                    {
                        // send updated state to all clients
                        List<SensorData> buzzerTrueInformMessage = new List<SensorData>
                            {
                                new SensorData("BUZZER", "TRUE")
                            };

                        await enqueueToAllSpecifiedTopics(buzzerTrueInformMessage);

                        foreach (Client client in _currentlyConnectedValidatedClients)
                        {
                            // update local clients cause you enabled buzzers by enqueueing message
                            client.currentSensorDatas = updateBuzzerSensor(client.currentSensorDatas,"TRUE");

                            if(client.clientId==clientId)
                            {
                                client.currentSensorDatas = updateSmokerSensor(client.currentSensorDatas, "TRUE");
                            }

                        }
                        // update gui with lambda
                     
                        UI.GUIClientManager.updateClients((client) => { 
                            
                            if(client.clientId == clientId) {
                                client.smokeDetectorState = "TRUE";
                            }
                            client.buzzerState = "TRUE"; 
                        
                        }) ;
                    }
                    // if clients reports no fire
                    else
                    {
                        Console.WriteLine("IN ELSE HANDLE MESSAGE");
                        Client clientWhoNoFire = GetClientByClientId(clientId);

                        if (clientWhoNoFire == null) {
                            throw new Exception("CLIENT WHO REPORTED NO FIRE IS NULL");
                        }


                        clientWhoNoFire.currentSensorDatas = rewriteListModifyingParameter(clientWhoNoFire.currentSensorDatas, "SMOKE", "FALSE");

                        foreach (Client client in _currentlyConnectedValidatedClients)
                        {
                            if (client.clientId == clientId)
                            {
                                client.currentSensorDatas = updateSmokerSensor(client.currentSensorDatas, "FALSE");
                            }
                        }

                        UI.GUIClientManager.updateClients((client) => { 

                            if (client.clientId == clientWhoNoFire.clientId) 
                            {
                                client.smokeDetectorState = "FALSE";                             
                            } 
                        });

                        if (!doAnyClientsHaveSmoke(_currentlyConnectedValidatedClients))
                        {
                            List<SensorData> buzzerFalseInformMessage = new List<SensorData>
                            {
                                new SensorData("BUZZER", "FALSE")
                            };
                            await enqueueToAllSpecifiedTopics(buzzerFalseInformMessage);
                            UI.GUIClientManager.updateClients((client) => { client.smokeDetectorState = "FALSE"; client.buzzerState = "FALSE"; });
                        }                                                                             
                    }
                }

                // jesli u klienta sie pali
                // wez liste podlaczonych klientow
                // sprawdz czy juz sensosry nie sa wlaczone (sytuacja ze jeden reportuje true a potem kolejny)
                // wyslij wszystkim buzzer true
                // update local listy i update gui
                
                //jezeli klient mowi false
                // ustaw mu false i sprawdz wszystkich klientów czy mają false. jesli tak to publish message buzzer na false
                
            }

            await Task.CompletedTask;
        }

       
        public async Task start(CancellationToken cancellationToken)
        {
            try
            {
               await _mqttServer.StartAsync();
                        

                int i = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (i % 60 == 0)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] I'm broker and I'm working properly");
                        i = 0;
                    }
                    i++;
         
              //      await enqueueToAllSpecifiedTopics(sensorDatas);
                   // await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message));
                    Thread.Sleep(5000);
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

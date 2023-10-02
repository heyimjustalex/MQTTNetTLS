using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Client.SensorService;
using Client.PKI;
using Client.Sensor;
using Client.MqttManager.Configuration;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Client.MqttManager
{
    class MqttManager
        {
        MqttClientConfiguration _mqttClientConfiguration;
        SensorBuzzerService _sensorBuzzerService;
        SensorSmokeDetectorService _sensorSmokeDetectorService;
  

        MqttFactory mqttFactory;
        IManagedMqttClient managedMqttClient;
        ManagedMqttClientOptions mqttManagedClientOptions;


        public MqttManager(MqttClientConfiguration mqttClientConfiguration, SensorBuzzerService sensorBuzzerService, SensorSmokeDetectorService smokeDetectorService)
        {
            _mqttClientConfiguration = mqttClientConfiguration;
            _sensorBuzzerService = sensorBuzzerService;
            _sensorSmokeDetectorService = smokeDetectorService;
         

            mqttFactory = new MqttFactory();
            managedMqttClient = mqttFactory.CreateManagedMqttClient();
            initConfiguration();
        }
        private void initConfiguration()
        {
            initClientConfigurationOptions();
            initClientFunctionHandlers();
        }
        private void initClientConfigurationOptions()
        {
          
            var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithClientId(_mqttClientConfiguration.Id)
                    .WithTcpServer(_mqttClientConfiguration.IpAddress, _mqttClientConfiguration.Port)
                    .WithCleanSession()
                    .WithCredentials(_mqttClientConfiguration.Username,_mqttClientConfiguration.Password)
                    //.WithCredentials(credentialsProvider)
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


        }
        private void initClientFunctionHandlers()
        {

            managedMqttClient.DisconnectedAsync += OnDisconnectAsync;
            managedMqttClient.ConnectedAsync += OnConnectAsync;
            managedMqttClient.ConnectingFailedAsync += OnConnectFailedAsync;
            managedMqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;
          

        }
        private async Task OnDisconnectAsync(MqttClientDisconnectedEventArgs args)
        {
            // we might add loger instead of loggin to console
            // Reconnect mechanism is internal so we do not need to reconnect when disconnected
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
        }

        private string determineBuzzerStateSentByBroker(List<SensorData> sensorDatas)
        {
            foreach (SensorData sensorData in sensorDatas) {

                if (sensorData.ParameterName == "BUZZER")
                {
                    return (sensorData.ParameterValue);
                }
            }
            return "";
        }
        private async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
         
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

            if (message != null) 
            {
                Console.WriteLine($"\nHandleReceivedMessage MESSAGE: {message.ToString()}");
                if (message.From == "broker")
                {
                    string buzzerStateSetByBroker = determineBuzzerStateSentByBroker(message.SensorDatas);
                    if(buzzerStateSetByBroker != "")
                    {
                        _sensorBuzzerService.set(bool.Parse(buzzerStateSetByBroker.ToUpper()));
                    }
                }
            }         
            Console.WriteLine();
        }

        private async Task OnConnectAsync(MqttClientConnectedEventArgs args)
        {
            // Successful connect
            Console.WriteLine("Task OnConnectAsync: Success connecting to broker. ...");
            SensorData smokeDetectorStateData = _sensorSmokeDetectorService.get();
            List<SensorData> sensorDatas = new List<SensorData>
            {
                smokeDetectorStateData
            };



            await enqueueToAllSpecifiedTopics(sensorDatas);

        }

        private async Task OnConnectFailedAsync(ConnectingFailedEventArgs args)
        {
            // Connect failed
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
        }

        private static bool OnCertificateValidation(MqttClientCertificateValidationEventArgs args)
        {

            X509Certificate2 serverCertificate = new X509Certificate2(args.Certificate);
            X509Certificate2 CACertificate = PKIUtilityStatic.ReadCertificateFromFile("../../../PKI/CA/RootCA.cer");

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

        private async Task enqueueToAllSpecifiedTopics(List<SensorData> sensorDatas)
        {           

            string json = System.Text.Json.JsonSerializer.Serialize(new MessageMQTT(DateTime.Now, _mqttClientConfiguration.Id, sensorDatas));
            foreach (var topic in _mqttClientConfiguration.TopicsClientEnqueuesTo)
            {
                await managedMqttClient.EnqueueAsync(topic, json, MqttQualityOfServiceLevel.ExactlyOnce);
         
            }
        }

        private async Task subscribeToAllSpecifiedTopics()
        {   
            foreach (var topic in _mqttClientConfiguration.TopicsClientSubscribesTo)
            {
                await managedMqttClient.SubscribeAsync(topic,MqttQualityOfServiceLevel.ExactlyOnce);

            }
        }


        public async Task start()
        {
           await managedMqttClient.StartAsync(mqttManagedClientOptions);
           await subscribeToAllSpecifiedTopics();

           SensorData smokeDetectorStateData = _sensorSmokeDetectorService.get();
         
            while (true)
            {
                bool ParameterValueSmokeDetectedOld = bool.Parse(smokeDetectorStateData.ParameterValue);
                smokeDetectorStateData = _sensorSmokeDetectorService.get();
                bool ParameterValueSmokeDetectedNew = bool.Parse(smokeDetectorStateData.ParameterValue);

                List<SensorData> sensorDatas = new List<SensorData>
            {
                smokeDetectorStateData
            };


                if (managedMqttClient.IsConnected)
                {                               
                    if(!(ParameterValueSmokeDetectedNew == ParameterValueSmokeDetectedOld))
                    {
                       await enqueueToAllSpecifiedTopics(sensorDatas);
                       Console.WriteLine($"Task start: Message published to remote broker (managedClient.IsConnected), state CHANGED, State:{ParameterValueSmokeDetectedNew }");
                    }
                    else
                    {
                        Console.WriteLine($"Task start: Message NOT published to remote broker (managedClient.IsConnected), state NOT CHANGED, State:{ParameterValueSmokeDetectedNew }");

                    }                   
                }
                else
                {
                        Console.WriteLine("Task start: Detector managed locally cuz managedClient.IsConnected = False.");
                    if(!ParameterValueSmokeDetectedOld && ParameterValueSmokeDetectedNew)
                    {
                        Console.WriteLine("Task start: (LOCAL DECISION): BUZZER SET TO TRUE I DETECTED SMOKE!");
                        _sensorBuzzerService.set(true);
                    }
                    else if(ParameterValueSmokeDetectedOld && !ParameterValueSmokeDetectedNew)
                    {
                        Console.WriteLine("Task start: (LOCAL DECISION): BUZZER SET TO FALSE I SEE NO MORE SMOKE!");
                        _sensorBuzzerService.set(false);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(8));

            }
        }       

    }

}

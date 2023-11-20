using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using MQTTnet;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Client.SensorService;
using Client.PKI;
using Client.SensorBase;
using Client.Sensors;


using Newtonsoft.Json;
using Client.Message;
using Client.Configuration;
using System.Device.Gpio;

namespace Client.MQTTCommunicationController
{
    class MQTTCommunicationController
    {
        MqttClientConfiguration _mqttClientConfiguration;
        SensorBuzzerService _sensorBuzzerService;
        SensorSmokeDetectorService _sensorSmokeDetectorService;  

        MqttFactory mqttFactory;
        IManagedMqttClient managedMqttClient;
        ManagedMqttClientOptions mqttManagedClientOptions;


        public MQTTCommunicationController(MqttClientConfiguration mqttClientConfiguration, SensorBuzzerService sensorBuzzerService, SensorSmokeDetectorService smokeDetectorService)
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
            // Reconnect mechanism is internal in ManagedMqttClient so we do not need to reconnect when disconnected
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
                Console.WriteLine($"BROKER: Message from broker -> {message.ToString()}");
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
            // DOCKER: IF ENV IS SET THEN USE ENV INSTEAD OF HARDCODED PATH
            string path = Environment.GetEnvironmentVariable("CA_PATH");
            if (path == null)
            {
                //means there it's not launched as container
                path = "./Client/PKI/CA/rootCA.cer";
            }
        
            X509Certificate2 serverCertificate = new X509Certificate2(args.Certificate);
            X509Certificate2 CACertificate = PKIUtilityStatic.ReadCertificateFromFile(path);

        
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
           string clientUsername = _mqttClientConfiguration.Username;

            SensorData smokeDetectorStateData = _sensorSmokeDetectorService.get();
            Buzzer buzzer = new Buzzer();
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
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
                       Console.WriteLine("CLIENT:"+$"{clientUsername}"+": REMOTE -> Message published (IsConnected), state CHANGED,  SMOKE:{" + ParameterValueSmokeDetectedNew.ToString().ToUpper() + "}");
                    }
                    else
                    {
                        Console.WriteLine("CLIENT:"+$"{clientUsername}"+": REMOTE -> Message NOT published (IsConnected), state NOT CHANGED,  SMOKE:{"+ParameterValueSmokeDetectedNew.ToString().ToUpper()+"}");
                    }                   
                }
                else
                {
                
                    if(!ParameterValueSmokeDetectedOld && ParameterValueSmokeDetectedNew)
                    {
                        Console.WriteLine("CLIENT:"+$"{clientUsername}"+": LOCAL (!IsConnected), SMOKE:{TRUE}");
                        //_sensorBuzzerService.set(true);
                        buzzer.set(true);
                        Console.WriteLine("BUZZER ENABLED BY LOCAL SYSTEM");
                    }
                    else if(ParameterValueSmokeDetectedOld && !ParameterValueSmokeDetectedNew)
                    {
                        Console.WriteLine("CLIENT:"+$"{clientUsername}"+": LOCAL(!IsConnected), SMOKE:{FALSE}");
                        //_sensorBuzzerService.set(false);
                        buzzer.set(false);
                        Console.WriteLine("CLIENT:" + $"{clientUsername}" + ": BUZZER DISABLED BY LOCAL SYSTEM");
                    }
                    else
                    {
                        Console.WriteLine("CLIENT:"+$"{clientUsername}"+": LOCAL (!managedClient.IsConnected), State as previous SMOKE:{"+ParameterValueSmokeDetectedNew.ToString().ToUpper()+"}");
                    }
                }
               
            }
        }       

    }

}

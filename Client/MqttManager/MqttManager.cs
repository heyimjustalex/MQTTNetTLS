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
using Client.DecisionManager;
using System.Runtime.CompilerServices;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Client.MqttManager
{

    public class SharedSensorResource
    {
        private List<SensorData> _sensorDataList = new List<SensorData>();
        private object lockObject = new object();
        private object stateChangeLock = new object();

        // Constructor without state change notification
 

        public List<SensorData> SensorDataList
        {
            get
            {
                lock (lockObject)
                {
                    return new List<SensorData>(_sensorDataList);
                }
            }
            set
            {
                lock (lockObject)
                {
                    // Check if the new list is different from the old one using SequenceEqual
                    if (!_sensorDataList.SequenceEqual(value))
                    {
                        _sensorDataList = value;

                        // Notify waiting threads of the state change
                        lock (stateChangeLock)
                        {
                            Monitor.PulseAll(stateChangeLock);
                        }
                    }
                }
            }
        }

        public void Work()
        {
            Console.WriteLine("Work method called because SensorData changed.");
            // Add your work logic here
        }

        // Monitoring thread that invokes Work when SensorData changes
        // Custom delegate for the monitoring function
         public delegate Task CustomMonitoringFunction(List<SensorData> currentSensorData, SharedSensorResource resource);

        // Monitoring thread that invokes a custom function when SensorData changes
        public void StartMonitoringThread(CustomMonitoringFunction customFunction)
        {
            Thread monitoringThread = new Thread(() =>
            {
                while (true)
                {
                    List<SensorData> previousSensorData;
                    lock (lockObject)
                    {
                        previousSensorData = new List<SensorData>(_sensorDataList);
                    }

                    lock (stateChangeLock)
                    {
                        Console.WriteLine("IN THREAD WAITING FOR STATE CHANGE");
                        Monitor.Wait(stateChangeLock); // Wait for a state change
                    }

                    //List<SensorData> currentSensorData;
                    //lock (lockObject)
                    //{
                    //    currentSensorData = new List<SensorData>(_sensorDataList);
                    //}

                    //// Check if SensorData has changed and invoke the custom function
                    //if (!previousSensorData.SequenceEqual(currentSensorData))
                    //{
                    Console.WriteLine("STATE CHANGED I PUBLISH MESSAGE TO BROKER");
                    List<SensorData> currentSensorData = new List<SensorData>(_sensorDataList); 
                    customFunction(currentSensorData, this); // Invoke the custom function
                    //}
                }
            });

            monitoringThread.Start();
        }
    }


        class MqttManager
    {
        MqttClientConfiguration _mqttClientConfiguration;
        SensorBuzzerService _sensorBuzzerService;
        SensorSmokeDetectorService _sensorSmokeDetectorService;
        ILocalDecisionMaker _localDecisionMaker;

        MqttFactory mqttFactory;
        IManagedMqttClient managedMqttClient;
        ManagedMqttClientOptions mqttManagedClientOptions;
            SharedSensorResource _sharedSensorResource;

        public MqttManager(MqttClientConfiguration mqttClientConfiguration, SensorBuzzerService sensorBuzzerService, SensorSmokeDetectorService smokeDetectorService)
        {
            _mqttClientConfiguration = mqttClientConfiguration;
            _sensorBuzzerService = sensorBuzzerService;
            _sensorSmokeDetectorService = smokeDetectorService;
            _localDecisionMaker = new LocalDecisionMaker();
            _sharedSensorResource = new SharedSensorResource();

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
                    // .WithCredentials(mqttClientConfiguration.Username, mqttClientConfiguration.Password)
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

        private async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine("\n### RECEIVED MESSAGE FROM BROKER ###\n");
            var payloadText = string.Empty;
            if (e.ApplicationMessage.PayloadSegment.Count <= 0)
            {
                return;
            }

             payloadText = Encoding.UTF8.GetString(
                    e.ApplicationMessage.PayloadSegment.Array,
                    e.ApplicationMessage.PayloadSegment.Offset,
                    e.ApplicationMessage.PayloadSegment.Count);
            

            List<SensorData> parametersFromBroker = JsonConvert.DeserializeObject<List<SensorData>>(payloadText);
           
            


            //Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            //Console.WriteLine($"+ Payload = {payloadText}");
            //Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            //Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
        }

        private async Task OnConnectAsync(MqttClientConnectedEventArgs args)
        {
            // Successful connect
            Console.WriteLine("Task OnConnectAsync: Success connecting to broker. ...");

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

      ///  private List<SensorData> sortOutJsonedSensorData() { }

        private List<SensorData> getAllSensorData() {

            List<SensorData> allSensorData = new List<SensorData>
            {
                // sequence matters
                _sensorBuzzerService.get(),
                _sensorSmokeDetectorService.get()
            };

            return allSensorData;
        }

        private async Task enqueueToAllSpecifiedTopics(List<SensorData> allSensorData)
        {
            
            string json = System.Text.Json.JsonSerializer.Serialize(new { message = allSensorData, sent = DateTime.UtcNow });
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
           List<SensorData> newSensorData = getAllSensorData();
           _sharedSensorResource.SensorDataList = newSensorData;
           _sharedSensorResource.StartMonitoringThread(async (currentSensorData, resource) =>
             {
                 // Access class properties or methods as needed
                 // For example, you can call enqueueToAllSpecifiedTopics with the currentSensorData
                 await enqueueToAllSpecifiedTopics(currentSensorData);
                 if (managedMqttClient.IsConnected)
                 {
                     Console.WriteLine("Task start: Message published to remote broker cuz managedClient.IsConnected = True.");
                 }
                 else
                 {
                     Console.WriteLine("Task start: Message queued locally cuz managedClient.IsConnected = False. I will send them when I connect to server");
                 }
             });

            while (true)
            {
                newSensorData = getAllSensorData();
                _sharedSensorResource.SensorDataList= newSensorData;
                foreach (SensorData sensorData in _sharedSensorResource.SensorDataList)
                {  
                    Console.WriteLine(sensorData.ParameterName + " " + sensorData.ParameterValue);                  
                    string ParameterName = sensorData.ParameterName;
                    string ParameterValue = sensorData.ParameterValue;
                }
                    //await enqueueToAllSpecifiedTopics(allSensorData);
                    await Task.Delay(TimeSpan.FromSeconds(2));
               

            }
        }       

    }

}

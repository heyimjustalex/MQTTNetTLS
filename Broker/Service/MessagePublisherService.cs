using MQTTnet;
using MQTTnet.Server;
using System;

using System.Threading.Tasks;

namespace Broker.Service
{
    internal class MessagePublisherService : IMessagePublisherService
    {
        public async Task publishMessageAsync(MqttServer mqttServer, string topic, string payload, string brokerId)
        {
            var message = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(payload).Build();
            //var message = new MqttApplicationMessageBuilder().WithTopic("HelloWorld").WithPayload("Test").Build();

            try
            {
                Console.WriteLine("SENDING MESSAGE");
                await mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message) { SenderClientId = "TestClient" });
                Console.WriteLine("Message sent");
            }
            catch (Exception ex)
            {
                // Handle the exception here
                Console.WriteLine($"Error while injecting MQTT message: {ex.Message}");
                throw;
            }
        }
    }
}


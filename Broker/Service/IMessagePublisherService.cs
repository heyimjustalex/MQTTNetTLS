using MQTTnet.Server;
using System.Threading.Tasks;

namespace Broker.Service
{
    internal interface IMessagePublisherService
    {
        public Task publishMessageAsync(MqttServer mqttServer, string topic, string payload, string brokerId);

    }
}

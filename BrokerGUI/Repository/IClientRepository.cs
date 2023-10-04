using BrokerGUI.Message;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Repository
{
    internal interface IClientRepository
    {
        public void registerClient(string username, string password, string clientId);
        public void removeClient(string username);
        public bool authenticateClient(string username, string password);
        public void setClientSensorData(string clientId, List<SensorData> sensorDatas);
        public List<SensorData> getClientSensorData(string clientId);
    }
      
}

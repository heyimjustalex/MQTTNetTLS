using Broker.Entity;
using BrokerGUI.Message;
using System.Collections.Generic;

namespace Broker.Database
{
    interface IClientDB
    {
        public void addClient(string clientId, string username, string password);
        public Client? getClientByUsername(string username);
        public void removeClient(string username);
        public bool authenticate(string username, string password);
        public List<SensorData> getSensorDataOfClient(string clientId);
        public void setSensorDataOfClient(string clientId, List<SensorData> sensorDatasNew);

    }
}


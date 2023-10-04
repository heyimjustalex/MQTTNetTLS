using BrokerGUI.Message;
using System.Collections.Generic;

namespace Broker.Entity
{
    public class Client
    {
        public string clientId;
        public string username;
        public string password;
        public List<SensorData> _sensorDatas;
        public Client(string clientId, string username, string password="")
        {
            this.clientId = clientId;
            this.username = username;
            this.password = password;
            _sensorDatas = new List<SensorData>();   
        }

    }
}

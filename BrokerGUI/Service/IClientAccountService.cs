using Server.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Service
{
    interface IClientAccountService
    {
        public void register(string username, string password, string clientId);
        public void remove(string username);
        public bool authenticate(string username, string password);
        public void setClientSensorData(string clientId, List<SensorData> sensorDatas);
        public List<SensorData> getClientSensorData(string clientId);

    }
}

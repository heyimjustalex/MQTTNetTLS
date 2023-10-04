using Broker.Database;
using Broker.Entity;
using Broker.Repository;
using BrokerGUI.Message;
using System;
using System.Collections.Generic;

namespace BrokerGUI.Repository
{
    class ClientRepository : IClientRepository
    {
        private IClientDB _db;
        public ClientRepository(IClientDB db)
        {

            _db = db;
        }
        public bool authenticateClient(string username, string password)
        {
            Client? client = _db.getClientByUsername(username);
            if (client != null)
            {
                if (client.password == password)
                {
                    return true;
                }
            }
            else
            {
                Console.WriteLine("Client not found in repository");
            }
            return false;
        }

        public void registerClient(string username, string password, string clientId)
        {
            _db.addClient(username, password, clientId);
        }

        public void setClientSensorData(string clientId, List<SensorData> sensorDatas)
        {
            _db.setSensorDataOfClient(clientId, sensorDatas);
        }

        public List<SensorData> getClientSensorData(string clientId)
        {
            return _db.getSensorDataOfClient(clientId);
        }

        public void removeClient(string username)
        {
            throw new NotImplementedException();
        }


    }
}

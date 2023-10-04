using Broker.Repository;
using BrokerGUI.Message;
using System.Collections.Generic;

namespace Broker.Service
{
    internal class ClientAccountService
    {
        IClientRepository _repository;
        public ClientAccountService(IClientRepository repository) {
            _repository = repository;   
        }
        public bool authenticate(string username, string password)
        {
           return _repository.authenticateClient(username, password);
        }    
        public void register(string username, string password, string clientId)
        {
            _repository.registerClient(username, password, clientId);   
        }
        public void remove(string username)
        {
            _repository.removeClient(username);
        }

        public void setClientSensorData(string clientId, List<SensorData> sensorDatas)
        {
           _repository.setClientSensorData(clientId, sensorDatas);
        }
        public List<SensorData> getClientSensorData(string clientId)
        {
            return _repository.getClientSensorData(clientId);
        }
    }
}

using Broker.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Service
{
    internal class ClientService : IClientService
    {
        IClientRepository _repository;
        public ClientService(IClientRepository repository) {
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
    }
}

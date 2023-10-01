using Broker.Entity;

namespace Broker.Database
{
    interface IClientDB
    {
        public void addClient(string clientId, string username, string password);
        public Client? getClientByUsername(string username);
        public void removeClient(string username);
        public bool authenticate(string username, string password);

    }
}


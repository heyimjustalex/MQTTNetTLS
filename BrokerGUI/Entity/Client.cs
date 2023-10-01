using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Entity
{
    public class Client
    {
        public string clientId;
        public string username;
        public string password;

        public Client(string clientId, string username, string password)
        {
            this.clientId = clientId;
            this.username = username;
            this.password = password;
        }

    }
}

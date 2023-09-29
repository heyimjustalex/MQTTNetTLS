using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client.MqttManager
{
    internal class MqttClientConfiguration
    {
        private int _port;
        private string _ipAddress;
        private string _id;
        private string _username;
        private string _password;

        public MqttClientConfiguration(int port, string ipAddress, string id, string username, string password)
        {
            _port = port;
            _ipAddress = ipAddress;
            _id = id;
            _username = username;
            _password = password;
        }

        public int Port
        {
            get => _port;
            set
            {
                _port = value;
            }
        }

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
            }
        }

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }
    }

}


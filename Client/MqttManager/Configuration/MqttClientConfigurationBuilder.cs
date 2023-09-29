using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MqttManager.Configuration
{
    internal class MqttClientConfigurationBuilder
    {
        private int _port;
        private string _ipAddress;
        private string _id;
        private string _username;
        private string _password;
        private string[] _topicsClientEnqueuesTo;
        private string[] _topicsClientSubscribesTo;

        public MqttClientConfigurationBuilder WithPort(int port)
        {
            _port = port;
            return this;
        }

        public MqttClientConfigurationBuilder WithIpAddress(string ipAddress)
        {
            _ipAddress = ipAddress;
            return this;
        }

        public MqttClientConfigurationBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public MqttClientConfigurationBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public MqttClientConfigurationBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public MqttClientConfigurationBuilder WithTopicsClientEnqueuesTo(string[] topics)
        {
            _topicsClientEnqueuesTo = topics;
            return this;
        }

        public MqttClientConfigurationBuilder WithTopicsClientSubscribesTo(string[] topics)
        {
            _topicsClientSubscribesTo = topics;
            return this;
        }

        public MqttClientConfiguration Build()
        {
            return new MqttClientConfiguration(_port, _ipAddress, _id, _username, _password, _topicsClientEnqueuesTo, _topicsClientSubscribesTo);
        }
    }

}

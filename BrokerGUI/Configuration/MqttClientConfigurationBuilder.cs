using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Configuration
{
    internal class MqttBrokerConfigurationBuilder
    {
        private int _port;
        private string _ipAddress;
        private string _id;
        private string _username;
        private string _password;
        private string[] _topicsBrokerEnqueuesTo;
        private string[] _topicsBrokerSubscribesTo;
        X509Certificate2 _certificate;

        public MqttBrokerConfigurationBuilder WithPort(int port)
        {
            _port = port;
            return this;
        }

        public MqttBrokerConfigurationBuilder WithIpAddress(string ipAddress)
        {
            _ipAddress = ipAddress;
            return this;
        }

        public MqttBrokerConfigurationBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public MqttBrokerConfigurationBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public MqttBrokerConfigurationBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public MqttBrokerConfigurationBuilder WithTopicsBrokerEnqueuesTo(string[] topics)
        {
            _topicsBrokerEnqueuesTo = topics;
            return this;
        }

        public MqttBrokerConfigurationBuilder WithTopicsBrokerSubscribesTo(string[] topics)
        {
            _topicsBrokerSubscribesTo = topics;
            return this;
        }

        public MqttBrokerConfigurationBuilder WithCertificate(X509Certificate2 certificate)
        {
            _certificate = certificate;
            return this;
        }

        public MqttBrokerConfiguration Build()
        {
            return new MqttBrokerConfiguration(_port, _ipAddress, _id, _username, _password, _topicsBrokerEnqueuesTo, _topicsBrokerSubscribesTo,_certificate);
        }
    }

}

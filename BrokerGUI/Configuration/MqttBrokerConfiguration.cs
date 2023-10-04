using System.Security.Cryptography.X509Certificates;

namespace Broker.Configuration
{
    internal class MqttBrokerConfiguration
    {
        private int _port;
        private string _ipAddress;
        private string _id;
        private string _username;
        private string _password;
        private string[] _topicsBrokerEnqueuesTo;
        private string[] _topicsBrokerSubscribesTo;
        X509Certificate2 _certificate;

        public MqttBrokerConfiguration(int port, string ipAddress, string id, string username, string password, string[] topicsBrokerEnqueuesTo, string[] topicsBrokerSubscribesTo,X509Certificate2 certificate)
        {
            _port = port;
            _ipAddress = ipAddress;
            _id = id;
            _username = username;
            _password = password;
            _topicsBrokerEnqueuesTo = topicsBrokerEnqueuesTo;
            _topicsBrokerSubscribesTo = topicsBrokerSubscribesTo;
            _certificate = certificate;
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

        public string[] TopicsBrokerEnqueuesTo
        {
            get => _topicsBrokerEnqueuesTo;
            set => _topicsBrokerEnqueuesTo = value;
        }

        public string[] TopicsBrokerSubscribesTo
        {
            get => _topicsBrokerSubscribesTo;
            set => _topicsBrokerSubscribesTo = value;
        }

        public X509Certificate2 Certificate
        {
            get => _certificate;
            set
            {
                _certificate = value;
            }
        }
    }


}


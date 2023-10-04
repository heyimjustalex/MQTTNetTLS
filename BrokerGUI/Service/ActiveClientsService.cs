using Broker.Database;
using Broker.Entity;
using Server.Sensor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;

namespace BrokerGUI.Service
{

    public class ActiveClientsService
    {

        List<Client> _currentlyConnectedValidatedClients;

        public ActiveClientsService()
        {

            _currentlyConnectedValidatedClients = new List<Client>();
        }
        public void add(Client client)
        {
            _currentlyConnectedValidatedClients.Add(client);
        }

        public void remove(Client client)
        {
            _currentlyConnectedValidatedClients.Remove(client);
        }

        public void removeClientById(string clientId)
        {
            foreach (Client client in _currentlyConnectedValidatedClients)
            {
                if (client.clientId == clientId)
                {
                    _currentlyConnectedValidatedClients.Remove(client);
                }
            }
        }
        public void rewriteClientSensorListModifyingParameter(string clientId, string ParamName, string ParamValue)
        {
            foreach (Client client in _currentlyConnectedValidatedClients)
            {
                if (client.clientId == clientId)
                {
                    foreach (SensorData sensorData in client.currentSensorDatas)
                    {
                        if (sensorData.ParameterName == ParamName)
                        {
                            sensorData.ParameterValue = ParamValue;
                        }
                    }

                }

            }
        }
        public void updateClients(Action<Client> update)
        {
            foreach (Client client in _currentlyConnectedValidatedClients)
            {
                update(client);
            }
        }

        public bool doAnyClientsHaveSmoke()
        {

            bool isSmokeOn = false;
            foreach (Client client in _currentlyConnectedValidatedClients)
            {
                foreach (SensorData sensorData in client.currentSensorDatas)
                {
                    if (sensorData.ParameterName == "SMOKE" && sensorData.ParameterValue == "TRUE")
                    {
                        isSmokeOn = true;
                        break;
                    }

                }
            }
            return isSmokeOn;

        }
    }
  

}

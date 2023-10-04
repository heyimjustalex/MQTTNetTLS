using System;
using System.Collections.Generic;
using System.Linq;
using UI;

namespace UI
{
    public static class GUIClientManager
    {
        private static List<ClientGUI> clients;

        static GUIClientManager()
        {
            clients = new List<ClientGUI>();
        }

        public static List<ClientGUI> GetClients()
        {
            return clients;
        }

        public static void updateClients(List<ClientGUI> clients)
        {
            GUIClientManager.clients = clients;
        }

        // invoked by mqttmanager to update state gui clients
        public static void updateClients(Action<ClientGUI> update)
        {
            foreach (ClientGUI client in clients)
            {
                update(client);
            }
        }

        public static void AddClient(ClientGUI client)
        {
            clients.Add(client);
        }

        public static void RemoveClient(string clientName)
        {
            ClientGUI clientToRemove = clients.FirstOrDefault(client => client.username == clientName);
            if (clientToRemove != null)
            {
                clients.Remove(clientToRemove);
            }
        }
        public static void RemoveClientByID(string clientId)
        {

            ClientGUI clientToRemove = clients.FirstOrDefault(client => client.clientId == clientId);
            if (clientToRemove != null)
            {
                Console.WriteLine("REMOVING BY ID SUCCESS");
                clients.Remove(clientToRemove);
            }
        }
    }
}

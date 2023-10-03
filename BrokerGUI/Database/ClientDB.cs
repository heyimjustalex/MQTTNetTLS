using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Broker.Entity;
using System.IO;
using Server.Sensor;

namespace Broker.Database
{
    internal class ClientDB : IClientDB
    {
        List<Client> _clients;      
        private string _dataFilePath;
        public ClientDB()
        {
            //deleteDB();
            _dataFilePath = "db.json";
            _clients = loadUserData();            
        }

        private void deleteDB()
        {
            if (File.Exists(_dataFilePath))
            {
                try
                {
                    File.Delete(_dataFilePath);
                    Console.WriteLine("Database file deleted successfully.");
                }
                catch (IOException e)
                {
                    Console.WriteLine($"Error deleting the file: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine("Database file does not exist.");
            }
        }

        public List<SensorData> getSensorDataOfClient(string clientId) 
        {
            foreach(Client client in _clients)
            {
                if(client.clientId== clientId)
                {
                    return client.currentSensorDatas;
                }
            }
            return new List<SensorData>();
        }

        public void setSensorDataOfClient(string clientId, List<SensorData> sensorDatasNew)
        {
            foreach (Client client in _clients)
            {
                if (client.clientId == clientId)
                {
                    client.currentSensorDatas = sensorDatasNew;
                }
            }
        }
        public void addClient(string clientId, string username, string password)
        {
            if (getClientByUsername(username) == null ) 
            { 
                _clients.Add(new Client(clientId, username, password));
                saveClientData();
            }
            else
            {
                throw new Exception($"Client {username} is already in the database");
            }                 
        }
        public Client? getClientByUsername(string username)
        {
            foreach (Client client in _clients)
            {
                if(client.username == username)
                {
                    return client;
                }
            }
            return null;
        }
        public void removeClient(string username)
        {
            Client? clientToRemove = getClientByUsername(username);

            if (clientToRemove != null)
            {      
                _clients.Remove(clientToRemove);
                saveClientData();
            }       
        }
        public bool authenticate(string username, string password)
        {

            Client? client = getClientByUsername(username);

            if (client != null && client.password == password)
            {    
                return true; 
            }
            else            
            {

                return false; 
            }
        }
        private List<Client> loadUserData()
        {
            if (File.Exists(_dataFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_dataFilePath);
                    var userData = JsonConvert.DeserializeObject<List<Client>>(json);
                    return userData ?? new List<Client>(); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading user data: {ex.Message}");
                }
            }
                return new List<Client>();
        }

        private void saveClientData()
        {
           Console.WriteLine("SAVING CLIENT DATA");
           string currentDirectory = Directory.GetCurrentDirectory();
           Console.WriteLine("Current Directory: " + currentDirectory);

            try
            {
                string json = JsonConvert.SerializeObject(_clients);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving user data: {ex.Message}");
            }
        }

    }
}

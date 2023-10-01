using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EntityClientGUI;

namespace BrokerGUI
{
    public partial class MainWindow : Window
    {
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private List<ClientGUI> clients = new List<ClientGUI>
        {
            new ClientGUI("User1","User12", "FALSE"),
            new ClientGUI("User2","User23", "FALSE"),
            new ClientGUI("User3","User3", "TRUE")
        };

        private bool isBrokerRunning = false; 
        private Dictionary<string, Color> alarmColors = new Dictionary<string, Color>
        {
            { "TRUE", Colors.Red },
            { "FALSE", Colors.Green },

        };

        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext of the ListBox to the list of clients
            clientItemsControl.ItemsSource = clients;

            // Start the alarm checking thread
            Thread alarmCheckingThread = new Thread(CheckAlarms);
            alarmCheckingThread.IsBackground = true;
            alarmCheckingThread.Start();
        }

        private void AddClient()
        {
            // Simulate adding a client
            Application.Current.Dispatcher.Invoke(() =>
            {
                clients.Add(new ClientGUI($"User{clients.Count + 1}", $"User{clients.Count + 1}", "FALSE"));
                clientItemsControl.Items.Refresh(); // Refresh the ListBox
            });
        }

        private void RemoveClient(string clientName)
        {
            // Find and remove the client with the specified name
            ClientGUI clientToRemove = clients.FirstOrDefault(client => client.username == clientName);
            if (clientToRemove != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    clients.Remove(clientToRemove);
                    clientItemsControl.Items.Refresh(); // Refresh the ListBox
                });
            }
        }
        private void SimulateClientOperations()
        {
            while (isBrokerRunning) // Check the flag to control the loop
            {
                AddClient();
                Thread.Sleep(1000); // Sleep for 1 second between adding and removing
                RemoveClient("User3");
            }
        }

        private void StartBrokerButton_Click(object sender, RoutedEventArgs e)
        {
          
            if (!isBrokerRunning)
            {
                Task.Run(() => (BrokerGUI.Program.Broker(_cancellationTokenSource.Token)));
                // Start the Broker if it's not running
                isBrokerRunning = true;
                StartBrokerButton.Content = "Stop Broker"; // Change button text
             
              
                //Thread brokerThread = new Thread(SimulateClientOperations);
                //  brokerThread.IsBackground = true;
                //  brokerThread.Start();
            }
            else
            {
                // Stop the Broker if it's running
                isBrokerRunning = false;
                StartBrokerButton.Content = "Start Broker"; // Change button text
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();


            }
        }

        private void CheckAlarms()
        {
            while (true)
            {
                string alarmState = clients.Any(client => client.alarmState.Contains("TRUE")) ? "TRUE" : "FALSE";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AlarmButton.Background = new SolidColorBrush(alarmColors[alarmState]);
                });

                Thread.Sleep(1000); // Check every second
            }
        }
    }
}

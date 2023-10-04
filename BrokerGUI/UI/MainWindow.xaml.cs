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
using UI;

namespace UI {
 
        public partial class MainWindow : Window
    {
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool isBrokerRunning = false;
        private Dictionary<string, Color> alarmColors = new Dictionary<string, Color>
    {
        { "TRUE", Colors.Red },
        { "FALSE", Colors.Green },
            {"UNKNOWN", Colors.Gray }
    };

        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext of the ListBox to the list of clients
            clientItemsControl.ItemsSource =GUIClientManager.GetClients();

            // Start the alarm checking thread
            Thread alarmCheckingThread = new Thread(CheckAlarms);
            alarmCheckingThread.IsBackground = true;
            alarmCheckingThread.Start();
        }
 
        private void refreshItems()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {               
                clientItemsControl.Items.Refresh(); 
            });
        }

            private void RemoveClient(string clientName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
               GUIClientManager.RemoveClient(clientName);
                clientItemsControl.Items.Refresh(); // Refresh the ListBox
            });
        }

        private void StartBrokerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isBrokerRunning)
            {
                Task.Run(() => (BrokerGUI.Program.Broker(_cancellationTokenSource.Token)));
                // Start the Broker if it's not running
                isBrokerRunning = true;
                StartBrokerButton.Content = "Stop Broker"; // Change button text
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
                string smokeDetectorState =GUIClientManager.GetClients().Any(client => client.buzzerState.Contains("TRUE")) ? "TRUE" : "FALSE";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AlarmButton.Background = new SolidColorBrush(alarmColors[smokeDetectorState]);
                    AlarmTextBlock.Text = smokeDetectorState == "TRUE" ? "ON" : "OFF";
                });
                refreshItems();
                Thread.Sleep(1000); // Check every second
            }
        }
    }

}

using Broker;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BrokerGUI
{ 
    public partial class App : Application
    {
    
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.11.0")]
        public static void Main()
        {
            Task.Run(() => (BrokerGUI.Program.Broker()));
            BrokerGUI.App app = new BrokerGUI.App();
            app.InitializeComponent();
            app.Run();
        }

       
    }

}

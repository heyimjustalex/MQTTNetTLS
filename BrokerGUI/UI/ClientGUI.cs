using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrokerGUI;

namespace UI
{
    public class ClientGUI
    {
        public string clientId { get; set; }
        public string username { get; set; }
        public string smokeDetectorState { get; set; }
        public string buzzerState { get; set; }

        public ClientGUI(string clientId, string username, string smokeDetectorState, string buzzerState)
        {
            this.clientId = clientId;
            this.username = username;           
            this.smokeDetectorState = smokeDetectorState;
            this.buzzerState = buzzerState;
        }

    }
}

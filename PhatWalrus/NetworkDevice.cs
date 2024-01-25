using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CredentialManager;

namespace NetworkDeviceManager
{
    class NetworkDevice
    {
        public string name { get; private set; }
        public string v4address { get; private set; }
        public int[] uiLocation {  get; private set; }
        public List<NetworkDevice> connections { get; private set; }
        public TerminalManager terminal { get; set; }

        private Credentials credentials { get; set; }
        private string assetsDir { get; set; }

        public NetworkDevice(string name, string v4address, int[] uiLocation, List<NetworkDevice> connections, Credentials credentials, string assetsDir) 
        {
            this.name = name;
            this.v4address = v4address;
            this.uiLocation = uiLocation;
            this.connections = connections;
            this.credentials = credentials;
            this.assetsDir = assetsDir;

            this.terminal = new TerminalManager(this.assetsDir, this.v4address, ManagementProtocol.SSH, this.credentials);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CredentialManager;
using SSHBackend;

namespace NetworkDeviceManager
{
    enum ManagementProtocol 
    {
        SSH,
        SSHNoExe,
        SNMP
    }

    class TerminalManager
    {
        private string assetsDir { get; set; }
        private Dictionary<string, object> catalystCommands { get; set; }
        private string v4address { get; set; }
        private ManagementProtocol protocol { get; set; }
        private Credentials credentials { get; set; }
        private SSHManager? sshManager { get; set; }

        public TerminalManager(string assetsDir, string v4address, ManagementProtocol protocol, Credentials credentials) 
        {
            this.assetsDir = assetsDir;
            this.catalystCommands = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(this.assetsDir + @"\CiscoCommandTree.json")) ?? new Dictionary<string, object>();
            this.v4address = v4address;
            this.credentials = credentials;

            if (protocol == ManagementProtocol.SSH)
            {
                sshManager = new SSHManager(this.v4address, this.credentials);
                sshManager.Connect();
                var task = new Task(() => { sshManager.ExecuteExecChannel("show version"); });
                if (task.Wait(TimeSpan.FromSeconds(5)))
                {
                    this.protocol = ManagementProtocol.SSH;
                } else
                {
                    this.protocol = ManagementProtocol.SSHNoExe;
                }
                sshManager.Diconnect();
            }
        }

        public string[] CiscoCommandCompletion(string[] currentCommand)
        {
            IEnumerable<string> matchingValues;

            if (currentCommand.Length > 1)
            {
                Dictionary<string, object> currentCommandDictionary = catalystCommands;
                try
                {
                    for (int i = 0; i <= currentCommand.Length - 2; i++)
                    {
                        currentCommandDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(currentCommandDictionary[currentCommand[i]].ToString()) ?? new Dictionary<string, object>();
                    }
                    matchingValues = currentCommandDictionary.Keys
                                    .Where(x => x.StartsWith(currentCommand[currentCommand.Length - 1]));
                }
                catch (KeyNotFoundException ex)
                {
                    throw new KeyNotFoundException(ex.ToString());
                }
                catch (Exception ex)
                {
                    throw new Exception("It errored: " + ex.ToString());
                }
            }
            else
            {
                matchingValues = catalystCommands.Keys
                                    .Where(x => x.StartsWith(currentCommand[0]));
            }

            return matchingValues.ToArray();
        }

        public void SendCommand(string command)
        {
            string response;
            if (protocol == ManagementProtocol.SSH)
            {
                response = sshManager.ExecuteExecChannel(command);
            }
            else if (protocol == ManagementProtocol.SSHNoExe) 
            {
                sshManager.Connect();
                sshManager.CreateShellStream();
                response = sshManager.ExecuteShellStream(command);
                sshManager.Diconnect();
            } else
            {
                response = "Failed.";
            }
            Console.WriteLine(response);
        }
    }
}

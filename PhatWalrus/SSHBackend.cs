using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SSHBackend
{
    class SSHConnection
    {
        // Can send a command and recieve a response to the command
        // returns a string with the response to the command -- either the error or the result
        public string SSHInterface(string hostaddress, string username, string password, string command)
        {
            using (var client = new SshClient(hostaddress, username, password))
            {
                client.Connect();
                client.RunCommand(command);
                var _command = client.CreateCommand(command);
                _command.Execute();
                string result = _command.Result;
                string error = _command.Error;
                client.Disconnect();
                if (error != "")
                {
                    throw new Exception("SSH Error: " + error);
                }
                else
                {
                    return result;
                }
            }
        }
    }

    class SSHManager
    {
        private string assets { get; set; }

        public SSHManager(string assets)
        {
            this.assets = assets;
        }
        public void AddClient()
        {
            Dictionary<string, string> data = new();
            Console.WriteLine("Address");
            data["address"] = Console.ReadLine();
            Console.WriteLine("Username");
            data["username"] = Console.ReadLine();
            Console.WriteLine("Password");
            data["password"] = Console.ReadLine();
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(@".\"+assets+"SSHClients.sidars", json);
        }
        // takes in the command when the tab key is pressed and returns the most likely completed command
        public string[] CiscoCommandCompletion(string[] currentCommand)
        {

            return "";
        }
    }
}
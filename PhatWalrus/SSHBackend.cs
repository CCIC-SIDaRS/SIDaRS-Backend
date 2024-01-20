using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SSHBackend
{
    class SSHConnection
    {
        private SshClient client { get; set; }

        // Can send a command and recieve a response to the command
        // returns a string with the response to the command -- either the error or the result
        public SSHConnection(string hostaddress, string username, string password)
        {
            client = new SshClient(hostaddress, username, password);
        }

        public void Connect()
        {
            try
            {
                client.Connect();
            }catch (Exception ex) 
            {
                throw new Exception(ex.ToString());
            }
            
        }
        public string Execute (string command)
        {
            SshCommand _command = client.CreateCommand(command);
            Console.WriteLine("exeucting: " + _command.CommandText);
            _command.Execute();
            Console.WriteLine("Execution Complete");
            string result = _command.Result;
            if (_command.Error != "")
            {
                //throw new Exception("SSH Command Error " + _command.Error);
            }
            return result;
        }
        public void Diconnect()
        {
            try
            {
                client.Disconnect();
            }catch (Exception ex)
            {
                throw new Exception(ex.ToString());
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
            Dictionary<string, Dictionary<string, string>> currentClients = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                currentClients = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(assets + "SSHClients.sidars")) ?? new Dictionary<string, Dictionary<string, string>>();
            }catch (Exception) { }
            
            Console.WriteLine("Device ID");
            string ID = Console.ReadLine();
            Console.WriteLine("Address");
            string address = Console.ReadLine();
            Console.WriteLine("Username");
            string username = Console.ReadLine();
            Console.WriteLine("Password");
            string password = Console.ReadLine();
            currentClients[ID] = new Dictionary<string, string> { ["address"] = address, ["username"] = username, ["password"] = password };
            string json = JsonSerializer.Serialize(currentClients);
            File.WriteAllText(assets + "SSHClients.sidars", string.Empty);
            File.WriteAllText(assets+"SSHClients.sidars", json);
        }
        // takes in the command when the tab key is pressed and returns the most likely completed command
        public string[] CiscoCommandCompletion(string[] currentCommand)
        {

            return [""];
        }
    }
}
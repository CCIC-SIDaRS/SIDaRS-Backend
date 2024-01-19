using Renci.SshNet;
using System.Text.Json;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Transactions;

namespace SSHBackend
{
    class Testing
    {
        private static void Main(string[] args)
        {
            SSHConnection something = new SSHConnection();
            if (Console.ReadLine() == "y")
            {
                something.AddClient();
            }
            string json = File.ReadAllText(@".\SSHClients.sidars");
            Dictionary <string, string> items = JsonSerializer.Deserialize<Dictionary<string,string>>(json);
            Console.WriteLine(items["password"]);
        }
    }
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
                }else
                {
                    return result;
                }
            }
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
            File.WriteAllText(@".\SSHClients.sidars", json);
        }

        // takes in the command when the tab key is pressed and returns the most likely completed command
        public string CiscoCommandCompletion(string currentCommand)
        {

            return "";
        }
    }
}
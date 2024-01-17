using Renci.SshNet;
using SshNet;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace SSHBackend
{
    class Testing
    {
        private static void Main(string[] args)
        {
            SSHConnection sSHConnection = new SSHConnection();
            Console.WriteLine("address");
            string address = Console.ReadLine();
            Console.WriteLine("username");
            string username = Console.ReadLine();
            Console.WriteLine("password");
            string password = Console.ReadLine();
            Console.WriteLine("command");
            string command = Console.ReadLine();
            Console.WriteLine(sSHConnection.SSHInterface(address, username, password, command));
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
    }
}
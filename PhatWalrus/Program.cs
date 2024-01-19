using Renci.SshNet;
using System.Text.Json;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Transactions;
using SSHBackend;

namespace Testing
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
}
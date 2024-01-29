using Renci.SshNet;
using System.Text.Json;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Transactions;
using NetworkDeviceManager;
using CredentialManager;

namespace Testing
{
    class Testing
    {
        private static void ConnectionReadCallback(string output)
        {
            Console.WriteLine(output);
        }
        private static void Main(string[] args)
        {
            Credentials credentials = new Credentials("walrus", "12345678!Aa", false);
            NetworkDevice device = new NetworkDevice("Testing", "192.168.1.254", [1,2], new List<NetworkDevice>(), credentials, @".\assets", ConnectionReadCallback);
            device.terminal.SendCommand("Show Running-Config");
        }
    }
}
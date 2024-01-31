using Renci.SshNet;
using System.Text.Json;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Transactions;
using NetworkDeviceManager;
using CredentialManager;
using SaveManager;
using SNMPBackend;
using System.ComponentModel;

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
            SNMPManager snmpManager = new SNMPManager();

            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            Credentials master = new Credentials(username, Convert.ToBase64String(SymmetricEncryption.Hash(password, "FATWALRUS!")));
            SymmetricEncryption.SetMaster(password);

            Credentials deviceCredentials = new Credentials("walrus", "12345678!Aa", false);
            NetworkDevice device = new NetworkDevice("Testing", "192.168.1.254", [1,2], new List<NetworkDevice>(), deviceCredentials, @".\assets", ConnectionReadCallback);

            device.terminal.Connect();

            bool runTerminal = true;
            while (runTerminal)
            {
                string cmdInput = Console.ReadLine();
                if (cmdInput == "stop")
                {
                    runTerminal = false;
                } else
                {
                    device.terminal.SendCommand(cmdInput);
                }
                break;
            }
            
            device.terminal.Disconnect();

            SaveSystem.Save(@".\assets\SaveFile.sidars", [device], master);
        }
    }
}
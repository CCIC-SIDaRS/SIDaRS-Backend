﻿using Renci.SshNet;
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
            //TestMultipleExecution();
        }

        // Tests the continuous execution of SSH commands using the SSH connection class
        private static void TestMultipleExecution()
        { 
            Dictionary <string, Dictionary<string, string>> client = JsonSerializer.Deserialize<Dictionary<string,Dictionary<string, string>>>(File.ReadAllText(@"C:\Users\skier\Documents\Code\SIDaRS-Backend\PhatWalrus\assests\SSHClients.sidars"));
            SSHConnection TestSwitch = new SSHConnection(client["TestSwitch"]["address"], client["TestSwitch"]["username"], client["TestSwitch"]["password"]);
            TestSwitch.Connect();
            while (true)
            {
                Console.WriteLine("Enter Command: ");
                string command = Console.ReadLine();
                if (command == "ur mother")
                {
                    break;
                }
                else
                {
                    Console.WriteLine (TestSwitch.Execute(command));
                }
            }
            TestSwitch.Diconnect();
        }
        private static void ClientAdditionAndEncryption()
        {
            SSHManager something = new SSHManager(@"C:\Users\skier\Documents\Code\SIDaRS-Backend\PhatWalrus\assests\");
            something.AddClient();
        }
        private static void ClientDecryption()
        {
            Dictionary<string, Dictionary<string, string>> client = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(@"C:\Users\skier\Documents\Code\SIDaRS-Backend\PhatWalrus\assests\SSHClients.sidars"));
            SymmetricEncryption something1 = new SymmetricEncryption("12345678!Aa", null);
            something1.encryptedText = client["Testing"]["password"];
            something1.IV = client["Testing"]["IV"];
            Console.WriteLine(something1.Decrypt());
        }
    }
}
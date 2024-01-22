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
            SSHManager something = new SSHManager(@".\assets");
            something.CiscoCommandCompletion("show running".Split(" "));
            //TestMultipleExecution();
        }

        // Tests the continuous execution of SSH commands using the SSH connection class
        private static void TestMultipleExecution()
        { 
            Dictionary <string, Dictionary<string, string>> client = JsonSerializer.Deserialize<Dictionary<string,Dictionary<string, string>>>(File.ReadAllText(@".\assets\SSHClients.sidars"));
            SSHConnection TestSwitch = new SSHConnection(client["Testing"]["address"], client["Testing"]["username"], ClientDecryption());
            TestSwitch.Connect();
            TestSwitch.CreateShellStream();
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
                    Console.WriteLine (TestSwitch.ExecuteShellStream(command));
                }
            }
            TestSwitch.Diconnect();
        }
        private static void ClientAdditionAndEncryption()
        {
            SSHManager something = new SSHManager(@".\assets\");
            string ID = Console.ReadLine();
            string address = Console.ReadLine();
            string username = Console.ReadLine();
            string password = Console.ReadLine();
            string masterPassword = Console.ReadLine();
            something.AddClient(ID, address, username, password, masterPassword);
        }
        private static string ClientDecryption()
        {
            Dictionary<string, Dictionary<string, string>> client = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(@".\assets\SSHClients.sidars"));
            SymmetricEncryption something1 = new SymmetricEncryption("12345678!Aa", null);
            something1.encryptedText = client["Testing"]["password"];
            something1.IV = client["Testing"]["IV"];
            return something1.Decrypt();
        }
    }
}
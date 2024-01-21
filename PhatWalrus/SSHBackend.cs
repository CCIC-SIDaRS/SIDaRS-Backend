using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Security;
using System.Security.Principal;

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
            }catch (JsonException) { }
            
            Console.WriteLine("Device ID");
            string ID = Console.ReadLine();
            Console.WriteLine("Address");
            string address = Console.ReadLine();
            Console.WriteLine("Username");
            string username = Console.ReadLine();
            Console.WriteLine("Password");
            string password = Console.ReadLine();
            SymmetricEncryption encryptor = new("12345678!Aa", password);
            encryptor.Encrypt();
            password = encryptor.encryptedText;
            Console.WriteLine(password);
            
            currentClients[ID] = new Dictionary<string, string> { ["address"] = address, ["username"] = username, ["password"] = password, ["IV"] = encryptor.IV };
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
    class SymmetricEncryption
    {
        private string encryptionPassword { get; set; }
        private string encryptionSource {  get; set; }
        public string encryptedText { get; set; }
        public string IV;

        public SymmetricEncryption (string encryptionPassword, string? encryptionSource)
        {
            this.encryptionPassword = encryptionPassword;
            this.encryptionSource = encryptionSource ?? "";
        }
        private byte[] DeriveKey()
        {
            byte[] emptySalt = GenerateSalt();
            int iterations = 10000;
            int desiredKeyLength = 16;
            HashAlgorithmName hashMethod = HashAlgorithmName.SHA384;
            return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(encryptionPassword),emptySalt,iterations,hashMethod,desiredKeyLength);
        }
        private byte[] GenerateSalt()
        {
            return Encoding.UTF8.GetBytes("SaltBytes");
        }

        public void Encrypt()
        {
            using Aes aes = Aes.Create();
            aes.Key = DeriveKey();
            aes.Padding = PaddingMode.PKCS7;
            using MemoryStream output = new();
            using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(Encoding.Unicode.GetBytes(encryptionSource));
            cryptoStream.FlushFinalBlock();
            cryptoStream.Close();

            encryptedText = Convert.ToBase64String(output.ToArray());
            IV = Convert.ToBase64String(aes.IV);
        }

        public string Decrypt()
        {
            byte[] encrpytedBytes = Convert.FromBase64String(encryptedText);
            using Aes aes = Aes.Create();
            aes.Key = DeriveKey();
            var testing = Convert.FromBase64String(IV);
            aes.IV = Convert.FromBase64String(IV);
            aes.Padding = PaddingMode.PKCS7;
            using MemoryStream input = new(encrpytedBytes);
            using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream output = new();
            cryptoStream.CopyTo(output);
            return Encoding.Unicode.GetString(output.ToArray());
        }
    }
}
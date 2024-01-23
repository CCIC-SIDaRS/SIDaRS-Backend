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
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Net.Http.Headers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections;

namespace SSHBackend
{
    class SSHConnection
    {
        private SshClient client { get; set; }
        private ShellStream? stream { get; set; }

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

        public void CreateShellStream()
        {
            stream = client.CreateShellStream("customCommand", 80, 24, 800, 600, 1024);
        }

        public string ExecuteExecChannel (string command)
        {
            SshCommand _command = client.CreateCommand(command);
            _command.Execute();
            string result = _command.Result;
            if (_command.Error != "")
            {
                throw new Exception("SSH Command Error " + _command.Error);
            }
            return result;
        }

        public string ExecuteShellStream (string command)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream) + " Please run the create shell stream function before attempting to execute commands through the shell channel");
            }
            StringBuilder answer;
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            WriteStream(command , writer, stream);
            answer = ReadStream(reader);
            return answer.ToString();
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

        private void WriteStream(string cmd, StreamWriter writer, ShellStream stream)
        {
            writer.WriteLine(cmd);
            while (stream.Length == 0)
            {
                Thread.Sleep(500);
            }
        }

        private StringBuilder ReadStream(StreamReader reader)
        {
            StringBuilder result = new StringBuilder();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                result.AppendLine(line);
            }
            return result;
        }
    }

    class SSHManager
    {
        private string assets { get; set; }
        private Dictionary <string, object> catalystCommands { get; set; }

        public SSHManager(string assets)
        {
            this.assets = assets;
            this.catalystCommands = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(assets + @"\CiscoCommandTree.json"));
        }
        public void AddClient(string ID, string address, string username, string devicePassword, string masterPassword)
        {
            Dictionary<string, Dictionary<string, string>> currentClients = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                currentClients = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(assets + "SSHClients.sidars")) ?? new Dictionary<string, Dictionary<string, string>>();
            }catch (JsonException) { }
            SymmetricEncryption encryptor = new(masterPassword, devicePassword);
            encryptor.Encrypt();
            devicePassword = encryptor.encryptedText;
            currentClients[ID] = new Dictionary<string, string> { ["address"] = address, ["username"] = username, ["password"] = devicePassword, ["IV"] = encryptor.IV };
            string json = JsonSerializer.Serialize(currentClients);
            File.WriteAllText(assets + "SSHClients.sidars", string.Empty);
            File.WriteAllText(assets+"SSHClients.sidars", json);
        }
        // takes in the command when the tab key is pressed and returns the most likely completed command
        public string[] CiscoCommandCompletion(string[] currentCommand)
        {
            IEnumerable<string> matchingValues;
            
            if (currentCommand.Length > 1)
            {
                Dictionary<string, object> currentCommandDictionary = catalystCommands;
                try
                {
                    for (int i = 0; i <= currentCommand.Length - 2; i++)
                    {
                        currentCommandDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(currentCommandDictionary[currentCommand[i]].ToString());
                    }
                }catch (KeyNotFoundException ex)
                {
                    throw new KeyNotFoundException(ex.ToString());
                }
                matchingValues = currentCommandDictionary.Keys
                                    .Where(x => x.StartsWith(currentCommand[currentCommand.Length - 1]));
            }
            else
            {
                matchingValues = catalystCommands.Keys
                                    .Where(x => x.StartsWith(currentCommand[0]));
            }
            
            return matchingValues.ToArray();
        }
    }

    // Uses AES to encrypt and decrypt strings, interface is a very jank right now
    // This class should use the master password to the application as the encryption key and the master password should be hashed
    // and only stored in memory for a limited period of time once correctly entered
    class SymmetricEncryption
    {
        private string encryptionPassword { get; set; }
        private string encryptionSource {  get; set; }
        public string? encryptedText { get; set; }
        public string? IV { get; set; }

        public SymmetricEncryption(string encryptionPassword, string? encryptionSource)
        {
            if (encryptionPassword == null)
            {
                throw new ArgumentNullException(nameof(encryptionPassword));
            }
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
            try
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
            }catch (Exception ex)
            {
                throw new Exception("Error during encrpytion " + ex);
            }
        }

        public string Decrypt()
        {
            try
            {
                byte[] encrpytedBytes = Convert.FromBase64String(encryptedText);
                using Aes aes = Aes.Create();
                aes.Key = DeriveKey();
                aes.IV = Convert.FromBase64String(IV);
                aes.Padding = PaddingMode.PKCS7;
                using MemoryStream input = new(encrpytedBytes);
                using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using MemoryStream output = new();
                cryptoStream.CopyTo(output);
                return Encoding.Unicode.GetString(output.ToArray());
            }catch (CryptographicException ex)
            {
                throw new Exception("Decryption error - It is likely the correct password was not entered " + ex);
            }catch (Exception ex)
            {
                throw new Exception("Unknown decryption error " + ex);
            }
        }
    }
}
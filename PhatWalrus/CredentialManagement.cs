using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CredentialManager
{
     class CredentialManager
     {
        private string assets { get; set; }
        private List<Dictionary<string, string>> currentClients { get {
                try
                {
                    return JsonSerializer.Deserialize<List<Dictionary<string, string>>>(File.ReadAllText(assets + "SSHClients.sidars")) ?? new List<Dictionary<string, string>>();
                }
                catch (JsonException)
                {
                    return new List<Dictionary<string, string>>();
                }

            } }
        public CredentialManager(string assets) 
        { 
            this.assets = assets;
        }
        public void AddClient(string deviceName, string address, string username, string devicePassword, string masterPassword)
        {
            string IV;
            string encryptedText;
            SymmetricEncryption.Encrypt(devicePassword, masterPassword, out encryptedText, out IV);
            currentClients.Add (new Dictionary<string, string> { ["name"] = deviceName, ["address"] = address, ["username"] = username, ["password"] = encryptedText, ["IV"] = IV });
        }
        public void SaveClients()
        {
            string json = JsonSerializer.Serialize(currentClients);
            File.WriteAllText(assets + "SSHClients.sidars", string.Empty);
            File.WriteAllText(assets + "SSHClients.sidars", json);
        }
        // Uses AES to encrypt and decrypt strings, interface is a very jank right now
        // This class should use the master password to the application as the encryption key and the master password should be hashed
        // and only stored in memory for a limited period of time once correctly entered
        internal static class SymmetricEncryption
        {

            private static byte[] DeriveKey(string encryptionPassword)
            {
                byte[] emptySalt = GenerateSalt();
                int iterations = 10000;
                int desiredKeyLength = 16;
                HashAlgorithmName hashMethod = HashAlgorithmName.SHA384;
                return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(encryptionPassword), emptySalt, iterations, hashMethod, desiredKeyLength);
            }
            private static byte[] GenerateSalt()
            {
                return Encoding.UTF8.GetBytes("SaltBytes");
            }

            public static void Encrypt(string sourceText, string password, out string text, out string IV)
            {
                try
                {
                    using Aes aes = Aes.Create();
                    aes.Key = DeriveKey(password);
                    aes.Padding = PaddingMode.PKCS7;
                    using MemoryStream output = new();
                    using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(Encoding.Unicode.GetBytes(sourceText));
                    cryptoStream.FlushFinalBlock();
                    cryptoStream.Close();
                    text = Convert.ToBase64String(output.ToArray());
                    IV = Convert.ToBase64String(aes.IV);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error during encrpytion " + ex);
                }
            }

            public static void Decrypt(string encryptedText, string IV, string password, out string decryptedText)
            {
                try
                {
                    byte[] encrpytedBytes = Convert.FromBase64String(encryptedText);
                    using Aes aes = Aes.Create();
                    aes.Key = DeriveKey(password);
                    aes.IV = Convert.FromBase64String(IV);
                    aes.Padding = PaddingMode.PKCS7;
                    using MemoryStream input = new(encrpytedBytes);
                    using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
                    using MemoryStream output = new();
                    cryptoStream.CopyTo(output);
                    decryptedText = Encoding.Unicode.GetString(output.ToArray());
                }
                catch (CryptographicException ex)
                {
                    throw new Exception("Decryption error - It is likely the correct password was not entered " + ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Unknown decryption error " + ex);
                }
            }
        }
    }
}

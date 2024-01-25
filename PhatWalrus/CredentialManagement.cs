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
    class Credentials
    {
        private string username { get; set; }
        private string password { get; set; }
        public Credentials (string username, string password, bool encrypted = true)
        {
            this.username = username;
            if (!encrypted)
            {
                this.password = SymmetricEncryption.Encrypt(password, "12345678!Aa");
            }else
            {
                this.password = password;
            }
        }
        public string[] GetCreds()
        {
            return [username, SymmetricEncryption.Decrypt(password, "12345678!Aa")];
        }
        public string Save()
        {
            return "{" + "username: " + username + ", " + "password: " + password + "}";
        }
    }
    static class SymmetricEncryption
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

        public static string Encrypt(string sourceText, string password)
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
                return Convert.ToBase64String(output.ToArray()) + "'''" + Convert.ToBase64String(aes.IV); ;
            }
            catch (Exception ex)
            {
                throw new Exception("Error during encrpytion " + ex);
            }
        }

        public static string Decrypt(string encryptedText, string password)
        {
            try
            {
                byte[] encrpytedBytes = Convert.FromBase64String(encryptedText.Split("'''")[0]);
                using Aes aes = Aes.Create();
                aes.Key = DeriveKey(password);
                aes.IV = Convert.FromBase64String(encryptedText.Split("'''")[1]);
                aes.Padding = PaddingMode.PKCS7;
                using MemoryStream input = new(encrpytedBytes);
                using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using MemoryStream output = new();
                cryptoStream.CopyTo(output);
                return Encoding.Unicode.GetString(output.ToArray());
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

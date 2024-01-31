using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CredentialManager;
using NetworkDeviceManager;


namespace SaveManager
{
     static class SaveSystem
     {
        public static void Save(string saveFile, NetworkDevice[] networkDevices, Credentials masterCredentials)
        {
            
            Dictionary<string, object> saveDict = new();
            saveDict["MasterCredentials"] = masterCredentials.Save();

            List<string> serializedNetDevices = new();
            foreach (NetworkDevice netDevice in networkDevices)
            {
                serializedNetDevices.Add(netDevice.Save());
            }

            saveDict["NetworkDevices"] = serializedNetDevices;
            Console.WriteLine(JsonSerializer.Serialize(saveDict));
            File.WriteAllText(saveFile, JsonSerializer.Serialize(saveDict));
        }
        public static void Load(string saveFile, out NetworkDevice[] networkDevices, out Credentials masterCredentials)
        {
            string data = File.ReadAllText(saveFile);
            Dictionary<string, string> dict = JsonSerializer.Deserialize<Dictionary<string, string>>(data);
            masterCredentials = JsonSerializer.Deserialize<Credentials>(dict["MasterCredentials"]);
            List<NetworkDevice> tempDevices = new();
            foreach (string device in JsonSerializer.Deserialize<List<string>>(dict["NetworkDevices"]))
            {
                tempDevices.Add(JsonSerializer.Deserialize<NetworkDevice>(device));
            }
            networkDevices = tempDevices.ToArray();
        }
     }
}
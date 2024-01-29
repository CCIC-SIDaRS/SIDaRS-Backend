using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
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
    }
}

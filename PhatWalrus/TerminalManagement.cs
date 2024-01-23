using SSHBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PhatWalrus
{
    class TerminalManagement
    {
        private string assetsFolder { get; set; }
        private Dictionary<string, object> catalystCommands { get; set; }
        public TerminalManagement() {
            this.assetsFolder = assetsFolder;
            this.catalystCommands = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(assetsFolder + @"\CiscoCommandTree.json"));
        }
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
                }
                catch (KeyNotFoundException ex)
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
}

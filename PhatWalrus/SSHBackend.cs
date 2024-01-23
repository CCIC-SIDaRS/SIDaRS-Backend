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
}
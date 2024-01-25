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
using System.Collections;
using CredentialManager;

namespace SSHBackend
{

    class SSHManager
    {
        private SshClient client { get; set; }
        private ShellStream? stream { get; set; }
        private Credentials credentials { get; set; }

        // Can send a command and recieve a response to the command
        // returns a string with the response to the command -- either the error or the result
        public SSHManager(string hostaddress, Credentials credentials)
        {
            client = new SshClient(hostaddress, credentials.GetCreds()[0], credentials.GetCreds()[1]);
            Console.WriteLine(hostaddress);
        }

        public void Connect()
        {
            try
            {
                client.Connect();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }

        public void CreateShellStream()
        {
            stream = client.CreateShellStream("customCommand", 80, 24, 800, 600, 1024);
        }

        public string ExecuteExecChannel(string command)
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

        public string ExecuteShellStream(string command)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream) + " Please run the create shell stream function before attempting to execute commands through the shell channel");
            }
            string answer;
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            WriteStream(command, writer, stream);
            Thread.Sleep(1000);
            answer = ReadStream(reader);
            return answer;
        }

        public void Diconnect()
        {
            try
            {
                client.Disconnect();
            }
            catch (Exception ex)
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

        private string ReadStream(StreamReader reader)
        {
            string result = "";
            string line = "";
            while (line != null)
            {
                line = reader.ReadLine();
                result += line + "\n";
            }
            return result;
        }
    }
}
using Microsoft.Win32;
using System.Management;
using System.ServiceProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Security;

namespace SNMPBackend
{
    class SNMPManager
    {
        public SNMPManager()
        {

        }
        private void StartSNMPTrapService()
        {
            ServiceController controller = new ServiceController("SNMPTrap");
            if (controller.Status == ServiceControllerStatus.Stopped)
                controller.Start();
        }
        // This is the basics of what needs to happen, we will need to figure out how to turn this
        // from using namespaces intended for windows services to using system.net.sockets
        private void Balls()
        {
            //If we are going to received SNMP V3 trap then un-comment below code.  
            var users = new UserRegistry();  
            users.Add(new OctetString("SecurityUserName"),  
            new DefaultPrivacyProvider(new MD5AuthenticationProvider(new OctetString("AuthenticationPassword"))));  
            lst = new Listener { Users = users };  
            lst.AddBinding(new IPEndPoint(IPAddress.Any, 162)); //IP address of listener system  
            lst.MessageReceived += Listener_MessageReceived;
            lst.StartAsync();
        }
        private static void Listener_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            File.AppendAllText("c:/temp/servicelog.log", "Version :" + e.Message.Version + "\n");
            File.AppendAllText("c:/temp/servicelog.log", "Version :" + e.Message.Scope.Pdu.Variables[4].Data.ToString() + "\n");
        }
    }
}
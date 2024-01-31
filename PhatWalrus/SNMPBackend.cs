using Microsoft.Win32;
using System.Management;
using System.ServiceProcess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

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
    }
}
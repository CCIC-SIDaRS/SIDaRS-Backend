using CredentialManager;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CredentialManager;
using Lextm.SharpSnmpLib.Security;

namespace SNMPBackend
{
    class SNMPManager
    {
        private IPEndPoint address { get; set; }
        private SHA1AuthenticationProvider auth { get ; set; }
        private DESPrivacyProvider priv { get; set; }

        public SNMPManager(string address, Credentials credentials) 
        {
            Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
            this.address = new IPEndPoint(IPAddress.Parse(address), 161);
            var something = discovery.GetResponse(60000, this.address);
            this.auth = new SHA1AuthenticationProvider(new OctetString(credentials.GetCreds()[1]));
            this.priv = new DESPrivacyProvider(new OctetString(credentials.GetCreds()[0]), auth);
        }
        public string Get()
        {
            return string.Empty;
        }
    }
}
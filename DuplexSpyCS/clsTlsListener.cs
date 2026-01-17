using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsTlsListener : clsListener
    {
        public clsTlsListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;

            m_protocol = enListenerProtocol.TLS;
        }

        ~clsTlsListener() => fnStop();

        public override void fnStart()
        {
            //base.fnStart();

            m_bIslistening = true;
        }

        public override void fnStop()
        {
            //base.fnStop();

            m_bIslistening = false;
        }
    }
}

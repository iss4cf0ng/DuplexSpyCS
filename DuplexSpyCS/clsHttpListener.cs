using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsHttpListener : clsListener
    {
        public clsHttpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;

            m_protocol = enListenerProtocol.HTTP;
        }

        ~clsHttpListener()
        {

        }

        public override void fnStart()
        {
            m_bIslistening = true;
        }

        public override void fnStop()
        {
            m_bIslistening = false;
        }
    }
}

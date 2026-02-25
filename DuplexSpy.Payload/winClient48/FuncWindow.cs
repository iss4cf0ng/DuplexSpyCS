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
using System.Windows.Forms.VisualStyles;
using static WinAPI;

namespace winClient48
{
    public class FuncWindow
    {
        private Icon GetAppIcon(IntPtr hWnd)
        {
            IntPtr hIcon = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, 0);
            if (IntPtr.Zero == hIcon)
                hIcon = SendMessage(hWnd, WM_GETICON, ICON_SMALL, 0);
            if (IntPtr.Zero == hIcon)
                hIcon = SendMessage(hWnd, WM_GETICON, ICON_BIG, 0);
            if (IntPtr.Zero == hIcon)
                hIcon = GetClassLongPtr(hWnd, GCL_HICON);
            if (IntPtr.Zero == hIcon)
                hIcon = GetClassLongPtr(hWnd, GCL_HICONSM);

            if (IntPtr.Zero == hIcon)
                return null;

            Icon icon = Icon.FromHandle(hIcon);

            return icon;
        }

        public (int, string, List<WindowInfo>) GetWindow()
        {
            int code = 1;
            string msg = string.Empty;
            List<WindowInfo> result = new List<WindowInfo>();

            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if (IsWindowVisible(hWnd))
                    {
                        StringBuilder lpWindowTitle = new StringBuilder(256);
                        GetWindowText(hWnd, lpWindowTitle, lpWindowTitle.Capacity);

                        if (lpWindowTitle.Length > 0)
                        {
                            int nProcessId = -1;
                            try
                            {
                                GetWindowThreadProcessId(hWnd, out nProcessId);
                            }
                            catch (Exception ex)
                            {
                                
                            }

                            Process proc = null;
                            try
                            {
                                proc = Process.GetProcessById(nProcessId);
                            }
                            catch (Exception ex)
                            {
                                
                            }

                            Icon iWindow = null;
                            try
                            {
                                iWindow = GetAppIcon(hWnd);
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.Message, ex.GetType().Name);
                            }

                            string szFilePath = "[Access Denial]";
                            if (proc != null && proc?.Id != null)
                            {
                                string[] aResult = clsGlobal.WMI_QueryNoEncode($"select ExecutablePath from win32_process where ProcessId = {proc.Id}");

                                if (aResult != null && aResult.Length > 0)
                                    szFilePath = aResult[0];
                            }

                            WindowInfo info = new WindowInfo()
                            {
                                szTitle = lpWindowTitle == null ? "[Access Denial]" : lpWindowTitle.ToString(),
                                szProcessName = proc == null ? "[Access Denial]" : proc.ProcessName,
                                szFilePath = szFilePath,
                                nProcessId = nProcessId,
                                nHandle = hWnd == null ? -1 : (int)hWnd,
                                iWindow = iWindow,
                            };

                            result.Add(info);
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg, result);
        }

        /// <summary>
        /// Capture window with Windows API DC.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public (int, string, Bitmap) CaptureWindowWithAPI(IntPtr hWnd)
        {
            int code = 1;
            string msg = string.Empty;
            Bitmap ret = null;

            try
            {
                if (!GetWindowRect(hWnd, out RECT rect))
                    throw new Exception("GetWindowRect() error.");

                int nWidth = rect.Right - rect.Left;
                int nHeight = rect.Bottom - rect.Top;

                using (Bitmap bmp = new Bitmap(nWidth, nHeight))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        IntPtr hdcWindow = GetDC(hWnd);
                        IntPtr hdcBitmap = g.GetHdc();
                        BitBlt(hdcBitmap, 0, 0, nWidth, nHeight, hdcWindow, 0, 0, SRCCOPY);
                        g.ReleaseHdc(hdcBitmap);
                        ReleaseDC(hWnd, hdcWindow);
                    }

                    if (bmp == null)
                        throw new Exception("Bitmap is null");

                    ret = (Bitmap)bmp.Clone();
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg, ret);
        }
        public (int, string, Bitmap) CaptureWindowWithFore(IntPtr hWnd)
        {
            int code = 1;
            string msg = string.Empty;
            Bitmap ret = null;

            try
            {
                if (!GetWindowRect(hWnd, out RECT rect))
                    throw new Exception("GetWindowRect() error.");

                int nWidth = rect.Right - rect.Left;
                int nHeight = rect.Bottom - rect.Top;

                using (Bitmap bmp = new Bitmap(nWidth, nHeight))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        IntPtr hCurrentWnd = GetForegroundWindow();

                        SetForegroundWindow(hWnd);
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(nWidth, nHeight));

                        if (bmp == null)
                            throw new Exception("Bitmap is null");

                        ret = (Bitmap)bmp.Clone();

                        SetForegroundWindow(hCurrentWnd);
                    }
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg, ret);
        }
    }
}

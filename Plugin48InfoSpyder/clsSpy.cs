using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    public class clsSpy
    {
        public string szName { get; set; }
        public string szEntry { get; set; }
        public string szDescription { get; set; }

        protected List<clsInstalledApp> m_lsApp = new List<clsInstalledApp>();

        public clsSpy()
        {
            (RegistryKey root, string path)[] roots = new (RegistryKey, string)[]
            {
                (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
                (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
                (Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
            };

            foreach (var (root, path) in roots)
            {
                using (RegistryKey key = root.OpenSubKey(path))
                {
                    if (key == null)
                        continue;

                    foreach (string szSubKeyName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (RegistryKey subKey = key.OpenSubKey(szSubKeyName))
                            {
                                string szDisplayName = (string)subKey.GetValue("DisplayName");
                                string szDisplayVersion = (string)subKey.GetValue("DisplayVersion");
                                string szUnstallPath = ((string)subKey.GetValue("UninstallString")).Trim('\"');
                                string szInstallDate = (string)subKey.GetValue("InstallDate");

                                if (!string.IsNullOrEmpty(szDisplayName))
                                {
                                    if (m_lsApp.Where(x => string.Equals(szDisplayName, x.Name)).ToList().Count > 0)
                                        continue;

                                    m_lsApp.Add(new clsInstalledApp()
                                    {
                                        Name = szDisplayName,
                                        Version = szDisplayVersion,
                                        AppFolder = Path.GetDirectoryName(szUnstallPath),
                                        InstallDate = szInstallDate,
                                    });
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        public virtual void fnRun(string szModule, List<string> lsArgs)
        {

        }

        public class clsInstalledApp
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string AppFolder { get; set; }
            public string InstallDate { get; set; }
        }
    }
}

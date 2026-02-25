using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

namespace test_window
{
    internal class GetWindow
    {
        // Define necessary Win32 API functions
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool isWow64);

        // EnumWindows callback delegate
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public void ShowAll()
        {
            // Call EnumWindows and pass the callback method
            EnumWindows((hWnd, lParam) =>
            {
                // Get the length of the window title
                int length = GetWindowTextLength(hWnd);
                if (length > 0)
                {
                    // Step 1: Check if the window is visible
                    if (IsWindowVisible(hWnd))
                    {
                        // Step 2: Get the window title
                        StringBuilder sb = new StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        string windowTitle = sb.ToString();

                        // Step 3: Get the process ID associated with the window
                        uint processId;
                        GetWindowThreadProcessId(hWnd, out processId);

                        try
                        {
                            // Get the process using the process ID
                            Process process = Process.GetProcessById((int)processId);

                            // Step 4: Check if the process is 32-bit or 64-bit
                            bool isWow64;
                            IsWow64Process(process.Handle, out isWow64);

                            // Step 5: If the process is 64-bit and the app is 32-bit, skip it
                            string name = Process.GetProcessById((int)processId).ProcessName;
                            if (isWow64) // The process is 32-bit
                            {
                                string executablePath = process.MainModule?.FileName;
                                Console.WriteLine($"Window Handle: {hWnd}");
                                Console.WriteLine($"Title: {windowTitle}");
                                Console.WriteLine($"Process ID: {processId}");
                                Console.WriteLine($"$Process Name: {name}");
                                Console.WriteLine($"Executable Path: {executablePath}");
                                Console.WriteLine("=============================================");
                            }
                            else
                            {
                                Console.WriteLine($"Window Handle: {hWnd}");
                                Console.WriteLine($"Title: {windowTitle}");
                                Console.WriteLine($"Process ID: {processId}");
                                Console.WriteLine($"$Process Name: {name}");
                                Console.WriteLine("Skipping 64-bit process in 32-bit application");
                                Console.WriteLine("=============================================");
                            }
                        }
                        catch (AccessViolationException ex)
                        {
                            // Catch permission errors when accessing system processes or processes with elevated rights
                            Console.WriteLine($"Could not access process information for window {windowTitle}: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            // Handle any other exceptions
                            Console.WriteLine($"Error retrieving process info: {ex.Message}");
                        }
                    }
                }

                // Return true to continue enumeration
                return true;
            }, IntPtr.Zero);
        }
    }

    internal class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        const int THREAD_SUSPEND_RESUME = 0x0002; //THREAD SUSPEND VALUE

        static void Main(string[] args)
        {
            Console.WriteLine("Enter the name of the process to manipulate:");
            string processName = Console.ReadLine();

            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
            {
                Console.WriteLine("No process found with that name.");
                return;
            }

            Console.WriteLine("Enter the operation: 'suspend' or 'resume':");
            string operation = Console.ReadLine()?.ToLower();

            foreach (var process in processes)
            {
                Console.WriteLine($"{operation} process: {process.ProcessName} (PID: {process.Id})");

                foreach (ProcessThread thread in process.Threads)
                {
                    IntPtr threadHandle = OpenThread(THREAD_SUSPEND_RESUME, false, (uint)thread.Id);
                    if (threadHandle != IntPtr.Zero)
                    {
                        if (operation == "suspend")
                        {
                            SuspendThread(threadHandle);
                        }
                        else if (operation == "resume")
                        {
                            ResumeThread(threadHandle);
                        }

                        CloseHandle(threadHandle);
                    }
                }
            }

            Console.WriteLine($"Process {operation} operation completed.");
        }
    }
}

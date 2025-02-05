using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

[DllImport("user32.dll", SetLastError = true)]
static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
static extern IntPtr GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
static extern int GetWindowTextLength(IntPtr hWnd);

Process[] processlist = Process.GetProcesses();

foreach (Process process in processlist)
{
    if (!string.IsNullOrEmpty(process.MainWindowTitle))
    {
        Console.WriteLine(process.MainWindowTitle);
    }
}
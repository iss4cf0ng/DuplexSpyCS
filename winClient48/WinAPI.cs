using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    public class WinAPI
    {
        //ACTIVE WINDOW TITLE
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int count);

        //Shell
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeConsole();
    }
}

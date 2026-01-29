using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using System.Diagnostics;

namespace winClient48
{
    internal class clsfnPower
    {
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        public clsfnPower()
        {

        }

        public static void Shutdown(int nSec) => Process.Start("shutdown", $"/s /t {nSec}");
        public static void Restart(int nSec) => Process.Start("shutdown", $"/r /t {nSec}");
        public static void Logoff() => Process.Start("shutdown", "/l");
        public static void Sleep() => SetSuspendState(false, true, true);
    }
}

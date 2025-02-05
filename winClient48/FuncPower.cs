using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq.Expressions;

namespace winClient48
{
    internal class FuncPower
    {
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        public FuncPower()
        {

        }

        public void Suspend(bool hiberate)
        {
            SetSuspendState(hiberate, false, false);
        }

        public void PowerOff()
        {

        }

        public void ShutdownDisable()
        {

        }
        public void ShutdownEnable()
        {

        }
    }
}

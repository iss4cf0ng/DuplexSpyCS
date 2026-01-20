using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    public class clsSpyMachine : clsSpy
    {
        public clsSpyMachine()
        {
            szName = "InfoSpyder.Machine";
            szEntry = "machine";
            szDescription = "Information on this machine.";
        }

        public override void fnRun(string szModule, List<string> lsArgs)
        {
            
        }
    }
}

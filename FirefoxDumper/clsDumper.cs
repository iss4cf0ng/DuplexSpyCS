using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirefoxDumper
{
    internal class clsDumper
    {
        public clsDumper()
        {

        }

        public struct stCredential
        {
            public long ID { get; set; }
            public string URL { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastUsedDate { get; set; }
        }

        public List<stCredential> fnDumpCredential()
        {

        }
    }
}

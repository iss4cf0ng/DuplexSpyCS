using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin48Dumper
{
    public class clsDumper
    {
        public bool Available { get; set; }
        public string Entry { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }

        protected string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        protected string ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public clsDumper()
        {

        }

        public virtual void fnRun(List<string> lsArgs)
        {

        }

        protected DateTime? fnChromeTimeToDateTime(long webkitTime)
        {
            if (webkitTime <= 0)
                return null;

            try
            {
                DateTime epoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                return epoch.AddTicks(webkitTime * 10);
            }
            catch
            {
                return null;
            }
        }
        protected string fnszChromeDateTime(long webkitTime)
        {
            DateTime? dt = fnChromeTimeToDateTime(webkitTime);
            return dt == null ? "N/A" : dt?.ToString("F");
        }

        protected string fnConnString(string szFilePath) => $"Data Source={szFilePath};Version=3;Read Only=True;";
        protected string fnNewTempFilePath() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class IniManager
    {
        public string Path { get; private set; }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string section, string key, string value, string file_path);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern long GetPrivateProfileString(string section, string key, string default_val, StringBuilder ret_val, int size, string file_path);

        public IniManager(string path)
        {
            Path = new FileInfo(path ?? "config.ini").FullName;
            if (!File.Exists(Path))
                throw new FileNotFoundException("Cannot find: " + path);
        }

        public string Read(string section, string key)
        {
            var ret_val = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", ret_val, 255, Path);
            return ret_val.ToString();
        }

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, Path);
        }
    }
}

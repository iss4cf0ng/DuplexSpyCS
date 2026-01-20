using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    public class clsSpyRemoteMgr : clsSpy
    {
        private string[] m_asTool =
        {
            // Management / Remote Tools
            "mobaxterm",        // Remote terminal tool
            "xshell",           // Remote terminal tool
            "putty",            // SSH/Telnet client
            "securecrt",        // SSH/Telnet client
            "bitvise",          // SSH client
            "process explorer", // System monitoring tool
            "process hacker",   // System monitoring tool
            "winscp",           // SFTP/FTP client

            // Database Tools
            "navicat",          // Database management tool
            "dbeaver",          // Database management tool
            "heidisql",         // MySQL/MariaDB management tool
            "sqlyog",           // MySQL management tool
        };

        public clsSpyRemoteMgr()
        {
            szName = "InfoSpyder.RemoteManager";
            szEntry = "manager";
            szDescription = "Remote manager tools.";
        }

        public override void fnRun(string szModule, List<string> lsArgs)
        {
            if (lsArgs[0] == "ls")
            {
                clsTools.fnLogInfo(new string('-', 50));

                foreach (string szPattern in m_asTool)
                {
                    foreach (var app in m_lsApp)
                    {
                        if (Regex.IsMatch(app.Name, szPattern, RegexOptions.IgnoreCase))
                        {
                            clsTools.fnLogInfo(new string('-', 50));

                            clsTools.fnLogInfo($"[Name]: {app.Name}");
                            clsTools.fnLogInfo($"[Path]: {app.AppFolder}");
                            clsTools.fnLogInfo($"[Version]: {app.Version}");
                            clsTools.fnLogInfo($"[Install Date]: {app.InstallDate}");

                            clsTools.fnLogInfo(new string('-', 50));
                        }
                    }
                }
            }
        }
    }
}

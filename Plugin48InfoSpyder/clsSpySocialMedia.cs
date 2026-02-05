using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    internal class clsSpySocialMedia : clsSpy
    {
        private string[] m_asTool =
        {
            // Social / Communication Tools
            "slack",            // Team collaboration software
            "teams",            // Microsoft Teams
            "discord",          // Communication software
            "skype",            // Communication software
            "zoom",             // Video conferencing software
            "wechat",           // Instant messaging software
            "line",             // Instant messaging software
            "telegram",         // Instant messaging software
        };

        private DataTable dtHelp = new DataTable();

        public clsSpySocialMedia()
        {
            szName = "InfoSpyder.SocialMedia";
            szEntry = "social";
            szDescription = "Social medial applications.";

            dtHelp.Columns.Add("Command");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("help", "Print help message.");
            dtHelp.Rows.Add("ls", "Show information.");
        }

        public override void fnRun(string szModule, List<string> lsArgs)
        {
            if (lsArgs.Count == 0)
            {
                Console.WriteLine("<...> social <help | ls>");
                clsTools.fnPrintTable(dtHelp);
                return;
            }

            if (lsArgs[0] == "help")
            {
                Console.WriteLine("<...> social <help | ls>");
                clsTools.fnPrintTable(dtHelp);
            }
            else if (lsArgs[0] == "ls")
            {
                clsTools.fnLogInfo(new string('-', 50));

                foreach (string szPattern in m_asTool)
                {
                    foreach (var app in m_lsApp)
                    {
                        if (Regex.IsMatch(app.Name, szPattern, RegexOptions.IgnoreCase))
                        {

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

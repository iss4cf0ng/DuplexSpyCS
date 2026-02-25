using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    internal class clsSpyBrowser : clsSpy
    {
        private string[] m_asTool =
        {
            // Browsers
            "chrome",           // Google Chrome
            "firefox",          // Mozilla Firefox
            "edge",             // Microsoft Edge
            "opera",            // Opera browser
            "brave",            // Brave browser
        };

        protected string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        protected string ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string ChromeDir { get { return Path.Combine(ApplicationData, "Google"); } }

        private DataTable dtHelp = new DataTable();

        public clsSpyBrowser()
        {
            szName = "InfoSpyder.Browser";
            szEntry = "browser";
            szDescription = "Browsers";

            dtHelp.Columns.Add("Command");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("help", "Print help message.");
            dtHelp.Rows.Add("ls", "Show information.");
        }

        public override void fnRun(string szModule, List<string> lsArgs)
        {
            if (lsArgs.Count == 0)
            {
                Console.WriteLine("<...> browser <help | ls>");
                clsTools.fnPrintTable(dtHelp);
                return;
            }

            if (lsArgs[0] == "help")
            {
                Console.WriteLine("<...> browser <help | ls>");
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

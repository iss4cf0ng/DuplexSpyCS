using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Data;

namespace Plugin48InfoSpyder
{
    public class clsSpyMachine : clsSpy
    {
        private DataTable dtHelp = new DataTable();

        public clsSpyMachine()
        {
            szName = "InfoSpyder.Machine";
            szEntry = "machine";
            szDescription = "Information on this machine.";

            dtHelp.Columns.Add("Command");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("help", "Print help message.");
            dtHelp.Rows.Add("ls", "Show information.");
        }

        private DataTable fnWQL(string szQuery)
        {
            DataTable dt = new DataTable();

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(szQuery))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        if (dt.Columns.Count == 0)
                        {
                            foreach (PropertyData prop in mo.Properties)
                            {
                                dt.Columns.Add(prop.Name);
                            }
                        }

                        DataRow row = dt.NewRow();
                        foreach (PropertyData prop in mo.Properties)
                        {
                            row[prop.Name] = prop.Value ?? DBNull.Value;
                        }

                        dt.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                clsTools.fnLogError($"{ex.Message}: {szQuery}");
            }

            return dt;
        }

        public override void fnRun(string szModule, List<string> lsArgs)
        {
            if (lsArgs.Count == 0)
            {
                Console.WriteLine("<...> machine <help | ls>");
                clsTools.fnPrintTable(dtHelp);
                return;
            }

            if (lsArgs[0] == "help")
            {
                Console.WriteLine("<...> machine <help | ls>");
                clsTools.fnPrintTable(dtHelp);
            }
            else if (lsArgs[0] == "ls")
            {
                clsTools.fnLogInfo($"Computer Name: {Environment.MachineName}");
                clsTools.fnLogInfo($"Username: {Environment.UserName}");
                clsTools.fnLogInfo($"OS: {Environment.OSVersion}");
                clsTools.fnLogInfo($"Is64: {(Environment.Is64BitOperatingSystem ? "Yes" : "No")}");
                clsTools.fnLogInfo($"Processor Count: {Environment.ProcessorCount}");

                clsTools.fnLogInfo($"Name: {fnWQL("SELECT Name FROM Win32_Processor").Rows[0]["Name"].ToString()}");
                clsTools.fnLogInfo($"TotalPhysicalMemory: {fnWQL("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem").Rows[0]["TotalPhysicalMemory"].ToString()}");
                clsTools.fnLogInfo($"DiskDrive Size: {fnWQL("SELECT Size FROM Win32_DiskDrive").Rows[0]["Model"].ToString()}");
                clsTools.fnLogInfo($"VideoController Name: {fnWQL("SELECT Name FROM Win32_VideoController").Rows[0]["Name"]}");
                clsTools.fnLogInfo($"OS Caption: {fnWQL("SELECT Caption FROM Win32_OperatingSystem").Rows[0]["Caption"]}");
                clsTools.fnLogInfo($"OS Version: {fnWQL("SELECT Version FROM Win32_OperatingSystem").Rows[0]["Version"]}");
            }
        }
    }
}

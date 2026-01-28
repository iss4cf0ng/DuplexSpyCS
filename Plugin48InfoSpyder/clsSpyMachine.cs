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
        public clsSpyMachine()
        {
            szName = "InfoSpyder.Machine";
            szEntry = "machine";
            szDescription = "Information on this machine.";
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
            if (lsArgs[0] == "ls")
            {
                clsTools.fnLogInfo($"Computer Name: {Environment.MachineName}");
                clsTools.fnLogInfo($"Username: {Environment.UserName}");
                clsTools.fnLogInfo($"OS: {Environment.OSVersion}");
                clsTools.fnLogInfo($"Is64: {(Environment.Is64BitOperatingSystem ? "Yes" : "No")}");
                clsTools.fnLogInfo($"Processor Count: {Environment.ProcessorCount}");

                clsTools.fnPrintTable(fnWQL("SELECT Name FROM Win32_Processor"));
                clsTools.fnPrintTable(fnWQL("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"));
                clsTools.fnPrintTable(fnWQL("SELECT Manufacturer, Product"));
                clsTools.fnPrintTable(fnWQL("SELECT Model, Size FROM Win32_DiskDrive"));
                clsTools.fnPrintTable(fnWQL("SELECT Name FROM Win32_VideoController"));
                clsTools.fnPrintTable(fnWQL("SELECT Caption, Version FROM Win32_OperatingSystem"));
                clsTools.fnPrintTable(fnWQL("SELECT * FROM Win32_ComputerSystem"));
            }
        }
    }
}

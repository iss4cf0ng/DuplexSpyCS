using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace winClient48
{
    internal class FuncSystem
    {
        private string regKey_uninstall = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private string query_device = "SELECT * FROM Win32_PnpEntity";

        public FuncSystem()
        {

        }

        //DEVICE
        public List<string[]> Device_ListDevices()
        {
            List<string[]> devices = new List<string[]>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query_device))
            {
                foreach (ManagementObject device in searcher.Get())
                {
                    string name = device["Name"]?.ToString() ?? "X";
                    string status = device["Status"]?.ToString() ?? "X";
                    string manufacturer = device["Manufacturer"]?.ToString() ?? "X";
                    string device_id = device["DeviceID"]?.ToString() ?? "X";
                    string pnp_class = device["PNPClass"]?.ToString() ?? "X";
                    string class_guid = device["ClassGuid"]?.ToString() ?? "X";

                    devices.Add(new string[]
                    {
                        name,
                        status,
                        manufacturer,
                        device_id,
                        pnp_class,
                        class_guid,
                    });
                }
            }

            return devices;
        }
        public string Device_Enable(string id, bool enable)
        {
            string query = $"SELECT * FROM Win32_PnpEntity WHERE DeviceID = '{id}'"; ;
            int code = 0;
            string msg = "?";

            try
            {
                using (ManagementObjectSearcher search = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject device in search.Get())
                    {
                        device.InvokeMethod(enable ? "Enable" : "Disable", null);
                        code = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }


            return $"{code}|{Crypto.b64E2Str(msg)}";
        }

        //INTERFACE
        /// <summary>
        /// List all network interface.
        /// </summary>
        /// <returns></returns>
        public List<string[]> If_ListInterface()
        {
            List<string[]> interfaces = new List<string[]>();
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                interfaces.Add(new string[]
                {
                    networkInterface.Name,
                    networkInterface.Description,
                    networkInterface.NetworkInterfaceType.ToString(),
                    networkInterface.OperationalStatus.ToString(),
                    networkInterface.GetPhysicalAddress().ToString(),
                });
            }

            return interfaces;
        }
        public string If_Enable(string name, bool enable)
        {
            string query = $"SELECT name from win32_networkadapter where netconnectionid = '{name}'";
            int code = 0;
            string msg = "?";

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject adapter in searcher.Get())
                {
                    try 
                    {
                        adapter.InvokeMethod(enable ? "Enable" : "Disable", null);
                        code = 1;
                    }
                    catch (Exception ex)
                    {
                        msg = ex.Message;
                    }
                }
            }

            return $"{code}|{Crypto.b64E2Str(msg)}";
        }

        //APPLICATION
        private string[] SubkeyGetValues(RegistryKey reg_subkey, string[] fields)
        {
            List<string> values = new List<string>();
            int x_cnt = 0;
            foreach (string field in fields)
            {
                try
                {
                    values.Add(reg_subkey.GetValue(field).ToString());
                }
                catch (Exception ex)
                {
                    values.Add("X");
                    x_cnt++;
                }
            }

            if (x_cnt < values.Count)
            {
                return values.ToArray();
            }
            else
            {
                return new string[]
                {
                    Path.GetFileName(reg_subkey.Name),
                    "X",
                    "X",
                    "X",
                    "X",
                };
            }
        }
        public List<string[]> ListApp()
        {
            List<string[]> apps = new List<string[]>();
            string[] fields =
            {
                "DisplayName",
                "Publisher",
                "DisplayVersion",
                "InstallDate",
                "InstallSource",
            };

            using (RegistryKey reg_key = Registry.LocalMachine.OpenSubKey(regKey_uninstall))
            {
                foreach (string subkey in reg_key.GetSubKeyNames())
                {
                    using (RegistryKey reg_subkey = reg_key.OpenSubKey(subkey))
                    {
                        apps.Add(SubkeyGetValues(reg_subkey, fields));
                    }
                }
            }

            return apps;
        }

        /// <summary>
        /// Validate the input string is GUID.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private bool IsGuid(string guid)
        {
            string pattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
            return Regex.IsMatch(guid, pattern);
        }

        public List<string[]> GetStartUp()
        {
            List<string[]> result = new List<string[]>();
            
            return result;
        }

        //ENVIRONMENT VARIABLE
        public string[] GetEnvironmentVariables()
        {
            List<string> variables = new List<string>();
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
                variables.Add($"{entry.Key}={entry.Value}");

            return variables.ToArray();
        }
        public string SetEV()
        {
            return null;
        }
    }
}

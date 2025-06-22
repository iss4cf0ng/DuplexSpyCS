using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace wmi_test
{
    internal class Program
    {
        static void WMI_Query(string query, string[] properties = null)
        {
            using (var searcher = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection coll = searcher.Get())
                {
                    try
                    {
                        foreach (var device in coll)
                        {
                            foreach (PropertyData data in device.Properties)
                            {
                                if (properties != null && properties.Length > 0 && properties.Contains(data.Name))
                                    Console.WriteLine($"{data.Name}: {device[data.Name]}");
                                else if (properties == null || properties.Length == 0)
                                    Console.WriteLine($"{data.Name}: {device[data.Name]}");
                            }

                            Console.WriteLine("-------------------------------");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                }
            }

            Console.WriteLine("\n\n");
        }

        static void Main(string[] args)
        {
            string query = "select * from win32_USBHub";
            string[] properties = new string[]
            {

            };

            //WMI_Query(query);
            //WMI_Query("select * from win32_systembios");
            //WMI_Query("select * from win32_group");
            //WMI_Query("select * from win32_printer");
            //WMI_Query("select name,servicetype,installdate,caption,displayname,processid,description,state,acceptpause,acceptstop,pathname from win32_service");
            WMI_Query("SELECT * from win32_product");
        }
    }
}

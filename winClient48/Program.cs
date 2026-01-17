using System;
using System.Collections.Generic;
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
            // Start the keylogger in the background
            //KeyLogger keylogger = new KeyLogger();
            //keylogger.Start();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args));
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            if (name == "Plugin.Abstractions48")
            {
                // 回傳 Client.exe 裡面 ILMerge 的 Abstractions48 Assembly
                return typeof(IPlugin).Assembly;
            }

            return null;
        }
    }
}

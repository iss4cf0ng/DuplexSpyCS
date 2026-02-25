using System.Diagnostics;
using System.Reflection;

namespace LoadToMemory
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "mem")
            {
                Console.WriteLine("[+] Running from memory.");
                // Now running in memory; nothing more to clean up
                return;
            }

            string exePath = Assembly.GetEntryAssembly().Location;
            Console.WriteLine(exePath);

            // Step 1: Read EXE into memory
            byte[] exeBytes = File.ReadAllBytes(exePath);

            // Step 2: Schedule deletion via cmd (ping = delay)
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c ping 127.0.0.1 -n 2 > nul & del \"{exePath}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] Failed to schedule deletion: " + ex.Message);
            }

            // Step 3: Load from memory and invoke
            try
            {
                Assembly loaded = Assembly.Load(exeBytes);
                MethodInfo entry = loaded.EntryPoint;
                object instance = null;

                if (!entry.IsStatic)
                    instance = loaded.CreateInstance(entry.Name);

                entry.Invoke(instance, new object[] { new string[] { "mem" } });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] Failed to run in memory: " + ex.Message);
            }

            // Optional: Kill original process after invoking in-memory version
            Environment.Exit(0);
        }
    }
}

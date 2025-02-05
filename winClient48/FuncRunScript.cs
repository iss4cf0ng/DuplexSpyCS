using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    internal class FuncRunScript
    {
        public FuncRunScript()
        {

        }

        public (int, string) EvaluateCS(string code, string[] szArr_Params)
        {
            int ret = 1;
            StringWriter sw_output = new StringWriter();

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters param = new CompilerParameters()
            {
                GenerateInMemory = true,
            };

            Console.SetOut(sw_output);

            CompilerResults result = provider.CompileAssemblyFromSource(param, code);
            if (result.Errors.HasErrors)
            {
                Console.SetError(sw_output);
                ret = 0;
            }

            Assembly assembly = result.CompiledAssembly;
            Type program_type = assembly.GetType("Program");
            MethodInfo main_method = program_type.GetMethod("Main");

            main_method.Invoke(null, szArr_Params);

            return (ret, sw_output.ToString());
        }

        public (int, string) EvaluateVB(string code, string[] szArr_Params)
        {
            int ret = 1;
            StringWriter sw_output = new StringWriter();

            VBCodeProvider provider = new VBCodeProvider();
            CompilerParameters param = new CompilerParameters()
            {
                GenerateInMemory = true,
            };

            Console.SetOut(sw_output);

            CompilerResults result = provider.CompileAssemblyFromSource(param, code);
            if (result.Errors.HasErrors)
            {
                Console.SetError(sw_output);
                ret = 0;
            }

            Assembly assembly = result.CompiledAssembly;
            Type program_type = assembly.GetType("Program");
            MethodInfo main_method = program_type.GetMethod("Main");

            main_method.Invoke(null, szArr_Params);

            return (ret, sw_output.ToString());
        }

        public (int, string) ExecBatch(string script, string[] szArr_Params)
        {
            int ret = 1;
            string msg = string.Empty;

            try
            {
                string szTmpFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Templates), Path.GetTempFileName() + ".bat");
                File.WriteAllText(szTmpFile, script);

                if (!File.Exists(szTmpFile))
                    throw new FileNotFoundException(szTmpFile);

                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = szTmpFile, //batch file
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    },
                };

                proc.Start();

                string output = proc.StandardOutput.ReadToEnd();
                string err = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                //Detect error
                if (!string.IsNullOrEmpty(err))
                {
                    msg = err;
                    ret = 0;
                }
                else
                {
                    msg = output;
                }

                File.Delete(szTmpFile);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                ret = 0;
            }

            return (ret, msg);
        }
    }
}
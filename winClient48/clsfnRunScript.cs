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
    internal class clsfnRunScript
    {
        public clsfnRunScript()
        {

        }

        public (int nCode, string szMsg) EvaluateCS(string code, string[] szArr_Params)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            StringWriter sw_output = new StringWriter();
            TextWriter originalOut = Console.Out;

            try
            {
                Console.SetOut(sw_output);

                var provider = new CSharpCodeProvider();
                var param = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = true
                };

                CompilerResults result = provider.CompileAssemblyFromSource(param, code);

                if (result.Errors.HasErrors)
                {
                    foreach (CompilerError error in result.Errors)
                        sw_output.WriteLine(error.ToString());

                    Console.SetOut(originalOut);
                    return (0, sw_output.ToString());
                }

                MethodInfo main = result.CompiledAssembly.EntryPoint;

                //Main(string[] args)
                main.Invoke(null, new object[] { szArr_Params });

                Console.SetOut(originalOut);

                nCode = 1;
            }
            catch (Exception ex)
            {
                sw_output.WriteLine(ex.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            return (nCode, sw_output.ToString());
        }

        public (int nCode, string szMsg) EvaluateVB(string code, string[] szArr_Params)
        {
            int nCode = 0;
            StringWriter sw_output = new StringWriter();
            TextWriter originalOut = Console.Out;

            try
            {
                Console.SetOut(sw_output);

                VBCodeProvider provider = new VBCodeProvider();
                CompilerParameters param = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = true
                };

                CompilerResults result =  provider.CompileAssemblyFromSource(param, code);

                if (result.Errors.HasErrors)
                {
                    foreach (CompilerError err in result.Errors)
                        sw_output.WriteLine(err.ToString());

                    return (0, sw_output.ToString());
                }

                MethodInfo main = result.CompiledAssembly.EntryPoint;

                //Main(String())
                if (main.GetParameters().Length == 1)
                    main.Invoke(null, new object[] { szArr_Params });
                else
                    main.Invoke(null, null);

                nCode = 1;
            }
            catch (Exception ex)
            {
                sw_output.WriteLine(ex.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }

            return (nCode, sw_output.ToString());
        }


        public (int nCode, string szMsg) ExecBatch(string script, string[] szArr_Params)
        {
            int ret = 1;
            string msg = string.Empty;

            try
            {
                string szTmpFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".bat");
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
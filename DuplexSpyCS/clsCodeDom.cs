using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using System.Windows.Forms;
using System.IO;

namespace DuplexSpyCS
{
    internal class clsCodeDom
    {
        public static bool Compile()
        {
            CodeDomProvider compiler = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters();
            CompilerResults cResult = default;


            return true;
        }
    }
}

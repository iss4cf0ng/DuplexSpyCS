using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    internal class clsHttpReq
    {
        private string m_szHost { get; set; }
        private string m_szPath { get; set; }
        private enMethod m_method { get; set; }
        private string m_szMsg { get; set; }

        public clsHttpReq(string szHost, string szPath, enMethod method, string szMsg)
        {
            m_szHost = szHost;
            m_szPath = szPath;
            m_method = method;
            m_szMsg = szMsg;
        }

        public enum enMethod
        {
            POST,
            PUT,
        }

        public byte[] fnabGetRequest<T>(T data)
        {
            if (data is string szMsg)
            {

            }
            else if (data is byte[] abMsg)
            {

            }

            return new byte[] { };
        }
    }
}

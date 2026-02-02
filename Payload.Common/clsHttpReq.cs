using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payload.Common
{
    internal class clsHttpReq
    {
        private string m_szHost { get; set; } //HTTP host.
        private string m_szPath { get; set; } //HTTP path.
        private string m_szMethod { get; set; } //HTTP request method.
        private string m_szUA { get; set; } //Client user-agent.
        private string m_szMsg { get; set; } //Body.

        public clsHttpReq(string szHost, string szPath, string szMethod, string szUA, string szMsg)
        {
            m_szHost = szHost;
            m_szPath = szPath;
            m_szMethod = szMethod;
            m_szUA = szUA;
            m_szMsg = szMsg;
        }

        public enum enMethod { POST, PUT }

        public byte[] fnabGetRequest()
        {
            string szResp =
                $"{m_szMethod} HTTP/1.1\r\n" +
                $"Host: {m_szHost}\r\n" +
                $"User-Agent: {m_szUA}\r\n" +
                $"Accept: */*\r\n" +
                $"Content-Type: application/text\r\n";

            szResp += $"Content-Length: {m_szMsg.Length}\r\n\r\n";
            szResp += m_szMsg;

            return Encoding.UTF8.GetBytes(szResp);
        }
    }
}

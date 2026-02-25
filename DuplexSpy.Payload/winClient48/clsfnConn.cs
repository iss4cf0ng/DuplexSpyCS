using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    public class clsfnConn
    {
        public clsfnConn()
        {

        }

        public string GetConn()
        {
            List<string> result = new List<string>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcp_conns = properties.GetActiveTcpConnections();

            UdpStatistics udp_stats_ipv4 = properties.GetUdpIPv4Statistics();
            UdpStatistics udp_stats_ipv6 = properties.GetUdpIPv6Statistics();

            foreach (var tcp in tcp_conns)
            {
                if (tcp != null)
                {
                    string data = $"" +
                        $"{tcp.LocalEndPoint.AddressFamily.ToString()}," +
                        $"{tcp.LocalEndPoint.ToString()}," +
                        $"{tcp.RemoteEndPoint.ToString()}," +
                        $"{tcp.State.ToString()}";
                    result.Add(data);
                }
            }

            return string.Join(";", result);
        }
    }
}
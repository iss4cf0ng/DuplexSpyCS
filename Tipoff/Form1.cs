using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using PacketDotNet;
using SharpPcap;

namespace Tipoff
{
    public partial class Form1 : Form
    {
        private Socket g_Socket;
        private bool m_bConnected = false;

        private int m_nTimeout = 10000; //ms
        private int m_nRetry = 10000; //ms

        public Form1()
        {
            InitializeComponent();
        }

        private void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var pktRaw = e.Packet;
            Packet pkt = Packet.ParsePacket(pktRaw.LinkLayerType, pktRaw.Data);

            var pktTCP = pkt.Extract(typeof(TcpPacket)) as TcpPacket;
            if (pktTCP != null && pktTCP.Syn)
            {

            }
        }

        void fnMain()
        {
            var devices = CaptureDeviceList.Instance;
            if (devices.Count == 0)
                return;

            foreach (var device in devices)
            {
                device.Open(DeviceMode.Promiscuous, 1000);
                device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
                device.StartCapture();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            fnMain();
        }
    }
}

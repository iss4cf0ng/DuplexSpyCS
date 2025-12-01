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
    public partial class frmListenerEdit : Form
    {
        private stListenerConfig m_config { get; set; }
        private clsSqlConn m_sqlConn { get; set; }

        public frmListenerEdit(stListenerConfig config, clsSqlConn sqlConn)
        {
            InitializeComponent();

            m_config = config;
            m_sqlConn = sqlConn;
        }

        void fnSetup()
        {
            //Controls
            foreach (string s in Enum.GetNames(typeof(enListenerProtocol)))
                comboBox1.Items.Add(s);

            comboBox1.SelectedIndex = 0;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; //ReadOnly.

            if (!string.IsNullOrEmpty(m_config.szName))
            {
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    if (string.Equals(comboBox1.Items[i].ToString(), Enum.GetName(typeof(enListenerProtocol), m_config.enProtocol)))
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }

                textBox1.Text        = m_config.szName;
                numericUpDown1.Value = m_config.nPort;
                textBox2.Text        = m_config.szDescription;
            }
        }

        private void frmListenerEdit_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stListenerConfig config = new stListenerConfig()
            {
                szName         = textBox1.Text,
                nPort          = (int)numericUpDown1.Value,
                szDescription  = textBox2.Text,

                dtCreationDate = DateTime.Now,
            };

            if (!m_sqlConn.fnbSaveListener(config))
            {
                MessageBox.Show("Save listener failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

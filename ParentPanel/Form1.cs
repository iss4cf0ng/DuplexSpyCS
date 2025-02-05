using System.Runtime.InteropServices;

namespace ParentPanel
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hwndChild, IntPtr hwndNewParent);

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            frmMsgBox f = new frmMsgBox();
            SetParent(f.Handle, Handle);
            f.Show();
        }
    }
}

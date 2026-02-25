using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public partial class frmLockScreen : Form
    {
        public frmLockScreen()
        {
            InitializeComponent();
        }

        public bool allow_close = false;

        private Color GetBackgroundColor(Bitmap bitmap)
        {
            // Collect colors from the image border
            var borderColors = new System.Collections.Generic.List<Color>();

            int width = bitmap.Width;
            int height = bitmap.Height;

            // Add pixels from the top and bottom edges
            for (int x = 0; x < width; x++)
            {
                borderColors.Add(bitmap.GetPixel(x, 0));            // Top edge
                borderColors.Add(bitmap.GetPixel(x, height - 1));  // Bottom edge
            }

            // Add pixels from the left and right edges
            for (int y = 0; y < height; y++)
            {
                borderColors.Add(bitmap.GetPixel(0, y));           // Left edge
                borderColors.Add(bitmap.GetPixel(width - 1, y));   // Right edge
            }

            // Find the most common color among the border pixels
            var mostCommonColor = FindMostCommonColor(borderColors);
            return mostCommonColor;
        }

        private Color FindMostCommonColor(System.Collections.Generic.List<Color> colors)
        {
            var colorCount = new System.Collections.Generic.Dictionary<Color, int>();

            foreach (var color in colors)
            {
                if (colorCount.ContainsKey(color))
                    colorCount[color]++;
                else
                    colorCount[color] = 1;
            }

            // Return the color with the highest frequency
            int maxCount = 0;
            Color mostCommon = Color.Empty;

            foreach (var kvp in colorCount)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    mostCommon = kvp.Key;
                }
            }

            return mostCommon;
        }

        public void ShowImage(string b64_img)
        {
            Image img = clsEZData.fnBase64ToImage(b64_img);
            Color color = GetBackgroundColor((Bitmap)img);
            pictureBox1.BackColor = color;
            pictureBox1.Image = img;
            Activate();
        }

        private void frmLockScreen_Load(object sender, EventArgs e)
        {
            BackColor = Color.Black;
            pictureBox1.BackColor = Color.Black;
            Visible = false;
            ShowInTaskbar = false;

            timer1.Interval = 30;
            timer1.Start();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void frmLockScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allow_close)
                e.Cancel = true;

            timer1.Stop();
        }

        private void frmLockScreen_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this == null || this.IsDisposed)
                return;

            try
            {
                TopMost = true;
                WinAPI.SetForegroundWindow(this.Handle);
            }
            catch
            {

            }
        }
    }
}

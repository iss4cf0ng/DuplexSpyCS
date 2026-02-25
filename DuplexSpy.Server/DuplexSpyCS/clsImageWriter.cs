using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    internal class clsImageWriter
    {
        //Write MP4
        private readonly string filePath;
        private readonly ConcurrentQueue<(float Fps, Image image)> imageQueue;
        private readonly SemaphoreSlim fileLock;
        private bool isProcessing;
        private System.Windows.Forms.Timer timerWriter;

        //OpenCV
        private VideoWriter videoWriter;
        private int videoRate { get; set; }
        private int imgWidth { get; set; }
        private int imgHeight { get; set; }

        public clsImageWriter(string filePath, long fileLen, int imgWidth, int imgHeight)
        {
            this.filePath = filePath;
            imageQueue = new ConcurrentQueue<(float, Image)>();
            fileLock = new SemaphoreSlim(1, 1);
            isProcessing = false;

            timerWriter = new System.Windows.Forms.Timer();
            timerWriter.Interval = 1000; //One second.
            timerWriter.Tick += TimerWriter_Tick;
            timerWriter.Start();

            videoRate = 30;
            this.imgWidth = imgWidth;
            this.imgHeight = imgHeight;
            videoWriter = new VideoWriter(filePath, FourCC.MP4V, 20, new OpenCvSharp.Size(imgWidth, imgHeight));
        }

        //Process Queue
        private void TimerWriter_Tick(object? sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (isProcessing)
                return;

            isProcessing = true;
            Task.Run(async () =>
            {
                try
                {
                    while (imageQueue.TryDequeue(out var val))
                    {
                        await fileLock.WaitAsync();
                        try
                        {
                            using (Mat frame = ImageToMat(val.image))
                            {
                                if (frame.Width != imgWidth || frame.Height != imgHeight)
                                    Cv2.Resize(frame, frame, new OpenCvSharp.Size(imgWidth, imgHeight));

                                videoWriter.Write(frame);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("AA");
                        }
                        finally
                        {
                            fileLock.Release();
                        }
                        MessageBox.Show("A");
                    }
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message); }
            });

            isProcessing = false;
        }

        public int Close()
        {
            timerWriter.Stop();
            videoWriter.Release();
            return 1;
        }

        public void EnqueueImage(float fps, Image img)
        {
            imageQueue.Enqueue((fps, img));
        }

        private void ProcessQueue()
        {
            if (isProcessing)
                return;

            isProcessing = true;

            //LOCK
            Task.Run(async () =>
            {
                //MessageBox.Show(imageQueue.Count.ToString());
                await fileLock.WaitAsync();
                while (imageQueue.TryDequeue(out var val))
                {
                    try
                    {
                        using (Mat frame = ImageToMat(val.image))
                        {
                            if (frame.Width != imgWidth || frame.Height != imgHeight)
                                Cv2.Resize(frame, frame, new OpenCvSharp.Size(imgWidth, imgHeight));

                            videoWriter.Write(frame);
                        }
                    }
                    finally
                    {
                        
                    }
                }
                fileLock.Release();
            });

            isProcessing = false;
        }

        private Mat ImageToMat(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
            }
        }
    }
}

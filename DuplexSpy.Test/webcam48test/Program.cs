using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace webcam48test
{
    internal static class Win32
    {
        // Start of USER messages
        internal const int WM_USER = 1024;

        // Defines start of the message range
        internal const int WM_CAP_START = WM_USER;

        // Start of unicode messages
        internal const int WM_CAP_UNICODE_START = WM_USER + 100;

        internal const int WM_CAP_GET_CAPSTREAMPTR = (WM_CAP_START + 1);

        internal const int WM_CAP_SET_CALLBACK_ERRORW = (WM_CAP_UNICODE_START + 2);
        internal const int WM_CAP_SET_CALLBACK_STATUSW = (WM_CAP_UNICODE_START + 3);
        internal const int WM_CAP_SET_CALLBACK_ERRORA = (WM_CAP_START + 2);
        internal const int WM_CAP_SET_CALLBACK_STATUSA = (WM_CAP_START + 3);
#if UNICODE
        internal const int WM_CAP_SET_CALLBACK_ERROR       = WM_CAP_SET_CALLBACK_ERRORW;
        internal const int WM_CAP_SET_CALLBACK_STATUS      = WM_CAP_SET_CALLBACK_STATUSW;
#else
        internal const int WM_CAP_SET_CALLBACK_ERROR = WM_CAP_SET_CALLBACK_ERRORA;
        internal const int WM_CAP_SET_CALLBACK_STATUS = WM_CAP_SET_CALLBACK_STATUSA;
#endif

        internal const int WM_CAP_SET_CALLBACK_YIELD = (WM_CAP_START + 4);
        internal const int WM_CAP_SET_CALLBACK_FRAME = (WM_CAP_START + 5);
        internal const int WM_CAP_SET_CALLBACK_VIDEOSTREAM = (WM_CAP_START + 6);
        internal const int WM_CAP_SET_CALLBACK_WAVESTREAM = (WM_CAP_START + 7);
        internal const int WM_CAP_GET_USER_DATA = (WM_CAP_START + 8);
        internal const int WM_CAP_SET_USER_DATA = (WM_CAP_START + 9);

        internal const int WM_CAP_DRIVER_CONNECT = (WM_CAP_START + 10);
        internal const int WM_CAP_DRIVER_DISCONNECT = (WM_CAP_START + 11);

        internal const int WM_CAP_DRIVER_GET_NAMEA = (WM_CAP_START + 12);
        internal const int WM_CAP_DRIVER_GET_VERSIONA = (WM_CAP_START + 13);
        internal const int WM_CAP_DRIVER_GET_NAMEW = (WM_CAP_UNICODE_START + 12);
        internal const int WM_CAP_DRIVER_GET_VERSIONW = (WM_CAP_UNICODE_START + 13);
#if UNICODE
        internal const int WM_CAP_DRIVER_GET_NAME          = WM_CAP_DRIVER_GET_NAMEW;
        internal const int WM_CAP_DRIVER_GET_VERSION       = WM_CAP_DRIVER_GET_VERSIONW;
#else
        internal const int WM_CAP_DRIVER_GET_NAME = WM_CAP_DRIVER_GET_NAMEA;
        internal const int WM_CAP_DRIVER_GET_VERSION = WM_CAP_DRIVER_GET_VERSIONA;
#endif

        internal const int WM_CAP_DRIVER_GET_CAPS = (WM_CAP_START + 14);

        internal const int WM_CAP_FILE_SET_CAPTURE_FILEA = (WM_CAP_START + 20);
        internal const int WM_CAP_FILE_GET_CAPTURE_FILEA = (WM_CAP_START + 21);
        internal const int WM_CAP_FILE_SAVEASA = (WM_CAP_START + 23);
        internal const int WM_CAP_FILE_SAVEDIBA = (WM_CAP_START + 25);
        internal const int WM_CAP_FILE_SET_CAPTURE_FILEW = (WM_CAP_UNICODE_START + 20);
        internal const int WM_CAP_FILE_GET_CAPTURE_FILEW = (WM_CAP_UNICODE_START + 21);
        internal const int WM_CAP_FILE_SAVEASW = (WM_CAP_UNICODE_START + 23);
        internal const int WM_CAP_FILE_SAVEDIBW = (WM_CAP_UNICODE_START + 25);
#if UNICODE
        internal const int WM_CAP_FILE_SET_CAPTURE_FILE    = WM_CAP_FILE_SET_CAPTURE_FILEW;
        internal const int WM_CAP_FILE_GET_CAPTURE_FILE    = WM_CAP_FILE_GET_CAPTURE_FILEW;
        internal const int WM_CAP_FILE_SAVEAS              = WM_CAP_FILE_SAVEASW;
        internal const int WM_CAP_FILE_SAVEDIB             = WM_CAP_FILE_SAVEDIBW;
#else
        internal const int WM_CAP_FILE_SET_CAPTURE_FILE = WM_CAP_FILE_SET_CAPTURE_FILEA;
        internal const int WM_CAP_FILE_GET_CAPTURE_FILE = WM_CAP_FILE_GET_CAPTURE_FILEA;
        internal const int WM_CAP_FILE_SAVEAS = WM_CAP_FILE_SAVEASA;
        internal const int WM_CAP_FILE_SAVEDIB = WM_CAP_FILE_SAVEDIBA;
#endif

        // Out of order to save on ifdefs
        internal const int WM_CAP_FILE_ALLOCATE = (WM_CAP_START + 22);
        internal const int WM_CAP_FILE_SET_INFOCHUNK = (WM_CAP_START + 24);

        internal const int WM_CAP_EDIT_COPY = (WM_CAP_START + 30);

        internal const int WM_CAP_SET_AUDIOFORMAT = (WM_CAP_START + 35);
        internal const int WM_CAP_GET_AUDIOFORMAT = (WM_CAP_START + 36);

        internal const int WM_CAP_DLG_VIDEOFORMAT = (WM_CAP_START + 41);
        internal const int WM_CAP_DLG_VIDEOSOURCE = (WM_CAP_START + 42);
        internal const int WM_CAP_DLG_VIDEODISPLAY = (WM_CAP_START + 43);
        internal const int WM_CAP_GET_VIDEOFORMAT = (WM_CAP_START + 44);
        internal const int WM_CAP_SET_VIDEOFORMAT = (WM_CAP_START + 45);
        internal const int WM_CAP_DLG_VIDEOCOMPRESSION = (WM_CAP_START + 46);

        internal const int WM_CAP_SET_PREVIEW = (WM_CAP_START + 50);
        internal const int WM_CAP_SET_OVERLAY = (WM_CAP_START + 51);
        internal const int WM_CAP_SET_PREVIEWRATE = (WM_CAP_START + 52);
        internal const int WM_CAP_SET_SCALE = (WM_CAP_START + 53);
        internal const int WM_CAP_GET_STATUS = (WM_CAP_START + 54);
        internal const int WM_CAP_SET_SCROLL = (WM_CAP_START + 55);

        internal const int WM_CAP_GRAB_FRAME = (WM_CAP_START + 60);
        internal const int WM_CAP_GRAB_FRAME_NOSTOP = (WM_CAP_START + 61);

        internal const int WM_CAP_SEQUENCE = (WM_CAP_START + 62);
        internal const int WM_CAP_SEQUENCE_NOFILE = (WM_CAP_START + 63);
        internal const int WM_CAP_SET_SEQUENCE_SETUP = (WM_CAP_START + 64);
        internal const int WM_CAP_GET_SEQUENCE_SETUP = (WM_CAP_START + 65);

        internal const int WM_CAP_SET_MCI_DEVICEA = (WM_CAP_START + 66);
        internal const int WM_CAP_GET_MCI_DEVICEA = (WM_CAP_START + 67);
        internal const int WM_CAP_SET_MCI_DEVICEW = (WM_CAP_UNICODE_START + 66);
        internal const int WM_CAP_GET_MCI_DEVICEW = (WM_CAP_UNICODE_START + 67);
#if UNICODE
        internal const int WM_CAP_SET_MCI_DEVICE           = WM_CAP_SET_MCI_DEVICEW;
        internal const int WM_CAP_GET_MCI_DEVICE           = WM_CAP_GET_MCI_DEVICEW;
#else
        internal const int WM_CAP_SET_MCI_DEVICE = WM_CAP_SET_MCI_DEVICEA;
        internal const int WM_CAP_GET_MCI_DEVICE = WM_CAP_GET_MCI_DEVICEA;
#endif

        internal const int WM_CAP_STOP = (WM_CAP_START + 68);
        internal const int WM_CAP_ABORT = (WM_CAP_START + 69);

        internal const int WM_CAP_SINGLE_FRAME_OPEN = (WM_CAP_START + 70);
        internal const int WM_CAP_SINGLE_FRAME_CLOSE = (WM_CAP_START + 71);
        internal const int WM_CAP_SINGLE_FRAME = (WM_CAP_START + 72);

        internal const int WM_CAP_PAL_OPENA = (WM_CAP_START + 80);
        internal const int WM_CAP_PAL_SAVEA = (WM_CAP_START + 81);
        internal const int WM_CAP_PAL_OPENW = (WM_CAP_UNICODE_START + 80);
        internal const int WM_CAP_PAL_SAVEW = (WM_CAP_UNICODE_START + 81);
#if UNICODE
        internal const int WM_CAP_PAL_OPEN                 = WM_CAP_PAL_OPENW;
        internal const int WM_CAP_PAL_SAVE                 = WM_CAP_PAL_SAVEW;
#else
        internal const int WM_CAP_PAL_OPEN = WM_CAP_PAL_OPENA;
        internal const int WM_CAP_PAL_SAVE = WM_CAP_PAL_SAVEA;
#endif

        internal const int WM_CAP_PAL_PASTE = (WM_CAP_START + 82);
        internal const int WM_CAP_PAL_AUTOCREATE = (WM_CAP_START + 83);
        internal const int WM_CAP_PAL_MANUALCREATE = (WM_CAP_START + 84);

        // Following added post VFW 1.1
        internal const int WM_CAP_SET_CALLBACK_CAPCONTROL = (WM_CAP_START + 85);

        // Defines end of the message range
        internal const int WM_CAP_UNICODE_END = WM_CAP_PAL_SAVEW;
        internal const int WM_CAP_END = WM_CAP_UNICODE_END;

        [StructLayout(LayoutKind.Sequential)]
        internal struct VideoHeader
        {
            public IntPtr lpData;
            public int dwBufferLength;
            public int dwBytesUsed;
            public uint dwTimeCaptured;
            public uint dwUser;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CapDriverCaps
        {
            public uint wDeviceIndex;
            public int fHasOverlay;
            public int fHasDlgVideoSource;
            public int fHasDlgVideoFormat;
            public int fHasDlgVideoDisplay;
            public int fCaptureInitialized;
            public int fDriverSuppliesPalettes;
            public IntPtr hVideoIn;
            public IntPtr hVideoOut;
            public IntPtr hVideoExtIn;
            public IntPtr hVideoExtOut;
        }

        internal delegate int FrameCallback(IntPtr hWnd, ref VideoHeader VideoHeader);

        [DllImport("user32", EntryPoint = "DestroyWindow")]
        internal static extern int DestroyWindow(IntPtr hWnd);

        [DllImport("user32", EntryPoint = "SendMessage")]
        internal static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32", EntryPoint = "SendMessage")]
        internal static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, FrameCallback fpProc);

        [DllImport("user32", EntryPoint = "SendMessage")]
        internal static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, ref CapDriverCaps caps);

        [DllImport("user32", EntryPoint = "SendMessage")]
        internal static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr ptr);

        [DllImport("avicap32.dll", EntryPoint = "capCreateCaptureWindowA", CharSet = CharSet.Ansi)]
        internal static extern IntPtr CapCreateCaptureWindow(string lpszWindowName, int dwStyle, int X, int Y, int nWidth, int nHeight, int hwndParent, int nID);

        [DllImport("user32", EntryPoint = "OpenClipboard")]
        internal static extern int OpenClipboard(IntPtr hWnd);

        [DllImport("user32", EntryPoint = "EmptyClipboard")]
        internal static extern int EmptyClipboard();

        [DllImport("user32", EntryPoint = "CloseClipboard")]
        internal static extern int CloseClipboard();
    }
    public sealed class CamException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CamException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CamException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CamException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CamException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CamException"/> class.
        /// </summary>
        public CamException()
            : base()
        {
        }
    }

    class Webcam : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CamVideoFormat
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        public delegate void CamPreviewCallback(Image PreviewImage);
        public Webcam(IWin32Window owner, CamPreviewCallback previewCallback = null, int previewRate = 66)
        {
            // Store owner window
            this.owner = owner;

            // Create and set the preview (capture) window
            SetPreviewWindow(CreatePreviewWindow());

            // Set up timer
            previewTimer = new Timer();
            previewTimer.Enabled = false;
            previewTimer.Tick += PreviewTick;

            // Init variables
            PreviewRate = previewRate;
            SetPreviewCallback(previewCallback);
        }

        ~Webcam()
        {
            Dispose();
        }

        #region Public Properties

        /// <summary>
        /// Gets the value indicating whether the class has been disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// The rate in of capture when in preview mode (in milliseconds).
        /// </summary>
        public int PreviewRate
        {
            get
            {
                return previewTimer.Interval;
            }
            set
            {
                previewTimer.Interval = value;
            }
        }

        /// <summary>
        /// Gets the video format used for capture.
        /// </summary>
        public CamVideoFormat VideoFormat
        {
            get
            {
                CamVideoFormat res;
                var size = Win32.SendMessage(camWindowHandle, Win32.WM_CAP_GET_VIDEOFORMAT, 0, 0);
                var lpVideoFormat = Marshal.AllocHGlobal(size);
                Win32.SendMessage(camWindowHandle, Win32.WM_CAP_GET_VIDEOFORMAT, size, lpVideoFormat);
                res = (CamVideoFormat)Marshal.PtrToStructure(lpVideoFormat, typeof(CamVideoFormat));
                Marshal.FreeHGlobal(lpVideoFormat);
                return res;
            }

            set
            {
                var size = Win32.SendMessage(camWindowHandle, Win32.WM_CAP_GET_VIDEOFORMAT, 0, 0);
                var lpVideoFormat = Marshal.AllocHGlobal(size);
                Win32.SendMessage(camWindowHandle, Win32.WM_CAP_GET_VIDEOFORMAT, size, lpVideoFormat);
                Marshal.StructureToPtr(value, lpVideoFormat, true);
                var result = Win32.SendMessage(camWindowHandle, Win32.WM_CAP_SET_VIDEOFORMAT, size, lpVideoFormat);
                Marshal.FreeHGlobal(lpVideoFormat);
                if (result == 0)
                    throw new CamException("Could not set the device format.");
            }
        }

        /// <summary>
        /// Gets or sets the capture frame size.
        /// </summary>
        /// <remarks>
        /// The getter and setter may cause side effects.
        /// </remarks>
        public Size FrameSize
        {
            get
            {
                var format = VideoFormat;
                return new Size(format.biWidth, format.biHeight);
            }

            set
            {
                var format = VideoFormat;
                format.biWidth = value.Width;
                format.biHeight = value.Height;
                VideoFormat = format;
            }
        }

        public bool HasDlgVideoDisplay
        {
            get
            {
                return hasDlgVideoDisplay;
            }
        }

        public bool HasDlgVideoFormat
        {
            get
            {
                return hasDlgVideoFormat;
            }
        }

        public bool HasDlgVideoSource
        {
            get
            {
                return hasDlgVideoSource;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            // Destroy preview window
            DestroyPreviewWindow();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Shows the video display dialog.
        /// </summary>
        /// <returns>true if the dialog was successfully shown; otherwise, false.</returns>
        public bool ShowVideoDisplayDialog()
        {
            return Win32.SendMessage(camWindowHandle, Win32.WM_CAP_DLG_VIDEODISPLAY, 0, 0) != 0;
        }

        /// <summary>
        /// Shows the video format dialog.
        /// </summary>
        /// <returns>true if the dialog was successfully shown; otherwise, false.</returns>
        public bool ShowVideoFormatDialog()
        {
            if (Win32.SendMessage(camWindowHandle, Win32.WM_CAP_DLG_VIDEOFORMAT, 0, 0) == 0)
                return false;

            // TODO: Check and update internal format (ex: I420)

            return true;
        }

        /// <summary>
        /// Shows the video source dialog.
        /// </summary>
        /// <returns>true if the dialog was successfully shown; otherwise, false.</returns>
        public bool ShowVideoSourceDialog()
        {
            return Win32.SendMessage(camWindowHandle, Win32.WM_CAP_DLG_VIDEOSOURCE, 0, 0) != 0;
        }

        /// <summary>
        /// Grabs a single frame and returns it as an image.
        /// </summary>
        public Image GrabFrame()
        {
            frameImage = null;

            // Grab image
            localGrab = true;
            if (Win32.SendMessage(camWindowHandle, Win32.WM_CAP_GRAB_FRAME_NOSTOP, 0, 0) == 0)
                return null;
            localGrab = false;

            // Return image and clear internal image
            var res = frameImage;
            frameImage = null;
            return res;
        }

        /// <summary>
        /// Sets the preview callback (set to null to disable preview mode).
        /// </summary>
        public void SetPreviewCallback(CamPreviewCallback previewCallback)
        {
            previewHandler = previewCallback;
            previewTimer.Enabled = (previewHandler != null);
        }

        #endregion

        /// <summary>
        /// Creates the preview window.
        /// </summary>
        IntPtr CreatePreviewWindow()
        {
            // TODO: CapDlgVideoDisplay (?)
            return Win32.CapCreateCaptureWindow("WebcamLib Window", 0x00000000, 0, 0, 320, 240, owner.Handle.ToInt32(), 0);
        }

        /// <summary>
        /// Sets the preview window.
        /// </summary>
        void SetPreviewWindow(IntPtr windowHandle)
        {
            // Set capture window
            camWindowHandle = windowHandle;

            var success = false;
            try
            {
                // Connect to the capture device
                if (Win32.SendMessage(camWindowHandle, Win32.WM_CAP_DRIVER_CONNECT, 0, 0) == 0)
                    throw new CamException("Could not connect to device.");
                Win32.SendMessage(camWindowHandle, Win32.WM_CAP_SET_PREVIEW, 0, 0);
                Win32.SendMessage(camWindowHandle, Win32.WM_CAP_SET_SCALE, 0, 0);

                // Get device capabilities
                var caps = new Win32.CapDriverCaps();
                Win32.SendMessage(camWindowHandle, Win32.WM_CAP_DRIVER_GET_CAPS, Marshal.SizeOf(caps), ref caps);
                hasDlgVideoDisplay = caps.fHasDlgVideoDisplay != 0;
                hasDlgVideoFormat = caps.fHasDlgVideoFormat != 0;
                hasDlgVideoSource = caps.fHasDlgVideoSource != 0;

                // TODO: Uncomment this to test which video formats are available -- remove this when finished testing
                //ShowVideoFormatDialog();

                // TODO: Get a list of supported video formats and throw an exception if none of them are supported

                // Set desired video format
                var format = VideoFormat;
                format.biCompression = 0;
                format.biBitCount = 24;
                VideoFormat = format;

                // Set callbacks
                frameCallback = FrameCallbackProc;
                if (Win32.SendMessage(camWindowHandle, Win32.WM_CAP_SET_CALLBACK_FRAME, 0, frameCallback) == 0)
                    throw new CamException("Could not set internal device callback.");

                success = true;
            }
            finally
            {
                if (!success)
                    Dispose();
            }
        }

        /// <summary>
        /// Destroys the preview window.
        /// </summary>
        void DestroyPreviewWindow()
        {
            // Clean up capture window
            if (camWindowHandle != IntPtr.Zero)
            {
                Win32.SendMessage(camWindowHandle, Win32.WM_CAP_DRIVER_DISCONNECT, 0, 0);
                Win32.DestroyWindow(camWindowHandle);
                camWindowHandle = IntPtr.Zero;
            }

            frameCallback = null;
        }

        /// <summary>
        /// Called on each frame update.
        /// </summary>
        /// <remarks>
        /// In order to draw on the resulting image, it must first be copied. This is because setting the bitmap data
        /// pointer directly and rotating the image can lead to random exceptions. This is expensive though. So by
        /// design, this doesn't happen automatically.
        /// 
        /// See the following sites for details about this:
        /// http://msdn.microsoft.com/en-us/library/system.drawing.image.rotateflip.aspx
        /// http://social.msdn.microsoft.com/Forums/en-IE/csharplanguage/thread/affa1855-e1ec-476d-bfb1-d0985971f394
        /// </remarks>
        int FrameCallbackProc(IntPtr hWnd, ref Win32.VideoHeader VideoHeader)
        {
            // Failsafe
            if (!localGrab)
                return 1;

            // Failsafe .. return False if bad
            if (VideoHeader.lpData == IntPtr.Zero)
                return 0;

            // Get image size and dimensions
            var format = VideoFormat;
            var cbw = format.biWidth * 3;
            var area = (cbw * Math.Abs(format.biHeight));

            // TODO: Handle decompression of different video formats

            // Create normal bitmap
            Image img;
            try
            {
                img = new Bitmap(format.biWidth, Math.Abs(format.biHeight), (((VideoHeader.dwBytesUsed > 0) ? (VideoHeader.dwBytesUsed - area) : (0)) + cbw), PixelFormat.Format24bppRgb, VideoHeader.lpData);
            }
            catch (NullReferenceException)
            {
                return 0;
            }

            // Rotate the image, if necessary
            if (format.biHeight > 0)
            {
                Bitmap bitmap = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                    g.DrawImage(img, 0, 0, new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                img = bitmap;
            }

            // Set image
            frameImage = img;

            return 1; // True
        }

        /// <summary>
        /// Grab the next frame.
        /// </summary>
        void PreviewTick(object sender, EventArgs e)
        {
            // Failsafe
            if (previewHandler == null)
            {
                previewTimer.Enabled = false;
                return;
            }

            // Call preview proc
            previewHandler(GrabFrame());
        }

        readonly IWin32Window owner;
        IntPtr camWindowHandle = IntPtr.Zero;
        bool hasDlgVideoDisplay;
        bool hasDlgVideoFormat;
        bool hasDlgVideoSource;
        bool localGrab;
        Image frameImage;
        // Required to bypass a GC
        Win32.FrameCallback frameCallback;
        CamPreviewCallback previewHandler;
        Timer previewTimer;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            
        }
    }
}

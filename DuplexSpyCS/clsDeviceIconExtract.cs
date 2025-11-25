using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class clsDeviceIconExtract
    {
        // GUID structure for Windows (SetupAPI)
        [StructLayout(LayoutKind.Sequential)]
        public struct GUID
        {
            public int Data1;
            public short Data2;
            public short Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;

            // Convert .NET Guid to Windows GUID structure
            public GUID(Guid guid)
            {
                byte[] guidBytes = guid.ToByteArray();
                Data1 = BitConverter.ToInt32(guidBytes, 0);
                Data2 = BitConverter.ToInt16(guidBytes, 4);
                Data3 = BitConverter.ToInt16(guidBytes, 6);
                Data4 = new byte[8];
                Array.Copy(guidBytes, 8, Data4, 0, 8);
            }
        }

        // SP_CLASSIMAGELIST_DATA structure
        [StructLayout(LayoutKind.Sequential)]
        public struct SP_CLASSIMAGELIST_DATA
        {
            public uint cbSize;
            public IntPtr ImageList;
            public int Reserved;
        }

        // P/Invoke declarations for SetupAPI
        [DllImport("Setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetClassImageList(ref SP_CLASSIMAGELIST_DATA classImageListData);

        [DllImport("Setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetClassImageIndex(ref SP_CLASSIMAGELIST_DATA classImageListData, ref GUID classGuid, out int imageIndex);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        // Constants
        public const int IMAGE_ICON = 0x00000001;
        public const int LR_SHARED = 0x00008000;

        public static Icon GetDeviceClassIcon(string guid)
        {
            if (guid == "X")
                return null;

            Guid classGuid = new Guid(guid);
            GUID classGuidWin = new GUID(classGuid); // Convert to Windows GUID structure

            // Initialize class image list data structure
            SP_CLASSIMAGELIST_DATA classImageListData = new SP_CLASSIMAGELIST_DATA();
            classImageListData.cbSize = (uint)Marshal.SizeOf(classImageListData);

            try
            {
                // Get the class image list
                if (!SetupDiGetClassImageList(ref classImageListData))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                // Get the image index for the device class
                if (SetupDiGetClassImageIndex(ref classImageListData, ref classGuidWin, out int imageIndex))
                {
                    // Extract the icon from the image list
                    IntPtr hIcon = ImageList_GetIcon(classImageListData.ImageList, imageIndex, IMAGE_ICON);

                    if (hIcon != IntPtr.Zero)
                    {
                        Icon icon = Icon.FromHandle(hIcon);
                        return icon;
                    }
                }
            }
            finally
            {
                // Clean up the class image list
                ImageList_Destroy(classImageListData.ImageList);
            }

            return null;
        }

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern bool ImageList_Destroy(IntPtr himl);
    }
}

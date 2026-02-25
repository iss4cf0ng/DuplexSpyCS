using System.Runtime.InteropServices;

namespace wallpaper
{
    internal class Program
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static void Main(string[] args)
        {
            string szImgFile = @"C:\users\user\desktop\eye.jpg";
            bool bResult = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, szImgFile, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

            if (bResult)
                Console.WriteLine("OK");
            else
                Console.WriteLine("NO");
        }
    }
}

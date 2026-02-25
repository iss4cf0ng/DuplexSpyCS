using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.IO;

[DllImport("user32.dll")]
static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);

[DllImport("user32.dll")]
static extern IntPtr LoadImage(IntPtr hInstance, IntPtr hIcon, uint uType, int cx, int cy, uint fuLoad);

[DllImport("user32.dll", SetLastError = true)]
static extern IntPtr GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

// Constants
const int ICON_SMALL = 0;
const int ICON_BIG = 1;
const uint LR_LOADFROMFILE = 0x00000010;

// Function to list all open windows with a valid MainWindowHandle
static void ListAllWindowsWithMainWindowHandle()
{
    EnumWindows((hWnd, lParam) =>
    {
        // Checking if the window is visible
        if (IsWindowVisible(hWnd))
        {
            StringBuilder windowTitle = new StringBuilder(256);
            GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

            // Check if window has a valid MainWindowHandle (hWnd is the handle of the window)
            if (windowTitle.Length > 0)
            {
                // Get the process that owns this window handle
                int processId;
                GetWindowThreadProcessId(hWnd, out processId);
                var process = Process.GetProcessById(processId);

                // Load the icon associated with the window
                Icon windowIcon = GetWindowIcon(hWnd);
                if (windowIcon != null)
                {
                    Console.WriteLine($"Window Handle: {hWnd}, Process: {process.ProcessName}, Title: {windowTitle}");
                    Console.WriteLine("Icon Found, saving as window_icon.ico");
                    // Save the icon as an .ico file (or process as needed)
                    SaveIconAsFile(windowIcon, "window_icon.ico");
                }
                else
                {
                    Console.WriteLine($"Window Handle: {hWnd}, Process: {process.ProcessName}, Title: {windowTitle} (No Icon Found)");
                }

                Console.WriteLine("------");
            }
        }
        return true; // Continue enumeration
    }, IntPtr.Zero);
}

static IntPtr GetWindowIconHandle(IntPtr hWnd)
{
    // Retrieve the icon associated with the window using the window handle
    IntPtr iconHandle = GetClassLongPtr(hWnd, ICON_BIG); // Change ICON_BIG to ICON_SMALL for small icons

    if (iconHandle != IntPtr.Zero)
    {
        return iconHandle;
    }

    return IntPtr.Zero;
}

static void SaveIconAsFile(Icon icon, string filePath)
{
    using (FileStream fs = new FileStream(filePath, FileMode.Create))
    {
        icon.Save(fs);
    }
}

static Icon GetWindowIcon(IntPtr hWnd)
{
    IntPtr hIcon = GetWindowIconHandle(hWnd);
    if (hIcon != IntPtr.Zero)
    {
        return Icon.FromHandle(hIcon);
    }
    return null;
}

// List all open windows
ListAllWindowsWithMainWindowHandle();

// Importing necessary user32.dll functions
[DllImport("user32.dll")]
static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

[DllImport("user32.dll")]
static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

[DllImport("user32.dll")]
static extern bool IsWindowVisible(IntPtr hWnd);

[DllImport("user32.dll")]
static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

// Delegate to handle window enumeration
public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
using HWND = System.IntPtr;
using System.Runtime.InteropServices;
using System.Text;

namespace cadwiki.NUnitTestRunner.WinAPI
{
    public class Stubs
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetWindowRect(HWND hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(HWND hWnd, HWND hdcBlt, int nFlags);



        [DllImport("user32.dll")]
        public static extern bool EnumWindows(Delegates.EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(HWND hWnd, StringBuilder title, int size);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(HWND hWnd);


        [DllImport("user32.dll")]
        public static extern HWND GetShellWindow();

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        public static extern HWND FindWindowEx(HWND hwndParent, HWND hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        public static extern int SendMessage(HWND hWnd, int Msg, int wParam, HWND lParam);

    }
}
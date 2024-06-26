using HWND = System.IntPtr;

namespace cadwiki.NUnitTestRunner.WinAPI
{
    public class Delegates
    {
        public delegate bool EnumWindowsProc(HWND hWnd, int lParam);

    }
}
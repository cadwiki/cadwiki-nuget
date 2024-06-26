using System.Collections.Generic;
using HWND = System.IntPtr;
using System.Text;

namespace cadwiki.NUnitTestRunner.WinAPI
{
    public class ExtensionMethods
    {

        public static HWND GetOpenWindow(string title)
        {
            Dictionary<string, HWND> titleToHandle = (Dictionary<string, HWND>)GetOpenWindows();
            HWND windowHandle;
            titleToHandle.TryGetValue(title, out windowHandle);
            return windowHandle;
        }

        public static IDictionary<string, HWND> GetOpenWindows()
        {
            var shellWindow = Stubs.GetShellWindow();
            var windows = new Dictionary<string, HWND>();
            Stubs.EnumWindows((hWnd, lParam) =>
            {
                if (hWnd.Equals(shellWindow))
                    return true;
                if (!Stubs.IsWindowVisible(hWnd))
                    return true;
                int length = Stubs.GetWindowTextLength(hWnd);
                if (length == 0)
                    return true;

                var stringBuilder = new StringBuilder(length);
                Stubs.GetWindowText(hWnd, stringBuilder, length + 1);

                windows[stringBuilder.ToString()] = hWnd;
                return true;
            }, 0);
            return windows;
        }

        public static object FindSubControl(HWND parentHwnd, string controlClass, string conrolDisplayText)
        {
            return Stubs.FindWindowEx(parentHwnd, HWND.Zero, controlClass, conrolDisplayText);
        }

        public static void CloseWindow(HWND hWnd)
        {
            Stubs.SendMessage(hWnd, Constants.WM_SYSCOMMAND, Constants.SC_CLOSE, (HWND)0);
        }
    }
}
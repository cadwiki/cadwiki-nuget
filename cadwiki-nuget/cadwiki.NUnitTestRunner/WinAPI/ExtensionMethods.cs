using System.Collections.Generic;
using HWND = System.IntPtr;
using System.Text;
using System.Linq;
using System;

namespace cadwiki.NUnitTestRunner.WinAPI
{
    public class ExtensionMethods
    {

        public static HWND GetOpenWindow(string title)
        {
            Dictionary<string, HWND> titleToHandle = (Dictionary<string, HWND>)GetOpenWindows();
            HWND windowHandle = DictionaryExtensions.GetValuesByKeyContains(titleToHandle, title).FirstOrDefault();
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

    public static class DictionaryExtensions
    {
        public static List<TValue> GetValuesByKeyContains<TKey, TValue>(Dictionary<TKey, TValue> dictionary, string searchString)
        {
            return dictionary
                .Where(entry => entry.Key.ToString().Contains(searchString))
                .Select(entry => entry.Value)
                .ToList();
        }
    }
}
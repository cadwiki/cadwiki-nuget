using System;
using System.Text;

namespace cadwiki.AC.Utilities
{
    /// <summary>
    /// A singleton debug logger service that can output to both Debug and a UI component
    /// </summary>
    public class DebugLogger
    {
        private static DebugLogger _instance;
        private static readonly object _lock = new object();
        private StringBuilder _logBuilder;
        private int _maxLines = 1000;

        public event Action<string> LogAdded;

        public static DebugLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DebugLogger();
                        }
                    }
                }
                return _instance;
            }
        }

        private DebugLogger()
        {
            _logBuilder = new StringBuilder();
        }

        /// <summary>
        /// Logs a message to the debug output and raises an event for UI updates
        /// </summary>
        public void Log(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            
            // Still write to Debug for Visual Studio output window
            System.Diagnostics.Debug.WriteLine(message);
            
            // Add to internal log
            _logBuilder.AppendLine(timestampedMessage);
            
            // Trim old entries if we exceed max lines
            TrimLogIfNeeded();
            
            // Raise event for UI subscribers
            LogAdded?.Invoke(timestampedMessage);
        }

        /// <summary>
        /// Logs a formatted message
        /// </summary>
        public void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        public void LogError(string message, Exception ex = null)
        {
            string fullMessage = ex != null 
                ? $"ERROR: {message} - {ex.Message}\n{ex.StackTrace}" 
                : $"ERROR: {message}";
            Log(fullMessage);
        }

        public void LogError( Exception ex = null)
        {
            string fullMessage = ex != null
                ? $"ERROR: {ex.Message}\n{ex.StackTrace}"
                : $"ERROR: {ex.Message}";
            Log(fullMessage);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            Log($"WARNING: {message}");
        }

        /// <summary>
        /// Gets the full log content
        /// </summary>
        public string GetLogContent()
        {
            return _logBuilder.ToString();
        }

        /// <summary>
        /// Clears the log
        /// </summary>
        public void Clear()
        {
            _logBuilder.Clear();
            LogAdded?.Invoke("--- Log Cleared ---");
        }

        private void TrimLogIfNeeded()
        {
            // Simple trimming - remove first portion if too large
            if (_logBuilder.Length > 100000)
            {
                string content = _logBuilder.ToString();
                int newLineIndex = content.IndexOf('\n', 50000);
                if (newLineIndex > 0)
                {
                    _logBuilder.Remove(0, newLineIndex + 1);
                }
            }
        }
    }
}
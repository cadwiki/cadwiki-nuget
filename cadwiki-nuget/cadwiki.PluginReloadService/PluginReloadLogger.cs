using System;
using System.IO;

namespace cadwiki.PluginReloadService
{
    /// <summary>
    /// Structured diagnostic logger for the Plugin Reload Service.
    /// Writes log lines in the format:
    ///   [HH:mm:ss.fff] [LEVEL] [Component] Message
    /// to a file on disk and optionally to a secondary TextWriter.
    ///
    /// Log files are placed at:
    ///   %TEMP%\cadwiki.PluginStaging\{PluginName}\{timestamp}\_reload.log
    /// </summary>
    public class PluginReloadLogger
    {
        // -------------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------------
        private readonly string _logFilePath;
        private readonly bool _verbose;
        private readonly object _lock = new object();

        // -------------------------------------------------------------------------
        // Construction
        // -------------------------------------------------------------------------

        /// <summary>
        /// Creates a new logger that writes to the specified file.
        /// </summary>
        /// <param name="logFilePath">
        ///   Full path of the log file. The parent directory must exist.
        /// </param>
        /// <param name="verbose">
        ///   When true, DEBUG-level messages are written in addition to INFO/WARN/ERROR.
        /// </param>
        public PluginReloadLogger(string logFilePath, bool verbose = false)
        {
            _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
            _verbose = verbose;

            try
            {
                // Ensure directory exists
                string dir = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                WriteRaw($"=== cadwiki.PluginReloadService log started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                WriteRaw($"=== Verbose mode: {verbose} ===");
                WriteRaw(string.Empty);
            }
            catch
            {
                // best-effort; if we can't create the log file the service still runs
            }
        }

        // -------------------------------------------------------------------------
        // Public log methods
        // -------------------------------------------------------------------------

        /// <summary>Writes an INFO-level message.</summary>
        public void Info(string component, string message)
            => Write("INFO ", component, message);

        /// <summary>Writes a WARN-level message.</summary>
        public void Warn(string component, string message)
            => Write("WARN ", component, message);

        /// <summary>
        /// Writes an ERROR-level message with a remediation hint.
        /// </summary>
        /// <param name="component">Component name (e.g. "AssemblyVersionRewriter").</param>
        /// <param name="operation">Short description of what failed (e.g. "Rewrite").</param>
        /// <param name="message">Exception or error message.</param>
        /// <param name="context">File path or other context that helps identify the problem.</param>
        /// <param name="hint">Optional actionable suggestion for the developer.</param>
        public void Error(string component, string operation, string message,
                          string context = null, string hint = null)
        {
            string line = $"[ERROR] {operation} FAILED: {message}";
            if (!string.IsNullOrEmpty(context))
                line += $" at {context}";
            if (!string.IsNullOrEmpty(hint))
                line += $". {hint}";
            Write("ERROR", component, line);
        }

        /// <summary>
        /// Writes a DEBUG-level message. Only written when verbose mode is enabled.
        /// </summary>
        public void Debug(string component, string message)
        {
            if (_verbose)
                Write("DEBUG", component, message);
        }

        /// <summary>Logs an exception with full context.</summary>
        public void Exception(string component, string operation, Exception ex,
                              string context = null)
        {
            Write("ERROR", component,
                $"[ERROR] {operation} FAILED: {ex.Message}" +
                (context != null ? $" at {context}" : "") +
                $"{Environment.NewLine}  StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
                Write("ERROR", component,
                    $"  InnerException: {ex.InnerException.Message}");
        }

        /// <summary>Writes a blank separator line to improve readability.</summary>
        public void Separator()
            => WriteRaw("─────────────────────────────────────────────────────────────────");

        /// <summary>
        /// Writes a summary footer block (timing, stats).
        /// </summary>
        public void WriteSummary(string pluginName, TimeSpan elapsed,
                                 int dllCount, int rewrittenCount, string stagingFolder)
        {
            Separator();
            WriteRaw($"[{DateTime.Now:HH:mm:ss.fff}] SUMMARY");
            WriteRaw($"  Plugin     : {pluginName}");
            WriteRaw($"  Elapsed    : {elapsed.TotalMilliseconds:0}ms");
            WriteRaw($"  DLLs total : {dllCount}");
            WriteRaw($"  Rewritten  : {rewrittenCount}");
            WriteRaw($"  Staged at  : {stagingFolder}");
            Separator();
        }

        /// <summary>Returns the full path of the log file for display in UI.</summary>
        public string LogFilePath => _logFilePath;

        // -------------------------------------------------------------------------
        // Internal helpers
        // -------------------------------------------------------------------------

        private void Write(string level, string component, string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] [{component}] {message}";
            WriteRaw(line);
        }

        private void WriteRaw(string line)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, line + Environment.NewLine,
                        System.Text.Encoding.UTF8);
                }
            }
            catch
            {
                // best-effort; never throw from logger
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace CadDevToolsDriver
{
    /// <summary>
    /// Immutable configuration for <see cref="Launcher"/>. Decouples the
    /// launcher from any specific plugin — callers supply their own DLL
    /// names, search paths, and AutoCAD settings.
    ///
    /// <para><b>Example — minimal config:</b></para>
    /// <code>
    ///   var config = new LauncherConfig.Builder("MyPlugin")
    ///       .AddDllPattern("*MyPlugin.dll")
    ///       .Build();
    ///   var deps = Launcher.GetDependencies(config);
    /// </code>
    ///
    /// <para><b>Example — full config:</b></para>
    /// <code>
    ///   var config = new LauncherConfig.Builder("MyPlugin")
    ///       .AddDllPattern("*MyPlugin.dll")
    ///       .AddDllPattern("*MyPlugin.Core.dll")
    ///       .AutoCADExePath(@"C:\Program Files\Autodesk\AutoCAD 2025\acad.exe")
    ///       .StartupSwitches("/p VANILLA")
    ///       .TempSubfolder("MyPlugin.Staging")
    ///       .StaleFolderDays(2)
    ///       .Build();
    /// </code>
    /// </summary>
    public class LauncherConfig
    {
        /// <summary>Plugin name (used for temp folder naming).</summary>
        public string PluginName { get; }

        /// <summary>
        /// Wildcard patterns for DLLs to netload (e.g. "*MyPlugin.dll").
        /// The launcher searches for the newest match in the staging folder.
        /// </summary>
        public IReadOnlyList<string> DllPatterns { get; }

        /// <summary>Path to acad.exe. Default: AutoCAD 2025 standard path.</summary>
        public string AutoCADExePath { get; }

        /// <summary>AutoCAD startup switches. Default: "/p VANILLA".</summary>
        public string StartupSwitches { get; }

        /// <summary>
        /// Temp subfolder name under %TEMP%. Default: "cadwiki.{PluginName}".
        /// </summary>
        public string TempSubfolder { get; }

        /// <summary>
        /// Number of days after which stale staging folders are deleted.
        /// Default: 1.
        /// </summary>
        public double StaleFolderDays { get; }

        /// <summary>Whether to set AutoCAD window to normal after launch.</summary>
        public bool SetAutocadWindowToNorm { get; }

        private LauncherConfig(
            string pluginName,
            IReadOnlyList<string> dllPatterns,
            string autoCADExePath,
            string startupSwitches,
            string tempSubfolder,
            double staleFolderDays,
            bool setAutocadWindowToNorm)
        {
            PluginName = pluginName;
            DllPatterns = dllPatterns;
            AutoCADExePath = autoCADExePath;
            StartupSwitches = startupSwitches;
            TempSubfolder = tempSubfolder;
            StaleFolderDays = staleFolderDays;
            SetAutocadWindowToNorm = setAutocadWindowToNorm;
        }

        // ── Fluent Builder ──────────────────────────────────────────────────

        /// <summary>Fluent builder for <see cref="LauncherConfig"/>.</summary>
        public class Builder
        {
            private readonly string _pluginName;
            private readonly List<string> _dllPatterns = new List<string>();
            private string _autoCADExePath = @"C:\Program Files\Autodesk\AutoCAD 2025\acad.exe";
            private string _startupSwitches = "/p VANILLA";
            private string _tempSubfolder;
            private double _staleFolderDays = 1.0;
            private bool _setAutocadWindowToNorm;

            /// <summary>Create a builder for the given plugin name.</summary>
            public Builder(string pluginName)
            {
                _pluginName = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
                _tempSubfolder = "cadwiki." + pluginName;
            }

            /// <summary>Add a wildcard DLL pattern to search for.</summary>
            public Builder AddDllPattern(string pattern) { _dllPatterns.Add(pattern); return this; }

            /// <summary>Set the AutoCAD executable path.</summary>
            public Builder AutoCADExePath(string path) { _autoCADExePath = path; return this; }

            /// <summary>Set AutoCAD startup switches.</summary>
            public Builder StartupSwitches(string switches) { _startupSwitches = switches; return this; }

            /// <summary>Set the temp subfolder name.</summary>
            public Builder TempSubfolder(string subfolder) { _tempSubfolder = subfolder; return this; }

            /// <summary>Set stale folder cleanup threshold in days.</summary>
            public Builder StaleFolderDays(double days) { _staleFolderDays = days; return this; }

            /// <summary>Set whether to normalize the AutoCAD window.</summary>
            public Builder SetAutocadWindowToNorm(bool value) { _setAutocadWindowToNorm = value; return this; }

            /// <summary>Build the immutable config.</summary>
            public LauncherConfig Build()
            {
                return new LauncherConfig(
                    _pluginName,
                    _dllPatterns.AsReadOnly(),
                    _autoCADExePath,
                    _startupSwitches,
                    _tempSubfolder,
                    _staleFolderDays,
                    _setAutocadWindowToNorm);
            }
        }
    }
}

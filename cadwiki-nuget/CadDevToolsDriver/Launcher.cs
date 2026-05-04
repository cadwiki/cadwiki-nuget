using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CadDevToolsDriver
{
    /// <summary>
    /// Resolves DLL paths and builds <see cref="cadwiki.CadDevTools.MainWindow.Dependencies"/>
    /// for the CadDevTools UI launcher.
    ///
    /// <para><b>Two usage modes:</b></para>
    /// <list type="bullet">
    ///   <item><b>Legacy (zero-arg)</b> — <see cref="GetDependencies()"/> uses hardcoded
    ///         TestPlugin defaults. Preserved for backward compatibility.</item>
    ///   <item><b>Configurable</b> — <see cref="GetDependencies(LauncherConfig)"/> accepts
    ///         a <see cref="LauncherConfig"/> so any addin can reuse the launcher without
    ///         modifying this class.</item>
    /// </list>
    ///
    /// <para><b>Example — reuse in another addin:</b></para>
    /// <code>
    ///   var config = new LauncherConfig.Builder("MyAddin")
    ///       .AddDllPattern("*MyAddin.dll")
    ///       .AddDllPattern("*MyAddin.Core.dll")
    ///       .AutoCADExePath(@"C:\Program Files\Autodesk\AutoCAD 2025\acad.exe")
    ///       .Build();
    ///   var deps = Launcher.GetDependencies(config);
    ///   var window = new cadwiki.CadDevTools.MainWindow(deps);
    /// </code>
    /// </summary>
    public class Launcher
    {
        // ── Configurable API ────────────────────────────────────────────────

        /// <summary>
        /// Builds dependencies from a <see cref="LauncherConfig"/>.
        /// Searches the temp staging folder and the exe directory for DLLs
        /// matching the configured patterns, cleans up stale folders, and
        /// returns a ready-to-use Dependencies object.
        /// </summary>
        /// <param name="config">Launcher configuration for the target plugin.</param>
        /// <returns>Dependencies configured for the CadDevTools MainWindow.</returns>
        public static cadwiki.CadDevTools.MainWindow.Dependencies GetDependencies(LauncherConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exePath);
            string tempDir = Path.Combine(Path.GetTempPath(), config.TempSubfolder);

            // Clean up stale staging folders
            if (Directory.Exists(tempDir))
            {
                var stagingFolders = Directory.GetDirectories(tempDir)
                    .OrderByDescending(f => new DirectoryInfo(f).CreationTime)
                    .ToList();
                DeleteFoldersOlderThan(stagingFolders, config.StaleFolderDays);
            }

            // Resolve DLL paths from all configured patterns
            var resolvedPaths = new List<string>();
            foreach (var pattern in config.DllPatterns)
            {
                string wildcard = pattern.StartsWith("*") ? pattern : "*" + pattern;

                // Search temp staging dir first, then exe dir
                string found = TryResolve(tempDir, wildcard)
                            ?? TryResolve(exeDir, wildcard);

                if (!string.IsNullOrEmpty(found))
                {
                    resolvedPaths.Add(found);
                }
            }

            var dependencies = new cadwiki.CadDevTools.MainWindow.Dependencies();
            dependencies.AutoCADExePath = config.AutoCADExePath;
            dependencies.AutoCADStartupSwitches = config.StartupSwitches;
            dependencies.DllFilePathsToNetloadCommaDelimited = string.Join(",", resolvedPaths);
            dependencies.CustomDirectoryToSearchForDllsToLoadFrom = exeDir;
            dependencies.SetAutocadWindowToNorm = config.SetAutocadWindowToNorm;

            // Use the first pattern as the default search pattern
            if (config.DllPatterns.Count > 0)
            {
                dependencies.DllWildCardSearchPattern = config.DllPatterns[0];
            }

            return dependencies;
        }

        // ── Legacy API (backward compatible) ────────────────────────────────

        /// <summary>
        /// Legacy entry point — uses hardcoded TestPlugin defaults.
        /// Prefer <see cref="GetDependencies(LauncherConfig)"/> for new code.
        /// </summary>
        public static cadwiki.CadDevTools.MainWindow.Dependencies GetDependencies()
        {
            var config = new LauncherConfig.Builder("TestPlugin")
                .AddDllPattern("*cadwiki.AC.TestPlugin.dll")
                .AddDllPattern("*cadwiki.AC.dll")
                .AutoCADExePath(@"C:\Program Files\Autodesk\AutoCAD 2025\acad.exe")
                .StartupSwitches("/p VANILLA")
                .TempSubfolder("cadwiki.TestPlugin")
                .Build();

            return GetDependencies(config);
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static string TryResolve(string searchDir, string wildcardFileName)
        {
            if (string.IsNullOrEmpty(searchDir) || !Directory.Exists(searchDir))
                return null;

            return cadwiki.NetUtils.Paths.GetNewestDllInAnySubfolderOfSolutionDirectory(
                searchDir, wildcardFileName);
        }

        private static void DeleteFoldersOlderThan(List<string> folders, double days)
        {
            var dateNow = DateTime.Now;
            foreach (var folder in folders)
            {
                var dateCreated = new DirectoryInfo(folder).CreationTime;
                if (dateNow.Subtract(dateCreated).TotalDays > days)
                {
                    try
                    {
                        Directory.Delete(folder, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace cadwiki.PluginReloadService
{
    /// <summary>
    /// Copies a plugin's build output into a uniquely-named staging folder,
    /// optionally rewriting assembly versions on managed DLLs via
    /// <see cref="AssemblyVersionRewriter"/>.
    ///
    /// <para><b>Staging folder naming convention:</b></para>
    /// <code>
    ///   %TEMP%\cadwiki.PluginStaging\{PluginName}\{yyyyMMdd--HH_mm_ss}--Build-{n}\
    /// </code>
    ///
    /// <para><b>Retention policy:</b> only the most recent 5 staging folders per
    /// plugin are kept. Older folders are deleted automatically after each run.</para>
    /// </summary>
    public class StagingCopier
    {
        // -------------------------------------------------------------------------
        // Constants
        // -------------------------------------------------------------------------

        /// <summary>Root folder under %TEMP% for all plugin staging.</summary>
        public const string StagingRootName = "cadwiki.PluginStaging";

        /// <summary>Maximum number of staging folders retained per plugin.</summary>
        public const int MaxStagingFoldersToRetain = 5;

        // -------------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------------

        private readonly string _pluginName;
        private readonly string _sourceBuildDir;
        private readonly DllFilterSpec _filterSpec;
        private readonly PluginReloadLogger _logger;

        // -------------------------------------------------------------------------
        // Construction
        // -------------------------------------------------------------------------

        /// <summary>
        /// Creates a new <see cref="StagingCopier"/>.
        /// </summary>
        /// <param name="pluginName">
        ///   Logical plugin name, used for the staging sub-directory.
        ///   Should match the main DLL name without the <c>.dll</c> extension.
        /// </param>
        /// <param name="sourceBuildDir">
        ///   Path to the plugin's build output directory (the folder that
        ///   contains the DLLs to be staged).
        /// </param>
        /// <param name="filterSpec">
        ///   Filter spec that decides which DLLs get version-rewritten.
        ///   Pass <c>null</c> to use a default "rewrite all non-SDK DLLs" spec.
        /// </param>
        /// <param name="logger">Optional structured logger.</param>
        public StagingCopier(
            string pluginName,
            string sourceBuildDir,
            DllFilterSpec filterSpec = null,
            PluginReloadLogger logger = null)
        {
            _pluginName     = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
            _sourceBuildDir = sourceBuildDir ?? throw new ArgumentNullException(nameof(sourceBuildDir));
            _filterSpec     = filterSpec ?? new DllFilterSpec { MainDllName = pluginName + ".dll" };
            _logger         = logger;
        }

        // -------------------------------------------------------------------------
        // Public API
        // -------------------------------------------------------------------------

        /// <summary>
        /// Performs the staging operation:
        /// <list type="number">
        ///   <item>Creates a new dated staging folder.</item>
        ///   <item>Scans the source build directory for DLLs and other files.</item>
        ///   <item>For each DLL: checks managed/native status, applies filter rules,
        ///     then either version-rewrites (managed) or copies verbatim.</item>
        ///   <item>Copies any PDB alongside its rewritten DLL.</item>
        ///   <item>Copies all non-DLL files verbatim.</item>
        ///   <item>Writes <c>_manifest.json</c>.</item>
        ///   <item>Deletes old staging folders (retains last N).</item>
        /// </list>
        /// </summary>
        /// <param name="buildTime">
        ///   Timestamp to encode into rewritten assemblies.
        ///   Defaults to <c>DateTime.Now</c>.
        /// </param>
        /// <returns>
        ///   A <see cref="StagingResult"/> describing the outcome.
        /// </returns>
        public StagingResult StagePlugin(DateTime? buildTime = null)
        {
            const string Component = "StagingCopier";
            var time = buildTime ?? DateTime.Now;

            _logger?.Separator();
            _logger?.Info(Component, $"StagePlugin STARTED — plugin: {_pluginName}");
            _logger?.Info(Component, $"Source dir: {_sourceBuildDir}");
            _logger?.Info(Component, $"Filter mode: {_filterSpec.DescribeMode()}");
            if (_filterSpec.IncludePatterns?.Count > 0)
                _logger?.Info(Component,
                    $"Include patterns: {string.Join(", ", _filterSpec.IncludePatterns)}");
            if (_filterSpec.ExcludePatterns?.Count > 0)
                _logger?.Info(Component,
                    $"Exclude patterns: {string.Join(", ", _filterSpec.ExcludePatterns)}");

            if (!Directory.Exists(_sourceBuildDir))
            {
                string msg = $"Source build directory not found: {_sourceBuildDir}. " +
                             "Ensure the plugin project was built before triggering reload.";
                _logger?.Error(Component, "StagePlugin", msg, _sourceBuildDir,
                    "Build the plugin project in Visual Studio, then retry.");
                throw new DirectoryNotFoundException(msg);
            }

            // -------- Create staging folder ----------------------------------------
            string stagingFolder = CreateStagingFolder(time, out int buildNumber);
            _logger?.Info(Component, $"Staging folder: {stagingFolder}");

            var manifest = new StagingManifest
            {
                PluginName      = _pluginName,
                SourceDirectory = _sourceBuildDir,
                StagingFolder   = stagingFolder,
                BuildTime       = time,
                BuildNumber     = buildNumber,
                FilterMode      = _filterSpec.DescribeMode(),
                IncludePatterns = new List<string>(_filterSpec.IncludePatterns ?? new List<string>()),
                ExcludePatterns = new List<string>(_filterSpec.ExcludePatterns ?? new List<string>()),
                ReloadLogPath   = _logger?.LogFilePath
            };

            var rewriteResults = new List<RewriteResult>();
            var errors         = new List<string>();

            // -------- Enumerate source files ----------------------------------------
            var allFiles = Directory.GetFiles(_sourceBuildDir)
                                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                                    .ToList();

            var dllFiles   = allFiles.Where(f => f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToList();
            var pdbFiles   = allFiles.Where(f => f.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)).ToList();
            var otherFiles = allFiles
                .Where(f => !f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                         && !f.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger?.Info(Component,
                $"Source scan: {dllFiles.Count} DLL(s), {pdbFiles.Count} PDB(s), " +
                $"{otherFiles.Count} other file(s)");

            // -------- Process DLLs --------------------------------------------------
            foreach (string dllPath in dllFiles)
            {
                string dllName = Path.GetFileName(dllPath);
                string destPath = Path.Combine(stagingFolder, dllName);

                bool isManaged = AssemblyVersionRewriter.IsManagedAssembly(dllPath);
                bool shouldRewrite = isManaged && _filterSpec.ShouldRewrite(dllName);
                string filterReason = _filterSpec.GetFilterReason(dllName);

                if (!isManaged)
                    filterReason = "native DLL — copied verbatim";

                _logger?.Debug(Component,
                    $"  {dllName}: managed={isManaged}, shouldRewrite={shouldRewrite}, reason={filterReason}");

                var entry = new DllManifestEntry
                {
                    FileName      = dllName,
                    WasRewritten  = shouldRewrite,
                    FilterReason  = filterReason
                };

                if (false && shouldRewrite)
                {
                    try
                    {
                        // Read original version for manifest
                        string originalVersionStr = GetAssemblyVersion(dllPath);
                        entry.OriginalVersion = originalVersionStr;

                        var result = AssemblyVersionRewriter.Rewrite(
                            dllPath, destPath, time, _logger);

                        entry.NewVersion     = result.NewVersion.ToString();
                        entry.DisplayVersion = result.DisplayVersion;
                        rewriteResults.Add(result);

                        _logger?.Info(Component,
                            $"{dllName} → REWRITTEN " +
                            $"({entry.OriginalVersion} → {entry.NewVersion}) {filterReason}");

                        // Preserve PDB alongside rewritten DLL
                        string pdbName    = Path.GetFileNameWithoutExtension(dllName) + ".pdb";
                        string pdbSrc     = Path.Combine(_sourceBuildDir, pdbName);
                        string pdbDest    = Path.Combine(stagingFolder, pdbName);
                        if (File.Exists(pdbSrc))
                        {
                            File.Copy(pdbSrc, pdbDest, overwrite: true);
                            entry.PdbPreserved = true;
                            _logger?.Debug(Component, $"  PDB preserved: {pdbName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        string errMsg = $"Failed to rewrite {dllName}: {ex.Message}";
                        errors.Add(errMsg);
                        _logger?.Exception(Component, $"Rewrite({dllName})", ex, dllPath);

                        // Fallback: copy verbatim so the staging folder is still usable
                        try
                        {
                            File.Copy(dllPath, destPath, overwrite: true);
                            entry.WasRewritten = false;
                            entry.FilterReason += " [FALLBACK: copied verbatim due to rewrite error]";
                        }
                        catch { /* best-effort */ }
                    }
                }
                else
                {
                    // Verbatim copy
                    entry.OriginalVersion = GetAssemblyVersionSafe(dllPath);
                    File.Copy(dllPath, destPath, overwrite: true);
                    _logger?.Info(Component, $"{dllName} → COPIED  ({filterReason})");

                    // Preserve matching PDB for verbatim copies too
                    string pdbName    = Path.GetFileNameWithoutExtension(dllName) + ".pdb";
                    string pdbSrc     = Path.Combine(_sourceBuildDir, pdbName);
                    string pdbDest    = Path.Combine(stagingFolder, pdbName);
                    if (File.Exists(pdbSrc))
                    {
                        File.Copy(pdbSrc, pdbDest, overwrite: true);
                        entry.PdbPreserved = true;
                    }
                }

                manifest.Dlls.Add(entry);
            }

            // -------- Copy other (non-DLL, non-PDB) files verbatim -----------------
            foreach (string filePath in otherFiles)
            {
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(stagingFolder, fileName);
                try
                {
                    File.Copy(filePath, destPath, overwrite: true);
                    manifest.OtherFiles.Add(fileName);
                    _logger?.Debug(Component, $"  Other file copied: {fileName}");
                }
                catch (Exception ex)
                {
                    _logger?.Exception(Component, $"CopyFile({fileName})", ex, filePath);
                }
            }

            // -------- Finalise manifest ---------------------------------------------
            manifest.DllCount      = dllFiles.Count;
            manifest.RewrittenCount = rewriteResults.Count;
            string manifestPath    = StagingManifestSerializer.Write(manifest, stagingFolder, _logger);

            _logger?.Info(Component,
                $"Staging complete: {dllFiles.Count} DLL(s), " +
                $"{rewriteResults.Count} rewritten, {otherFiles.Count} other file(s)");
            _logger?.Info(Component, $"Manifest: {manifestPath}");

            // -------- Cleanup old staging folders -----------------------------------
            int deleted = CleanupOldStagingFolders();
            if (deleted > 0)
                _logger?.Info(Component, $"Cleanup: deleted {deleted} old staging folder(s)");

            _logger?.Separator();

            return new StagingResult
            {
                PluginName      = _pluginName,
                StagingFolder   = stagingFolder,
                BuildTime       = time,
                BuildNumber     = buildNumber,
                DllCount        = dllFiles.Count,
                RewrittenCount  = rewriteResults.Count,
                RewriteResults  = rewriteResults,
                ManifestPath    = manifestPath,
                Errors          = errors,
                Manifest        = manifest
            };
        }

        // -------------------------------------------------------------------------
        // Folder management helpers
        // -------------------------------------------------------------------------

        /// <summary>
        /// Returns the staging root for the current plugin.
        /// </summary>
        public string GetPluginStagingRoot()
            => Path.Combine(Path.GetTempPath(), StagingRootName, _pluginName);

        private string CreateStagingFolder(DateTime time, out int buildNumber)
        {
            string root      = GetPluginStagingRoot();
            string timestamp = time.ToString("yyyyMMdd--HH_mm_ss");

            // Determine build number: scan existing folders, find max Build-N
            buildNumber = ComputeNextBuildNumber(root);

            string folderName = $"{timestamp}--Build-{buildNumber}";
            string path       = Path.Combine(root, folderName);

            // Handle rare collision (same second)
            int suffix = 1;
            while (Directory.Exists(path))
            {
                path = Path.Combine(root, $"{folderName}({suffix++})");
            }

            Directory.CreateDirectory(path);
            return path;
        }

        private static int ComputeNextBuildNumber(string root)
        {
            if (!Directory.Exists(root))
                return 1;

            int max = 0;
            foreach (string dir in Directory.GetDirectories(root))
            {
                string name = Path.GetFileName(dir);
                // Name format: yyyyMMdd--HH_mm_ss--Build-{n}
                int idx = name.IndexOf("--Build-", StringComparison.Ordinal);
                if (idx < 0) continue;
                string numPart = name.Substring(idx + "--Build-".Length);
                // Strip optional collision suffix like "(2)"
                int paren = numPart.IndexOf('(');
                if (paren >= 0) numPart = numPart.Substring(0, paren);
                if (int.TryParse(numPart, out int n) && n > max)
                    max = n;
            }
            return max + 1;
        }

        private int CleanupOldStagingFolders()
        {
            string root = GetPluginStagingRoot();
            if (!Directory.Exists(root))
                return 0;

            var dirs = Directory.GetDirectories(root)
                .Select(d => new DirectoryInfo(d))
                .OrderByDescending(d => d.CreationTime)
                .ToList();

            int deleted = 0;
            for (int i = MaxStagingFoldersToRetain; i < dirs.Count; i++)
            {
                try
                {
                    dirs[i].Delete(recursive: true);
                    deleted++;
                    _logger?.Debug("StagingCopier", $"Deleted old staging folder: {dirs[i].FullName}");
                }
                catch (Exception ex)
                {
                    _logger?.Debug("StagingCopier", $"Could not delete old folder {dirs[i].FullName}: {ex.Message}");
                }
            }
            return deleted;
        }

        // -------------------------------------------------------------------------
        // Version helpers
        // -------------------------------------------------------------------------

        private static string GetAssemblyVersion(string dllPath)
        {
            try
            {
                var name = AssemblyName.GetAssemblyName(dllPath);
                return name.Version?.ToString() ?? "0.0.0.0";
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        private static string GetAssemblyVersionSafe(string dllPath)
        {
            try { return GetAssemblyVersion(dllPath); }
            catch { return null; }
        }
    }

    // -------------------------------------------------------------------------
    // Result model
    // -------------------------------------------------------------------------

    /// <summary>
    /// Describes the outcome of a <see cref="StagingCopier.StagePlugin"/> call.
    /// </summary>
    public class StagingResult
    {
        /// <summary>Plugin name.</summary>
        public string PluginName { get; set; }

        /// <summary>Full path of the created staging folder.</summary>
        public string StagingFolder { get; set; }

        /// <summary>Build timestamp.</summary>
        public DateTime BuildTime { get; set; }

        /// <summary>Auto-incremented build number for this plugin.</summary>
        public int BuildNumber { get; set; }

        /// <summary>Total number of DLL files found in source.</summary>
        public int DllCount { get; set; }

        /// <summary>Number of DLLs that had their version rewritten.</summary>
        public int RewrittenCount { get; set; }

        /// <summary>Per-DLL rewrite results (only for rewritten DLLs).</summary>
        public List<RewriteResult> RewriteResults { get; set; } = new List<RewriteResult>();

        /// <summary>Full path of the written <c>_manifest.json</c> file.</summary>
        public string ManifestPath { get; set; }

        /// <summary>Non-fatal errors encountered during staging.</summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>The complete manifest data object.</summary>
        public StagingManifest Manifest { get; set; }

        /// <summary>
        /// Returns <c>true</c> if the staging succeeded without any errors.
        /// </summary>
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>
        /// Returns the full path of the main plugin DLL in the staging folder.
        /// </summary>
        public string GetMainDllPath(string mainDllName)
            => string.IsNullOrEmpty(StagingFolder) || string.IsNullOrEmpty(mainDllName)
                ? null
                : Path.Combine(StagingFolder, mainDllName);
    }
}

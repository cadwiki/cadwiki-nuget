using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace cadwiki.PluginReloadService
{
    /// <summary>
    /// Specifies which DLLs should have their assembly version rewritten
    /// during the staging process.
    ///
    /// <para>Supports two exclusive modes:</para>
    /// <list type="bullet">
    ///   <item><b>Whitelist (Include) mode:</b> only DLLs matching at least one
    ///     <see cref="IncludePatterns"/> pattern are rewritten. Blacklist is ignored.</item>
    ///   <item><b>Blacklist (Exclude) mode:</b> all DLLs are rewritten <em>except</em>
    ///     those matching any <see cref="ExcludePatterns"/> pattern.</item>
    ///   <item><b>No-filter mode:</b> all non-SDK DLLs are rewritten (default when
    ///     both pattern lists are empty).</item>
    /// </list>
    ///
    /// <para><b>Precedence:</b> Whitelist &gt; Blacklist &gt; Rewrite-All.</para>
    ///
    /// <para>
    ///   AutoCAD SDK DLLs listed in <see cref="AutoCadSdkBlacklist"/> are ALWAYS
    ///   excluded regardless of the configured filter rules.
    /// </para>
    /// </summary>
    public class DllFilterSpec
    {
        // -------------------------------------------------------------------------
        // AutoCAD SDK hard blacklist (these DLLs are NEVER rewritten)
        // -------------------------------------------------------------------------

        /// <summary>
        /// Read-only list of AutoCAD SDK DLL names that must never be
        /// version-rewritten. These are system DLLs shipped with AutoCAD.
        /// Matches the <c>DllsToSkip</c> list in
        /// <c>AutodeskAppDomainReloader.Dependencies</c>.
        /// </summary>
        public static readonly IReadOnlyList<string> AutoCadSdkBlacklist = new List<string>
        {
            "AcCoreMgd.dll",
            "AcCui.dll",
            "AcDbMgd.dll",
            "acdbmgdbrep.dll",
            "AcDx.dll",
            "AcMgd.dll",
            "AcMr.dll",
            "AcSeamless.dll",
            "AcTcMgd.dll",
            "AcWindows.dll",
            "AdUIMgd.dll",
            "AdUiPalettes.dll",
            "AdWindows.dll",
            "cadwiki.AcRemoveCmdGroup.dll"
        };

        // -------------------------------------------------------------------------
        // Properties
        // -------------------------------------------------------------------------

        /// <summary>
        /// Whitelist patterns. If non-empty, the filter operates in whitelist mode:
        /// only DLLs matching at least one pattern are version-rewritten.
        /// When set, <see cref="ExcludePatterns"/> is completely ignored.
        /// Supports glob patterns: <c>*</c> (any chars), <c>?</c> (single char).
        /// Example: <c>["MyPlugin*.dll", "MyPlugin.Utilities.dll"]</c>
        /// </summary>
        public List<string> IncludePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Blacklist patterns. If non-empty AND <see cref="IncludePatterns"/> is empty,
        /// operates in blacklist mode: all DLLs are rewritten <em>except</em> matches.
        /// Supports glob patterns: <c>*</c> (any chars), <c>?</c> (single char).
        /// Example: <c>["*Test*.dll", "Newtonsoft.*.dll"]</c>
        /// </summary>
        public List<string> ExcludePatterns { get; set; } = new List<string>();

        /// <summary>
        /// The main plugin DLL name (filename only, e.g. <c>MyPlugin.dll</c>).
        /// The main DLL is always included in the rewrite set, even if it would
        /// otherwise be excluded by filter rules. This prevents accidental
        /// misconfiguration from producing a no-op reload.
        /// </summary>
        public string MainDllName { get; set; }

        /// <summary>
        /// When <c>true</c>, verbose per-file evaluation messages are emitted to
        /// the logger during filtering. Controlled by INI setting <c>Verbose=true</c>
        /// or CLI flag <c>--verbose</c>.
        /// </summary>
        public bool VerboseLogging { get; set; }

        // -------------------------------------------------------------------------
        // Core filtering logic
        // -------------------------------------------------------------------------

        /// <summary>
        /// Determines whether the named DLL should have its assembly version rewritten.
        /// </summary>
        /// <param name="fileName">
        ///   Filename only (no directory), e.g. <c>MyPlugin.dll</c>.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the DLL should be version-rewritten;
        ///   <c>false</c> if it should be copied as-is.
        /// </returns>
        /// <remarks>
        /// Decision precedence:
        /// <list type="number">
        ///   <item>Non-.dll files → always false.</item>
        ///   <item>AutoCAD SDK DLLs (<see cref="AutoCadSdkBlacklist"/>) → always false.</item>
        ///   <item>Main DLL (<see cref="MainDllName"/>) → always true (safety net).</item>
        ///   <item>Whitelist mode (IncludePatterns non-empty) → true iff any pattern matches.</item>
        ///   <item>Blacklist mode (ExcludePatterns non-empty) → false iff any pattern matches.</item>
        ///   <item>No filter → true (rewrite everything).</item>
        /// </list>
        /// </remarks>
        public bool ShouldRewrite(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // Rule 1: non-DLL files are never version-rewritten
            if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                return false;

            // Rule 2: AutoCAD SDK DLLs — always skip
            if (IsAutoCADSdkDll(fileName))
                return false;

            // Rule 3: Main plugin DLL — always rewrite (safety net)
            if (!string.IsNullOrEmpty(MainDllName) &&
                string.Equals(fileName, MainDllName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Rule 4: WHITELIST MODE
            if (IncludePatterns != null && IncludePatterns.Count > 0)
                return IncludePatterns.Any(p => GlobMatch(fileName, p));

            // Rule 5: BLACKLIST MODE
            if (ExcludePatterns != null && ExcludePatterns.Count > 0)
                return !ExcludePatterns.Any(p => GlobMatch(fileName, p));

            // Rule 6: No filter — rewrite everything
            return true;
        }

        /// <summary>
        /// Returns a human-readable description of why a file was included
        /// or excluded, suitable for manifest logging.
        /// </summary>
        /// <param name="fileName">Filename only (no directory).</param>
        public string GetFilterReason(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "empty filename";

            if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                return "non-DLL file — always copied as-is";

            if (IsAutoCADSdkDll(fileName))
                return "AutoCAD SDK DLL — never rewritten (hard blacklist)";

            if (!string.IsNullOrEmpty(MainDllName) &&
                string.Equals(fileName, MainDllName, StringComparison.OrdinalIgnoreCase))
                return "main plugin DLL — always rewritten (safety net)";

            if (IncludePatterns != null && IncludePatterns.Count > 0)
            {
                string match = IncludePatterns.FirstOrDefault(p => GlobMatch(fileName, p));
                return match != null
                    ? $"matched include pattern: {match}"
                    : "not in whitelist — copied as-is";
            }

            if (ExcludePatterns != null && ExcludePatterns.Count > 0)
            {
                string match = ExcludePatterns.FirstOrDefault(p => GlobMatch(fileName, p));
                return match != null
                    ? $"matched exclude pattern: {match} — copied as-is"
                    : "no exclude match — rewritten";
            }

            return "no filter configured — rewritten";
        }

        /// <summary>
        /// Returns a concise label for the active filter mode.
        /// Suitable for manifest <c>filterMode</c> field and log output.
        /// </summary>
        public string DescribeMode()
        {
            if (IncludePatterns?.Count > 0)
                return $"whitelist ({IncludePatterns.Count} pattern(s))";
            if (ExcludePatterns?.Count > 0)
                return $"blacklist ({ExcludePatterns.Count} pattern(s))";
            return "none (rewrite all non-SDK DLLs)";
        }

        // -------------------------------------------------------------------------
        // Configuration loaders
        // -------------------------------------------------------------------------

        /// <summary>
        /// Loads a filter spec from a cadwiki INI file.
        /// Reads the <c>[DllFilter]</c> section with keys <c>Include</c> and <c>Exclude</c>.
        /// </summary>
        /// <param name="iniPath">Full path of the INI file.</param>
        /// <param name="mainDllName">Optional main DLL name to attach.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>
        ///   A populated <see cref="DllFilterSpec"/>, or <c>null</c> if the file does not exist.
        /// </returns>
        public static DllFilterSpec FromIni(string iniPath, string mainDllName = null,
                                            PluginReloadLogger logger = null)
        {
            const string Section   = "DllFilter";
            const string Component = "DllFilterSpec";

            if (!File.Exists(iniPath))
            {
                logger?.Debug(Component, $"INI file not found: {iniPath}");
                return null;
            }

            var ini = new NetUtils.IniFile(iniPath);
            var spec = new DllFilterSpec { MainDllName = mainDllName };

            string include = ini.GetString(Section, "Include", "");
            if (!string.IsNullOrWhiteSpace(include))
                spec.IncludePatterns = ParseSemicolonList(include);

            string exclude = ini.GetString(Section, "Exclude", "");
            if (!string.IsNullOrWhiteSpace(exclude))
                spec.ExcludePatterns = ParseSemicolonList(exclude);

            string verboseStr = ini.GetString(Section, "Verbose", "false");
            spec.VerboseLogging = string.Equals(verboseStr, "true",
                StringComparison.OrdinalIgnoreCase);

            logger?.Info(Component,
                $"Loaded from INI: mode={spec.DescribeMode()}, verbose={spec.VerboseLogging}");
            return spec;
        }

        /// <summary>
        /// Loads a filter spec from a <c>cadwiki-reload.json</c> file placed in the
        /// plugin's build output directory (or any explicit path).
        /// </summary>
        /// <param name="jsonPath">
        ///   Full path to the JSON file (typically <c>cadwiki-reload.json</c>
        ///   in the plugin's build output folder).
        /// </param>
        /// <param name="mainDllName">Optional main DLL name to attach.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>
        ///   A populated <see cref="DllFilterSpec"/>, or <c>null</c> if the file
        ///   does not exist.
        /// </returns>
        public static DllFilterSpec FromJson(string jsonPath, string mainDllName = null,
                                             PluginReloadLogger logger = null)
        {
            const string Component = "DllFilterSpec";

            if (!File.Exists(jsonPath))
            {
                logger?.Debug(Component, $"cadwiki-reload.json not found: {jsonPath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(jsonPath);

                var serializer = new JavaScriptSerializer();
                var root = serializer.Deserialize<Dictionary<string, object>>(json);

                var spec = new DllFilterSpec { MainDllName = mainDllName };

                if (root != null && root.TryGetValue("dllFilter", out var filterObj))
                {
                    var filter = filterObj as Dictionary<string, object>;
                    if (filter != null)
                    {
                        if (filter.TryGetValue("include", out var incObj))
                        {
                            var incList = incObj as object[];
                            if (incList != null)
                            {
                                spec.IncludePatterns = incList
                                    .Select(o => o?.ToString())
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .ToList();
                            }
                        }

                        if (filter.TryGetValue("exclude", out var excObj))
                        {
                            var excList = excObj as object[];
                            if (excList != null)
                            {
                                spec.ExcludePatterns = excList
                                    .Select(o => o?.ToString())
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .ToList();
                            }
                        }

                        if (filter.TryGetValue("verbose", out var verbObj))
                        {
                            if (verbObj is bool b)
                                spec.VerboseLogging = b;
                        }
                    }
                }

                logger?.Info(Component,
                    $"Loaded from JSON: {jsonPath}, mode={spec.DescribeMode()}");

                return spec;
            }
            catch (Exception ex)
            {
                logger?.Exception(Component, "FromJson", ex, jsonPath);
                return null;
            }
        }
        /// <summary>
        /// Builds a filter spec from command-line argument tokens.
        /// Recognises <c>--include &lt;pattern&gt;</c> and
        /// <c>--exclude &lt;pattern&gt;</c> pairs.
        /// Also recognises <c>--main &lt;dllName&gt;</c> and <c>--verbose</c>.
        /// </summary>
        /// <param name="args">Command-line argument array (e.g. <c>Environment.GetCommandLineArgs()</c>).</param>
        /// <param name="logger">Optional logger.</param>
        public static DllFilterSpec FromArgs(string[] args, PluginReloadLogger logger = null)
        {
            const string Component = "DllFilterSpec";

            var spec = new DllFilterSpec();

            if (args == null || args.Length == 0)
                return spec;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--include":
                        if (i + 1 < args.Length)
                            spec.IncludePatterns.Add(args[++i]);
                        break;
                    case "--exclude":
                        if (i + 1 < args.Length)
                            spec.ExcludePatterns.Add(args[++i]);
                        break;
                    case "--main":
                        if (i + 1 < args.Length)
                            spec.MainDllName = args[++i];
                        break;
                    case "--verbose":
                        spec.VerboseLogging = true;
                        break;
                }
            }

            logger?.Info(Component,
                $"Loaded from CLI args: mode={spec.DescribeMode()}, verbose={spec.VerboseLogging}");
            return spec;
        }

        /// <summary>
        /// Creates a spec with precedence resolution:
        /// CLI args &gt; cadwiki-reload.json &gt; INI &gt; default (rewrite-all).
        /// </summary>
        /// <param name="sourceBuildDir">
        ///   Plugin build output directory, checked for <c>cadwiki-reload.json</c>.
        /// </param>
        /// <param name="mainDllName">Main DLL filename.</param>
        /// <param name="iniPath">Optional INI file path.</param>
        /// <param name="cliArgs">Optional CLI argument tokens.</param>
        /// <param name="logger">Optional logger.</param>
        public static DllFilterSpec LoadFilterSpec(
            string sourceBuildDir,
            string mainDllName,
            string iniPath   = null,
            string[] cliArgs = null,
            PluginReloadLogger logger = null)
        {
            const string Component = "DllFilterSpec";

            // 1. CLI args (highest precedence)
            if (cliArgs != null && cliArgs.Length > 0)
            {
                var spec = FromArgs(cliArgs, logger);
                if (spec.IncludePatterns.Count > 0 || spec.ExcludePatterns.Count > 0)
                {
                    spec.MainDllName = mainDllName;
                    logger?.Info(Component, $"Using CLI filter: {spec.DescribeMode()}");
                    return spec;
                }
            }

            // 2. cadwiki-reload.json in build output directory
            string jsonPath = Path.Combine(sourceBuildDir, "cadwiki-reload.json");
            {
                var spec = FromJson(jsonPath, mainDllName, logger);
                if (spec != null)
                {
                    logger?.Info(Component, $"Using JSON filter: {spec.DescribeMode()}");
                    return spec;
                }
            }

            // 3. INI file
            if (!string.IsNullOrEmpty(iniPath))
            {
                var spec = FromIni(iniPath, mainDllName, logger);
                if (spec != null &&
                    (spec.IncludePatterns.Count > 0 || spec.ExcludePatterns.Count > 0))
                {
                    logger?.Info(Component, $"Using INI filter: {spec.DescribeMode()}");
                    return spec;
                }
            }

            // 4. Default — no filter
            logger?.Info(Component, "No filter config found; using default (rewrite all non-SDK DLLs)");
            return new DllFilterSpec { MainDllName = mainDllName };
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        /// <summary>
        /// Returns <c>true</c> if the filename is on the AutoCAD SDK hard blacklist
        /// (case-insensitive comparison).
        /// </summary>
        public static bool IsAutoCADSdkDll(string fileName)
            => AutoCadSdkBlacklist.Any(s =>
                   string.Equals(s, fileName, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Performs glob-style matching.
        /// Supports <c>*</c> (any sequence of characters) and <c>?</c> (single character).
        /// Matching is case-insensitive.
        /// </summary>
        public static bool GlobMatch(string input, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            // Convert glob to regex
            string regex = "^" + Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".") + "$";

            return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase);
        }

        private static List<string> ParseSemicolonList(string value)
            => value
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
    }
}

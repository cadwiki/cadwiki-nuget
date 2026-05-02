using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace cadwiki.PluginReloadService
{
    /// <summary>
    /// JSON manifest written to <c>_manifest.json</c> in each staging folder.
    /// Contains a complete record of the staging operation for diagnostics
    /// and traceability.
    /// </summary>
    public class StagingManifest
    {
        /// <summary>Plugin name (same as the plugin folder name).</summary>
        public string PluginName { get; set; }

        /// <summary>Source directory from which DLLs were staged.</summary>
        public string SourceDirectory { get; set; }

        /// <summary>Full path of the staging folder.</summary>
        public string StagingFolder { get; set; }

        /// <summary>Build timestamp used for version encoding.</summary>
        public DateTime BuildTime { get; set; }

        /// <summary>Short build counter for the staging folder name (1, 2, 3 …).</summary>
        public int BuildNumber { get; set; }

        /// <summary>Active filter mode description (e.g. "whitelist (3 patterns)").</summary>
        public string FilterMode { get; set; }

        /// <summary>Whitelist patterns (empty if blacklist/no-filter mode).</summary>
        public List<string> IncludePatterns { get; set; } = new List<string>();

        /// <summary>Blacklist patterns (empty if whitelist/no-filter mode).</summary>
        public List<string> ExcludePatterns { get; set; } = new List<string>();

        /// <summary>Per-DLL records.</summary>
        public List<DllManifestEntry> Dlls { get; set; } = new List<DllManifestEntry>();

        /// <summary>Non-DLL files that were copied verbatim.</summary>
        public List<string> OtherFiles { get; set; } = new List<string>();

        /// <summary>Total number of DLLs processed.</summary>
        public int DllCount { get; set; }

        /// <summary>Number of DLLs whose version was rewritten.</summary>
        public int RewrittenCount { get; set; }

        /// <summary>Full path to the associated _reload.log diagnostic file.</summary>
        public string ReloadLogPath { get; set; }

        /// <summary>UTC timestamp when this manifest was created.</summary>
        public string CreatedAtUtc { get; set; }
    }

    /// <summary>
    /// Records the staging outcome for a single DLL file.
    /// </summary>
    public class DllManifestEntry
    {
        /// <summary>Filename only (e.g. <c>MyPlugin.dll</c>).</summary>
        public string FileName { get; set; }

        /// <summary>
        /// Whether the DLL was version-rewritten (<c>true</c>) or copied
        /// verbatim (<c>false</c>).
        /// </summary>
        public bool WasRewritten { get; set; }

        /// <summary>Original assembly version string (e.g. <c>1.0.0.0</c>).</summary>
        public string OriginalVersion { get; set; }

        /// <summary>
        /// New assembly version string after rewriting (e.g. <c>1.0.0.231371580</c>).
        /// Null/empty when <see cref="WasRewritten"/> is <c>false</c>.
        /// </summary>
        public string NewVersion { get; set; }

        /// <summary>
        /// Human-readable display version (e.g. <c>1.0.0.2023_10_15_14_22_30</c>).
        /// </summary>
        public string DisplayVersion { get; set; }

        /// <summary>Filter reason from <see cref="DllFilterSpec.GetFilterReason"/>.</summary>
        public string FilterReason { get; set; }

        /// <summary>Whether a matching PDB was detected and preserved.</summary>
        public bool PdbPreserved { get; set; }
    }

    /// <summary>
    /// Handles serialisation and deserialisation of <see cref="StagingManifest"/>
    /// objects to/from <c>_manifest.json</c>.
    ///
    /// Uses <see cref="System.Text.Json"/> when available. Falls back to manual
    /// string building if serialisation fails.
    /// </summary>
    public static class StagingManifestSerializer
    {
        /// <summary>Manifest filename inside each staging folder.</summary>
        public const string FileName = "_manifest.json";

        /// <summary>
        /// Serialises a <see cref="StagingManifest"/> to <c>_manifest.json</c>
        /// in <paramref name="stagingFolder"/>.
        /// </summary>
        public static string Write(StagingManifest manifest, string stagingFolder,
                                    PluginReloadLogger logger = null)
        {
            const string Component = "StagingManifest";
            string path = Path.Combine(stagingFolder, FileName);

            try
            {
                manifest.CreatedAtUtc = DateTime.UtcNow.ToString("o");

                string json = SerializeToJson(manifest);
                File.WriteAllText(path, json, Encoding.UTF8);

                logger?.Info(Component, $"Manifest written: {path}");
                return path;
            }
            catch (Exception ex)
            {
                logger?.Exception(Component, "Write", ex, path);
                return null;
            }
        }

        // -------------------------------------------------------------------------
        // Internal JSON helpers — manual serialisation so we never depend on the
        // System.Text.Json NuGet package at runtime (the package may not be installed
        // in all deployment scenarios). The JSON produced is always valid.
        // -------------------------------------------------------------------------

        private static string SerializeToJson(StagingManifest m)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"pluginName\": {Q(m.PluginName)},");
            sb.AppendLine($"  \"sourceDirectory\": {Q(m.SourceDirectory)},");
            sb.AppendLine($"  \"stagingFolder\": {Q(m.StagingFolder)},");
            sb.AppendLine($"  \"buildTime\": {Q(m.BuildTime.ToString("o"))},");
            sb.AppendLine($"  \"buildNumber\": {m.BuildNumber},");
            sb.AppendLine($"  \"filterMode\": {Q(m.FilterMode)},");
            sb.AppendLine($"  \"includePatterns\": {SerializeStringList(m.IncludePatterns)},");
            sb.AppendLine($"  \"excludePatterns\": {SerializeStringList(m.ExcludePatterns)},");
            sb.AppendLine($"  \"dllCount\": {m.DllCount},");
            sb.AppendLine($"  \"rewrittenCount\": {m.RewrittenCount},");
            sb.AppendLine($"  \"reloadLogPath\": {Q(m.ReloadLogPath)},");
            sb.AppendLine($"  \"createdAtUtc\": {Q(m.CreatedAtUtc)},");
            sb.AppendLine($"  \"otherFiles\": {SerializeStringList(m.OtherFiles)},");

            sb.AppendLine("  \"dlls\": [");
            for (int i = 0; i < m.Dlls.Count; i++)
            {
                var d = m.Dlls[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"fileName\": {Q(d.FileName)},");
                sb.AppendLine($"      \"wasRewritten\": {(d.WasRewritten ? "true" : "false")},");
                sb.AppendLine($"      \"originalVersion\": {Q(d.OriginalVersion)},");
                sb.AppendLine($"      \"newVersion\": {Q(d.NewVersion)},");
                sb.AppendLine($"      \"displayVersion\": {Q(d.DisplayVersion)},");
                sb.AppendLine($"      \"filterReason\": {Q(d.FilterReason)},");
                sb.Append($"      \"pdbPreserved\": {(d.PdbPreserved ? "true" : "false")}");
                sb.AppendLine();
                sb.Append("    }");
                sb.AppendLine(i < m.Dlls.Count - 1 ? "," : "");
            }
            sb.AppendLine("  ]");
            sb.Append("}");
            return sb.ToString();
        }

        private static string SerializeStringList(List<string> list)
        {
            if (list == null || list.Count == 0) return "[]";
            var items = string.Join(", ", list.ConvertAll(s => Q(s)));
            return $"[ {items} ]";
        }

        /// <summary>Quotes and escapes a string for JSON output.</summary>
        private static string Q(string value)
        {
            if (value == null) return "null";
            return "\"" + value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                + "\"";
        }
    }
}

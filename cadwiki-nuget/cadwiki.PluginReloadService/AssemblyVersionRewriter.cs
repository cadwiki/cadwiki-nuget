using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace cadwiki.PluginReloadService
{
    /// <summary>
    /// Rewrites the assembly version attributes on a compiled DLL using Mono.Cecil.
    /// The original source file and the plugin's build output are NEVER modified.
    /// A rewritten copy is written to a staging path.
    ///
    /// <para><b>Version encoding strategy:</b></para>
    /// <para>
    ///   The fourth segment (revision) of <c>System.Version</c> must fit in an
    ///   <c>Int32</c>. We use a compact timestamp:
    ///   <code>Revision = (DaysSinceEpoch(2020-01-01) × 100_000) + SecondsInDay</code>
    ///   This gives ~58 years of headroom (to year 2078) while guaranteeing
    ///   strict monotonic increase every second.
    /// </para>
    /// <para>
    ///   A human-readable form is also written to
    ///   <c>AssemblyInformationalVersion</c>: <c>#.#.#.YYYY_MM_DD_HH_mm_ss</c>.
    /// </para>
    /// </summary>
    public static class AssemblyVersionRewriter
    {
        // -------------------------------------------------------------------------
        // Constants
        // -------------------------------------------------------------------------

        /// <summary>
        /// Epoch for compact timestamp encoding.
        /// Days are counted from this date (2020-01-01).
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local);

        private const string AssemblyFileVersionAttrName =
            "System.Reflection.AssemblyFileVersionAttribute";

        private const string AssemblyInformationalVersionAttrName =
            "System.Reflection.AssemblyInformationalVersionAttribute";

        // -------------------------------------------------------------------------
        // Public API
        // -------------------------------------------------------------------------

        /// <summary>
        /// Rewrites the assembly version attributes of <paramref name="sourcePath"/>
        /// and writes the result to <paramref name="outputPath"/>.
        /// The source file is never modified.
        /// </summary>
        /// <param name="sourcePath">Path to the original compiled DLL.</param>
        /// <param name="outputPath">
        ///   Destination path for the version-stamped copy (may equal sourcePath
        ///   only if you explicitly want in-place rewriting — not recommended).
        /// </param>
        /// <param name="buildTime">Timestamp used for version encoding.</param>
        /// <param name="logger">Optional structured logger.</param>
        /// <returns>Metadata about the rewrite operation.</returns>
        /// <exception cref="FileNotFoundException">
        ///   Thrown when <paramref name="sourcePath"/> does not exist.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   Thrown when the file is not a managed .NET assembly.
        ///   Callers should check <see cref="IsManagedAssembly"/> first.
        /// </exception>
        public static RewriteResult Rewrite(
            string sourcePath,
            string outputPath,
            DateTime buildTime,
            PluginReloadLogger logger = null)
        {
            const string Component = "AssemblyVersionRewriter";

            if (!File.Exists(sourcePath))
            {
                string msg = $"Source DLL not found: {sourcePath}. Ensure build completed successfully.";
                logger?.Error(Component, "Rewrite", msg, sourcePath,
                    "Ensure the plugin project has been built before triggering reload.");
                throw new FileNotFoundException(msg, sourcePath);
            }

            int revision = ComputeCompactTimestamp(buildTime);
            logger?.Debug(Component,
                $"Compact timestamp → epoch offset: {(int)(buildTime.Date - Epoch).TotalDays} days, " +
                $"seconds in day: {buildTime.Hour * 3600 + buildTime.Minute * 60 + buildTime.Second}, " +
                $"revision: {revision}");

            AssemblyDefinition asm = null;
            try
            {
                // Read without write-lock so we can use a copy strategy
                var readerParams = new ReaderParameters
                {
                    ReadWrite = false,
                    ReadSymbols = false   // we handle PDB separately
                };
                asm = AssemblyDefinition.ReadAssembly(sourcePath, readerParams);

                Version originalVersion = asm.Name.Version;
                var newVersion = new Version(
                    originalVersion.Major,
                    originalVersion.Minor,
                    originalVersion.Build < 0 ? 0 : originalVersion.Build,
                    revision);

                string newVersionStr = newVersion.ToString();
                string displayVersion =
                    $"{originalVersion.Major}.{originalVersion.Minor}" +
                    $".{(originalVersion.Build < 0 ? 0 : originalVersion.Build)}" +
                    $".{buildTime:yyyy_MM_dd_HH_mm_ss}";

                logger?.Debug(Component,
                    $"Input:  {Path.GetFileName(sourcePath)} original version {originalVersion}");
                logger?.Debug(Component,
                    $"Output: new version {newVersionStr}, display version {displayVersion}");

                // 1. Rewrite AssemblyVersion (identity version — used for binding)
                asm.Name.Version = newVersion;

                // 2. Rewrite AssemblyFileVersion (what FileVersionInfo.GetVersionInfo reads)
                SetOrAddStringAttribute(asm, AssemblyFileVersionAttrName, newVersionStr);

                // 3. Write human-readable display version to InformationalVersion
                SetOrAddStringAttribute(asm, AssemblyInformationalVersionAttrName, displayVersion);

                // 4. Write to output path (preserves source)
                var writerParams = new WriterParameters();
                asm.Write(outputPath, writerParams);

                logger?.Info(Component,
                    $"{Path.GetFileName(sourcePath)} → REWRITTEN " +
                    $"({originalVersion} → {newVersionStr}, display: {displayVersion})");
                logger?.Info(Component, $"Written to: {outputPath}");

                return new RewriteResult
                {
                    SourcePath = sourcePath,
                    OutputPath = outputPath,
                    OriginalVersion = originalVersion,
                    NewVersion = newVersion,
                    DisplayVersion = displayVersion,
                    CompactTimestamp = revision,
                    BuildTime = buildTime
                };
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                logger?.Exception(Component, "Rewrite", ex, sourcePath);
                throw;
            }
            finally
            {
                asm?.Dispose();
            }
        }

        /// <summary>
        /// Computes the compact 4th-segment revision integer from a timestamp.
        /// <code>Revision = (DaysSinceEpoch × 100_000) + SecondsInDay</code>
        /// </summary>
        /// <param name="time">The build timestamp.</param>
        /// <returns>
        ///   A positive <c>Int32</c> that is strictly increasing per second
        ///   and fits comfortably within <see cref="int.MaxValue"/>.
        /// </returns>
        public static int ComputeCompactTimestamp(DateTime time)
        {
            int daysSinceEpoch = (int)(time.Date - Epoch.Date).TotalDays;
            int secondsInDay = time.Hour * 3600 + time.Minute * 60 + time.Second;
            return daysSinceEpoch * 100_000 + secondsInDay;
        }

        /// <summary>
        /// Returns a human-readable display version string for a given timestamp.
        /// Format: <c>major.minor.build.YYYY_MM_DD_HH_mm_ss</c>.
        /// </summary>
        public static string FormatDisplayVersion(Version originalVersion, DateTime buildTime)
        {
            int build = originalVersion.Build < 0 ? 0 : originalVersion.Build;
            return $"{originalVersion.Major}.{originalVersion.Minor}.{build}.{buildTime:yyyy_MM_dd_HH_mm_ss}";
        }

        /// <summary>
        /// Returns <c>true</c> if the file at <paramref name="path"/> is a valid
        /// managed .NET assembly. Returns <c>false</c> for native DLLs and
        /// corrupted files.
        /// </summary>
        /// <remarks>
        /// This check uses <see cref="AssemblyName.GetAssemblyName"/> which examines
        /// the PE header. Native DLLs throw <see cref="BadImageFormatException"/>.
        /// </remarks>
        public static bool IsManagedAssembly(string path)
        {
            try
            {
                AssemblyName.GetAssemblyName(path);
                return true;
            }
            catch (BadImageFormatException)
            {
                return false;
            }
            catch (FileLoadException)
            {
                // Managed but currently locked; assume managed
                return true;
            }
            catch
            {
                return false;
            }
        }

        // -------------------------------------------------------------------------
        // Internal helpers
        // -------------------------------------------------------------------------

        /// <summary>
        /// Sets the single string constructor argument of a named custom attribute
        /// on the assembly manifest. If the attribute does not exist, it is added.
        /// </summary>
        private static void SetOrAddStringAttribute(
            AssemblyDefinition asm,
            string attributeTypeFullName,
            string value)
        {
            var module = asm.MainModule;

            // Find existing attribute
            var existing = asm.CustomAttributes.FirstOrDefault(
                a => a.AttributeType.FullName == attributeTypeFullName);

            if (existing != null)
            {
                existing.ConstructorArguments.Clear();
                existing.ConstructorArguments.Add(
                    new CustomAttributeArgument(module.TypeSystem.String, value));
            }
            else
            {
                // Attribute not present — create and add it
                // Map attribute type name to BCL type for reflection-based import
                Type clrType = ResolveAttributeType(attributeTypeFullName);
                if (clrType == null) return;   // unknown attribute — skip gracefully

                var ctorInfo = clrType.GetConstructor(new[] { typeof(string) });
                if (ctorInfo == null) return;

                var attrTypeRef = module.ImportReference(clrType);
                var ctorRef     = module.ImportReference(ctorInfo);

                var attr = new CustomAttribute(ctorRef);
                attr.ConstructorArguments.Add(
                    new CustomAttributeArgument(module.TypeSystem.String, value));
                asm.CustomAttributes.Add(attr);
            }
        }

        private static Type ResolveAttributeType(string fullName)
        {
            switch (fullName)
            {
                case AssemblyFileVersionAttrName:
                    return typeof(System.Reflection.AssemblyFileVersionAttribute);
                case AssemblyInformationalVersionAttrName:
                    return typeof(System.Reflection.AssemblyInformationalVersionAttribute);
                default:
                    return null;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Result model
    // -------------------------------------------------------------------------

    /// <summary>
    /// Holds the outcome of a single DLL version-rewrite operation.
    /// </summary>
    public class RewriteResult
    {
        /// <summary>Full path of the source DLL (not modified).</summary>
        public string SourcePath { get; set; }

        /// <summary>Full path of the rewritten output DLL.</summary>
        public string OutputPath { get; set; }

        /// <summary>Assembly version before rewriting.</summary>
        public Version OriginalVersion { get; set; }

        /// <summary>
        /// New assembly version after rewriting.
        /// Revision segment contains the compact timestamp.
        /// </summary>
        public Version NewVersion { get; set; }

        /// <summary>
        /// Human-readable informational version string.
        /// Format: <c>major.minor.build.YYYY_MM_DD_HH_mm_ss</c>.
        /// </summary>
        public string DisplayVersion { get; set; }

        /// <summary>The compact timestamp integer used as the revision segment.</summary>
        public int CompactTimestamp { get; set; }

        /// <summary>The build timestamp used for this rewrite.</summary>
        public DateTime BuildTime { get; set; }
    }
}

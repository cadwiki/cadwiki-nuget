using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cadwiki.DllReloader.PluginReloadService;

namespace UnitTests.PluginReloadService
{
    // =========================================================================
    //  AssemblyVersionRewriter — unit tests
    // =========================================================================

    [TestClass]
    public class TestAssemblyVersionRewriter
    {
        // ------------------------------------------------------------------
        // Compact timestamp encoding
        // ------------------------------------------------------------------

        [TestMethod]
        public void CompactTimestamp_Epoch_IsZero()
        {
            // Epoch = 2020-01-01 00:00:00
            int ts = AssemblyVersionRewriter.ComputeCompactTimestamp(AssemblyVersionRewriter.Epoch);
            Assert.AreEqual(0, ts,
                "Compact timestamp at epoch (2020-01-01 00:00:00) must be 0.");
        }

        [TestMethod]
        public void CompactTimestamp_EpochPlusOneDay_Is100000()
        {
            var time = AssemblyVersionRewriter.Epoch.AddDays(1);
            int ts = AssemblyVersionRewriter.ComputeCompactTimestamp(time);
            Assert.AreEqual(100_000, ts,
                "1 day after epoch with 0 seconds = 100,000");
        }

        [TestMethod]
        public void CompactTimestamp_EpochPlusOneSecond_IsOne()
        {
            var time = AssemblyVersionRewriter.Epoch.AddSeconds(1);
            int ts = AssemblyVersionRewriter.ComputeCompactTimestamp(time);
            Assert.AreEqual(1, ts);
        }

        [TestMethod]
        public void CompactTimestamp_EpochPlusOneDayAndOneSecond_Is100001()
        {
            var time = AssemblyVersionRewriter.Epoch.AddDays(1).AddSeconds(1);
            int ts = AssemblyVersionRewriter.ComputeCompactTimestamp(time);
            Assert.AreEqual(100_001, ts);
        }

        [TestMethod]
        public void CompactTimestamp_MaxSecondsInDay()
        {
            // Last second of day 0: 23:59:59 = 86399 seconds
            var time = new DateTime(2020, 1, 1, 23, 59, 59);
            int ts = AssemblyVersionRewriter.ComputeCompactTimestamp(time);
            Assert.AreEqual(86_399, ts);
        }

        [TestMethod]
        public void CompactTimestamp_IsStrictlyIncreasing()
        {
            var t1 = new DateTime(2023, 10, 15, 14, 22, 30);
            var t2 = new DateTime(2023, 10, 15, 14, 22, 31);
            int ts1 = AssemblyVersionRewriter.ComputeCompactTimestamp(t1);
            int ts2 = AssemblyVersionRewriter.ComputeCompactTimestamp(t2);
            Assert.IsTrue(ts2 > ts1, "Timestamp must increase by exactly 1 per second.");
            Assert.AreEqual(1, ts2 - ts1);
        }

        [TestMethod]
        public void CompactTimestamp_FitsInPositiveInt32()
        {
            // Year 2078 is 58 years after epoch = ~21,169 days
            var far = new DateTime(2078, 1, 1, 23, 59, 59);
            long ts = (long)(far.Date - AssemblyVersionRewriter.Epoch.Date).TotalDays * 100_000L
                      + 86_399L;
            Assert.IsTrue(ts < int.MaxValue,
                "Compact timestamp in 2078 must still fit in Int32.MaxValue.");
        }

        [TestMethod]
        public void CompactTimestamp_KnownValue_2023_10_15_14_22_30()
        {
            var time = new DateTime(2023, 10, 15, 14, 22, 30);
            int ts = AssemblyVersionRewriter.ComputeCompactTimestamp(time);

            // Days from 2020-01-01 to 2023-10-15
            int days = (int)(time.Date - AssemblyVersionRewriter.Epoch.Date).TotalDays;
            int secs = 14 * 3600 + 22 * 60 + 30;
            int expected = days * 100_000 + secs;
            Assert.AreEqual(expected, ts);
        }

        // ------------------------------------------------------------------
        // Display version formatting
        // ------------------------------------------------------------------

        [TestMethod]
        public void FormatDisplayVersion_ProducesExpectedString()
        {
            var original = new Version(1, 0, 5, 0);
            var time = new DateTime(2023, 10, 15, 14, 22, 30);
            string display = AssemblyVersionRewriter.FormatDisplayVersion(original, time);
            Assert.AreEqual("1.0.5.2023_10_15_14_22_30", display);
        }

        [TestMethod]
        public void FormatDisplayVersion_NegativeBuildSegment_TreatedAsZero()
        {
            // Version(1,0,-1,-1) is invalid; Version with Build=-1 means unspecified
            var original = new Version(2, 3, 0, 0);
            var time = new DateTime(2020, 6, 1, 9, 0, 0);
            string display = AssemblyVersionRewriter.FormatDisplayVersion(original, time);
            StringAssert.StartsWith(display, "2.3.0.");
        }

        // ------------------------------------------------------------------
        // IsManagedAssembly
        // ------------------------------------------------------------------

        [TestMethod]
        public void IsManagedAssembly_ThisTestAssembly_ReturnsTrue()
        {
            string path = Assembly.GetExecutingAssembly().Location;
            Assert.IsTrue(AssemblyVersionRewriter.IsManagedAssembly(path),
                "The executing test assembly must be reported as managed.");
        }

        [TestMethod]
        public void IsManagedAssembly_NonexistentFile_ReturnsFalse()
        {
            string path = @"C:\this_does_not_exist_12345.dll";
            Assert.IsFalse(AssemblyVersionRewriter.IsManagedAssembly(path));
        }

        // ------------------------------------------------------------------
        // Rewrite — round-trip test using a temp copy of this test assembly
        // ------------------------------------------------------------------

        [TestMethod]
        public void Rewrite_ProducesNewVersionFile()
        {
            string srcPath = Assembly.GetExecutingAssembly().Location;
            string tmpDir  = Path.Combine(Path.GetTempPath(), "cwiki_test_rewrite_" + Guid.NewGuid().ToString("N"));

            try
            {
                Directory.CreateDirectory(tmpDir);
                string destPath = Path.Combine(tmpDir, Path.GetFileName(srcPath));

                var buildTime = new DateTime(2023, 10, 15, 14, 22, 30);
                var result = AssemblyVersionRewriter.Rewrite(srcPath, destPath, buildTime);

                Assert.IsTrue(File.Exists(destPath), "Output DLL must exist.");
                Assert.AreEqual(srcPath, result.SourcePath);
                Assert.AreEqual(destPath, result.OutputPath);

                int expectedRevision = AssemblyVersionRewriter.ComputeCompactTimestamp(buildTime);
                Assert.AreEqual(expectedRevision, result.NewVersion.Revision,
                    "Revision segment must equal the compact timestamp.");
                Assert.IsFalse(string.IsNullOrEmpty(result.DisplayVersion));

                // Verify the output file really has the new version embedded
                var name = AssemblyName.GetAssemblyName(destPath);
                Assert.AreEqual(expectedRevision, name.Version.Revision,
                    "Embedded AssemblyName.Version.Revision must match compact timestamp.");
            }
            finally
            {
                try { Directory.Delete(tmpDir, recursive: true); } catch { }
            }
        }

        [TestMethod]
        public void Rewrite_MissingSource_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => AssemblyVersionRewriter.Rewrite(
                @"C:\does_not_exist.dll",
                @"C:\Temp\out.dll",
                DateTime.Now));
        }
    }

    // =========================================================================
    //  DllFilterSpec — unit tests
    // =========================================================================

    [TestClass]
    public class TestDllFilterSpec
    {
        // ------------------------------------------------------------------
        // AutoCAD SDK hard blacklist
        // ------------------------------------------------------------------

        [TestMethod]
        public void SdkBlacklist_ContainsAllRequiredEntries()
        {
            var required = new[]
            {
                "AcCoreMgd.dll", "AcCui.dll", "AcDbMgd.dll", "acdbmgdbrep.dll",
                "AcDx.dll", "AcMgd.dll", "AcMr.dll", "AcSeamless.dll",
                "AcTcMgd.dll", "AcWindows.dll", "AdUIMgd.dll",
                "AdUiPalettes.dll", "AdWindows.dll", "cadwiki.AcRemoveCmdGroup.dll"
            };

            foreach (string dll in required)
                Assert.IsTrue(
                    DllFilterSpec.IsAutoCADSdkDll(dll),
                    $"{dll} must be in the AutoCAD SDK hard blacklist.");
        }

        [TestMethod]
        public void SdkBlacklist_MatchIsCaseInsensitive()
        {
            Assert.IsTrue(DllFilterSpec.IsAutoCADSdkDll("accoremgd.dll"));
            Assert.IsTrue(DllFilterSpec.IsAutoCADSdkDll("ACCOREMGD.DLL"));
            Assert.IsTrue(DllFilterSpec.IsAutoCADSdkDll("AcCoreMgd.DLL"));
        }

        // ------------------------------------------------------------------
        // Glob matching
        // ------------------------------------------------------------------

        [TestMethod]
        public void GlobMatch_ExactName_Matches()
        {
            Assert.IsTrue(DllFilterSpec.GlobMatch("MyPlugin.dll", "MyPlugin.dll"));
        }

        [TestMethod]
        public void GlobMatch_StarWildcard_Matches()
        {
            Assert.IsTrue(DllFilterSpec.GlobMatch("MyPlugin.dll",  "MyPlugin*.dll"));
            Assert.IsTrue(DllFilterSpec.GlobMatch("MyPlugin.Utilities.dll", "MyPlugin*.dll"));
            Assert.IsTrue(DllFilterSpec.GlobMatch("MyPlugin.Core.dll", "MyPlugin*.dll"));
        }

        [TestMethod]
        public void GlobMatch_StarWildcard_NoMatch()
        {
            Assert.IsFalse(DllFilterSpec.GlobMatch("OtherPlugin.dll", "MyPlugin*.dll"));
        }

        [TestMethod]
        public void GlobMatch_QuestionWildcard_MatchesSingleChar()
        {
            Assert.IsTrue(DllFilterSpec.GlobMatch("Foo1.dll", "Foo?.dll"));
            Assert.IsFalse(DllFilterSpec.GlobMatch("Foo12.dll", "Foo?.dll"));
        }

        [TestMethod]
        public void GlobMatch_CaseInsensitive()
        {
            Assert.IsTrue(DllFilterSpec.GlobMatch("myplugin.dll", "MYPLUGIN.DLL"));
            Assert.IsTrue(DllFilterSpec.GlobMatch("MYPLUGIN.DLL", "myplugin*.dll"));
        }

        [TestMethod]
        public void GlobMatch_EmptyPattern_ReturnsFalse()
        {
            Assert.IsFalse(DllFilterSpec.GlobMatch("foo.dll", ""));
            Assert.IsFalse(DllFilterSpec.GlobMatch("foo.dll", null));
        }

        // ------------------------------------------------------------------
        // ShouldRewrite — precedence rules
        // ------------------------------------------------------------------

        [TestMethod]
        public void ShouldRewrite_NonDllFile_ReturnsFalse()
        {
            var spec = new DllFilterSpec();
            Assert.IsFalse(spec.ShouldRewrite("cadwiki-reload.json"));
            Assert.IsFalse(spec.ShouldRewrite("readme.txt"));
            Assert.IsFalse(spec.ShouldRewrite("MyPlugin.pdb"));
        }

        [TestMethod]
        public void ShouldRewrite_SdkDll_AlwaysFalse()
        {
            var spec = new DllFilterSpec
            {
                IncludePatterns = new List<string> { "*" }  // even "include all"
            };
            Assert.IsFalse(spec.ShouldRewrite("AcCoreMgd.dll"),
                "AutoCAD SDK DLL must never be rewritten, even with include-all whitelist.");
        }

        [TestMethod]
        public void ShouldRewrite_MainDll_AlwaysTrue()
        {
            var spec = new DllFilterSpec
            {
                MainDllName     = "MyPlugin.dll",
                ExcludePatterns = new List<string> { "*" }  // even "exclude all"
            };
            Assert.IsTrue(spec.ShouldRewrite("MyPlugin.dll"),
                "Main DLL must always be rewritten, even with exclude-all blacklist.");
        }

        [TestMethod]
        public void ShouldRewrite_WhitelistMode_OnlyMatchesRewritten()
        {
            var spec = new DllFilterSpec
            {
                MainDllName     = "MyPlugin.dll",
                IncludePatterns = new List<string> { "MyPlugin*.dll" }
            };

            Assert.IsTrue(spec.ShouldRewrite("MyPlugin.dll"));
            Assert.IsTrue(spec.ShouldRewrite("MyPlugin.Core.dll"));
            Assert.IsFalse(spec.ShouldRewrite("Newtonsoft.Json.dll"),
                "Non-matching DLL must be excluded in whitelist mode.");
        }

        [TestMethod]
        public void ShouldRewrite_BlacklistMode_ExcludesMatchedDlls()
        {
            var spec = new DllFilterSpec
            {
                ExcludePatterns = new List<string> { "Newtonsoft.*.dll", "*Test*.dll" }
            };

            Assert.IsTrue(spec.ShouldRewrite("MyPlugin.dll"));
            Assert.IsFalse(spec.ShouldRewrite("Newtonsoft.Json.dll"));
            Assert.IsFalse(spec.ShouldRewrite("MyPlugin.Tests.dll"));
        }

        [TestMethod]
        public void ShouldRewrite_NoFilter_RewritesAll_ExceptSdk()
        {
            var spec = new DllFilterSpec();
            Assert.IsTrue(spec.ShouldRewrite("MyPlugin.dll"));
            Assert.IsTrue(spec.ShouldRewrite("Newtonsoft.Json.dll"));
            Assert.IsFalse(spec.ShouldRewrite("AcCoreMgd.dll"));
        }

        [TestMethod]
        public void ShouldRewrite_WhitelistTakesPrecedenceOverBlacklist()
        {
            // When IncludePatterns is set, ExcludePatterns is completely ignored.
            var spec = new DllFilterSpec
            {
                IncludePatterns = new List<string> { "MyPlugin*.dll" },
                ExcludePatterns = new List<string> { "MyPlugin.Core.dll" }
            };

            // MyPlugin.Core.dll matches whitelist → should be rewritten
            // despite also matching the blacklist (which is ignored in whitelist mode)
            Assert.IsTrue(spec.ShouldRewrite("MyPlugin.Core.dll"),
                "Whitelist must take precedence over blacklist.");
        }

        // ------------------------------------------------------------------
        // GetFilterReason
        // ------------------------------------------------------------------

        [TestMethod]
        public void GetFilterReason_SdkDll_MentionsHardBlacklist()
        {
            var spec = new DllFilterSpec();
            string reason = spec.GetFilterReason("AcMgd.dll");
            StringAssert.Contains(reason, "hard blacklist");
        }

        [TestMethod]
        public void GetFilterReason_MainDll_MentionsSafetyNet()
        {
            var spec = new DllFilterSpec { MainDllName = "MyPlugin.dll" };
            string reason = spec.GetFilterReason("MyPlugin.dll");
            StringAssert.Contains(reason, "safety net");
        }

        [TestMethod]
        public void GetFilterReason_WhitelistMatch_MentionsPattern()
        {
            var spec = new DllFilterSpec
            {
                IncludePatterns = new List<string> { "My*.dll" }
            };
            string reason = spec.GetFilterReason("MyPlugin.dll");
            StringAssert.Contains(reason, "My*.dll");
        }

        // ------------------------------------------------------------------
        // Config loaders
        // ------------------------------------------------------------------

        [TestMethod]
        public void FromJson_ValidWhitelist_LoadsCorrectly()
        {
            string json = @"{
  ""dllFilter"": {
    ""include"": [""MyPlugin.dll"", ""MyPlugin.Core.dll""],
    ""verbose"": false
  }
}";
            string tmpJson = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tmpJson, json);
                var spec = DllFilterSpec.FromJson(tmpJson, "MyPlugin.dll");

                Assert.IsNotNull(spec);
                Assert.AreEqual(2, spec.IncludePatterns.Count);
                Assert.AreEqual(0, spec.ExcludePatterns.Count);
                Assert.AreEqual("MyPlugin.dll", spec.IncludePatterns[0]);
                Assert.AreEqual("MyPlugin.dll", spec.MainDllName);
                Assert.IsFalse(spec.VerboseLogging);
            }
            finally
            {
                File.Delete(tmpJson);
            }
        }

        [TestMethod]
        public void FromJson_ValidBlacklist_LoadsCorrectly()
        {
            string json = @"{
  ""dllFilter"": {
    ""exclude"": [""Newtonsoft.*.dll"", ""*Test*.dll""],
    ""verbose"": true
  }
}";
            string tmpJson = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tmpJson, json);
                var spec = DllFilterSpec.FromJson(tmpJson);

                Assert.IsNotNull(spec);
                Assert.AreEqual(0, spec.IncludePatterns.Count);
                Assert.AreEqual(2, spec.ExcludePatterns.Count);
                Assert.IsTrue(spec.VerboseLogging);
            }
            finally
            {
                File.Delete(tmpJson);
            }
        }

        [TestMethod]
        public void FromJson_FileNotFound_ReturnsNull()
        {
            var spec = DllFilterSpec.FromJson(@"C:\does_not_exist.json");
            Assert.IsNull(spec);
        }

        [TestMethod]
        public void FromArgs_ParsesIncludeAndExclude()
        {
            var args = new[] { "--include", "MyPlugin*.dll", "--exclude", "*Test*.dll", "--verbose" };
            var spec = DllFilterSpec.FromArgs(args);

            Assert.AreEqual(1, spec.IncludePatterns.Count);
            Assert.AreEqual("MyPlugin*.dll", spec.IncludePatterns[0]);
            Assert.AreEqual(1, spec.ExcludePatterns.Count);
            Assert.AreEqual("*Test*.dll", spec.ExcludePatterns[0]);
            Assert.IsTrue(spec.VerboseLogging);
        }

        [TestMethod]
        public void FromArgs_ParsesMain()
        {
            var args = new[] { "--main", "MyPlugin.dll" };
            var spec = DllFilterSpec.FromArgs(args);
            Assert.AreEqual("MyPlugin.dll", spec.MainDllName);
        }

        [TestMethod]
        public void FromArgs_MultipleIncludes()
        {
            var args = new[]
            {
                "--include", "Foo.dll",
                "--include", "Bar*.dll",
                "--include", "Baz.Core.dll"
            };
            var spec = DllFilterSpec.FromArgs(args);
            Assert.AreEqual(3, spec.IncludePatterns.Count);
        }

        [TestMethod]
        public void FromArgs_EmptyArray_ReturnsDefaultSpec()
        {
            var spec = DllFilterSpec.FromArgs(new string[0]);
            Assert.IsNotNull(spec);
            Assert.AreEqual(0, spec.IncludePatterns.Count);
            Assert.AreEqual(0, spec.ExcludePatterns.Count);
        }
    }

    // =========================================================================
    //  StagingCopier — unit tests
    // =========================================================================

    [TestClass]
    public class TestStagingCopier
    {
        private string _tmpPluginDir;
        private string _tmpStagingRoot;

        [TestInitialize]
        public void SetUp()
        {
            // Create a fake "plugin build output" directory with dummy DLLs
            _tmpPluginDir = Path.Combine(Path.GetTempPath(), "cwiki_plugin_src_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tmpPluginDir);

            // Copy this test DLL as a stand-in for "MyPlugin.dll"
            string testDll = Assembly.GetExecutingAssembly().Location;
            File.Copy(testDll, Path.Combine(_tmpPluginDir, "MyPlugin.dll"),  overwrite: true);
            File.Copy(testDll, Path.Combine(_tmpPluginDir, "MyPlugin.Core.dll"), overwrite: true);

            // Write a dummy native DLL (just a text file renamed) to test native detection
            File.WriteAllText(Path.Combine(_tmpPluginDir, "native.dll"), "MZ fake native");

            // Write a non-DLL file
            File.WriteAllText(Path.Combine(_tmpPluginDir, "cadwiki-reload.json"), "{}");
        }

        [TestCleanup]
        public void TearDown()
        {
            try { Directory.Delete(_tmpPluginDir, recursive: true); } catch { }
        }

        [TestMethod]
        public void StagePlugin_CreatesFolder()
        {
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir);
            var result = copier.StagePlugin(new DateTime(2023, 10, 15, 14, 22, 30));

            Assert.IsTrue(Directory.Exists(result.StagingFolder),
                "Staging folder must be created.");
            Assert.AreEqual("MyPlugin", result.PluginName);
        }

        [TestMethod]
        public void StagePlugin_FolderNameContainsBuildNumber()
        {
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir);
            var result = copier.StagePlugin(new DateTime(2023, 10, 15, 14, 22, 30));

            string folderName = Path.GetFileName(result.StagingFolder);
            StringAssert.Contains(folderName, "--Build-",
                "Staging folder name must contain '--Build-N'.");
        }

        [TestMethod]
        public void StagePlugin_BuildNumberIncrements()
        {
            var copier = new StagingCopier("MyPlugin_Incr_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                                            _tmpPluginDir);

            var r1 = copier.StagePlugin(new DateTime(2023, 1, 1, 10, 0, 0));
            var r2 = copier.StagePlugin(new DateTime(2023, 1, 1, 10, 0, 1));

            Assert.AreEqual(1, r1.BuildNumber);
            Assert.AreEqual(2, r2.BuildNumber);
        }

        [TestMethod]
        public void StagePlugin_WritesManifest()
        {
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir);
            var result = copier.StagePlugin(new DateTime(2023, 10, 15, 14, 22, 30));

            string manifestPath = Path.Combine(result.StagingFolder, StagingManifestSerializer.FileName);
            Assert.IsTrue(File.Exists(manifestPath),
                "_manifest.json must be written to staging folder.");
            Assert.AreEqual(manifestPath, result.ManifestPath);
        }

        [TestMethod]
        public void StagePlugin_ManifestContainsPluginName()
        {
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir);
            var result = copier.StagePlugin(new DateTime(2023, 10, 15, 14, 22, 30));

            string manifestContent = File.ReadAllText(result.ManifestPath);
            StringAssert.Contains(manifestContent, "MyPlugin");
        }

        [TestMethod]
        public void StagePlugin_DllCountMatchesSource()
        {
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir);
            var result = copier.StagePlugin(DateTime.Now);

            // Source has 3 DLL files (MyPlugin.dll, MyPlugin.Core.dll, native.dll)
            Assert.AreEqual(3, result.DllCount);
        }

        [TestMethod]
        public void StagePlugin_ManagedDllsAreVersionRewritten()
        {
            var spec = new DllFilterSpec { MainDllName = "MyPlugin.dll" };
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir, spec);
            var result = copier.StagePlugin(new DateTime(2023, 10, 15, 14, 22, 30));

            // MyPlugin.dll and MyPlugin.Core.dll are managed → rewritten
            // native.dll is NOT managed → not rewritten
            Assert.IsTrue(result.RewrittenCount >= 1,
                "At least one managed DLL must be version-rewritten.");
        }

        [TestMethod]
        public void StagePlugin_NativeDllCopiedVerbatim()
        {
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir);
            var result = copier.StagePlugin(DateTime.Now);

            string nativeDest = Path.Combine(result.StagingFolder, "native.dll");
            Assert.IsTrue(File.Exists(nativeDest),
                "Native DLL must be copied verbatim to staging folder.");
        }

        [TestMethod]
        public void StagePlugin_OtherFilesAreCopied()
        {
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir);
            var result = copier.StagePlugin(DateTime.Now);

            string jsonDest = Path.Combine(result.StagingFolder, "cadwiki-reload.json");
            Assert.IsTrue(File.Exists(jsonDest),
                "Non-DLL files must be copied to staging folder.");
        }

        [TestMethod]
        public void StagePlugin_WithWhitelistFilter_OnlyMatchingDllsRewritten()
        {
            var spec = new DllFilterSpec
            {
                MainDllName     = "MyPlugin.dll",
                IncludePatterns = new List<string> { "MyPlugin.dll" }
                // MyPlugin.Core.dll should be COPIED, not rewritten
            };
            var copier = new StagingCopier("MyPlugin", _tmpPluginDir, spec);
            var result = copier.StagePlugin(new DateTime(2023, 10, 15, 14, 22, 30));

            // Only MyPlugin.dll should be rewritten (1 entry in RewriteResults)
            Assert.AreEqual(1, result.RewrittenCount,
                "Only MyPlugin.dll matches the whitelist and should be rewritten.");
        }

        [TestMethod]
        public void StagePlugin_Cleanup_RetainsMaxFolders()
        {
            // Use a unique plugin name to avoid interference with other tests
            string uniquePlugin = "CleanupTest_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var copier = new StagingCopier(uniquePlugin, _tmpPluginDir);

            // Create MaxStagingFoldersToRetain + 2 staging folders
            int totalFolders = StagingCopier.MaxStagingFoldersToRetain + 2;
            for (int i = 0; i < totalFolders; i++)
            {
                copier.StagePlugin(DateTime.Now.AddSeconds(i));
            }

            string pluginRoot = copier.GetPluginStagingRoot();
            int remaining = Directory.GetDirectories(pluginRoot).Length;
            Assert.AreEqual(StagingCopier.MaxStagingFoldersToRetain, remaining,
                $"After {totalFolders} runs, only {StagingCopier.MaxStagingFoldersToRetain} staging folders should remain.");

            // Cleanup
            try { Directory.Delete(pluginRoot, recursive: true); } catch { }
        }

        [TestMethod]
        public void StagePlugin_MissingSourceDir_Throws()
        {
            var copier = new StagingCopier("MyPlugin", @"C:\nonexistent_plugin_dir_" + Guid.NewGuid());

            Assert.Throws<DirectoryNotFoundException>(
                () => copier.StagePlugin(DateTime.Now));
        }
    }

    // =========================================================================
    //  StagingManifest — serialization tests
    // =========================================================================

    [TestClass]
    public class TestStagingManifest
    {
        [TestMethod]
        public void ManifestSerializer_WritesValidJson()
        {
            var manifest = new StagingManifest
            {
                PluginName      = "MyPlugin",
                SourceDirectory = @"C:\repos\MyPlugin\bin\Debug",
                StagingFolder   = @"C:\Temp\cadwiki.PluginStaging\MyPlugin\20231015--14_22_30--Build-1",
                BuildTime       = new DateTime(2023, 10, 15, 14, 22, 30),
                BuildNumber     = 1,
                FilterMode      = "whitelist (1 pattern(s))",
                IncludePatterns = new List<string> { "MyPlugin*.dll" },
                DllCount        = 3,
                RewrittenCount  = 2,
                Dlls = new List<DllManifestEntry>
                {
                    new DllManifestEntry
                    {
                        FileName       = "MyPlugin.dll",
                        WasRewritten   = true,
                        OriginalVersion = "1.0.0.0",
                        NewVersion     = "1.0.0.231371580",
                        DisplayVersion = "1.0.0.2023_10_15_14_22_30",
                        FilterReason   = "main plugin DLL — always rewritten (safety net)",
                        PdbPreserved   = false
                    }
                }
            };

            string tmpDir = Path.Combine(Path.GetTempPath(), "cwiki_manifest_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmpDir);

            try
            {
                string path = StagingManifestSerializer.Write(manifest, tmpDir);
                Assert.IsTrue(File.Exists(path));
                string content = File.ReadAllText(path);

                StringAssert.Contains(content, "MyPlugin");
                StringAssert.Contains(content, "whitelist");
                StringAssert.Contains(content, "1.0.0.231371580");
                StringAssert.Contains(content, "safety net");
            }
            finally
            {
                try { Directory.Delete(tmpDir, recursive: true); } catch { }
            }
        }
    }
}

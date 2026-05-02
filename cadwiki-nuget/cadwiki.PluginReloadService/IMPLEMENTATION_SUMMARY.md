# cadwiki.PluginReloadService — Implementation Summary

Branch: `release-25.0.0.0`

---

## Phase 0 — Pre-build Setup: Deterministic Version Stamping

**File: `cadwiki-nuget/build-targets/UpdateAssemblyVersionTimestamp.targets`**

A drop-in MSBuild `.targets` file that automatically rewrites the 4th segment
(revision) of `AssemblyVersion` in `AssemblyInfo.cs` **before every build**,
using the same compact-timestamp encoding as `AssemblyVersionRewriter.cs`:

```
Revision = (DaysSinceEpoch(UTC 2020-01-01) × 100_000) + SecondsOfDay(UTC)
```

**Properties:**
- Strictly monotonically increasing — every UTC second produces a larger revision
- Fully deterministic — same UTC second → same revision, always
- Timezone-invariant — always computed in UTC regardless of build machine locale
- No overflow until ~2079

**Usage — one import line in any `.csproj`:**
```xml
<Import Project="..\build-targets\UpdateAssemblyVersionTimestamp.targets" />
```

**Disable via MSBuild property:**
```bash
msbuild MyPlugin.csproj /p:UpdateAssemblyVersion=false
```

**Build output (example):**
```
[UpdateAssemblyVersionTimestamp] AssemblyInfo.cs: 4.1.0.1 → 4.1.0.231374025  (UTC 2026-05-01 20:33:45)
```

**Why this matters for external plugin developers:**
External consumers who build cadwiki-based plugins can drop this target into
their project and never worry about version collisions or manually bumping
revision numbers. Every build is guaranteed a unique, ascending version — which
is critical for NuGet package versioning, AutoCAD plugin reload detection, and
assembly binding correctness.

See `cadwiki-nuget/build-targets/README.md` for full documentation, compatibility
matrix, and configuration options.

---

## What Was Implemented

### Phase 1 — Core Engine

**New project: `cadwiki.PluginReloadService`** (`.NET 4.8 class library`)

Located at: `cadwiki-nuget/cadwiki-nuget/cadwiki.PluginReloadService/`
Project GUID: `{B2C3D4E5-F6A7-B8C9-D0E1-F2A3B4C5D6E7}`

#### 1. `PluginReloadLogger.cs`
- Structured logger writing `[HH:mm:ss.fff] [LEVEL] [Component] Message` lines.
- Writes to `%TEMP%\cadwiki.PluginStaging\{PluginName}\{timestamp}\_reload.log`.
- Thread-safe (file-lock guard).
- `Info`, `Warn`, `Error`, `Debug`, `Exception`, `Separator`, `WriteSummary` methods.
- `VerboseLogging` controls DEBUG output.
- `OpenLog()` helper on `PipelineResult` opens the log in the default app.

#### 2. `AssemblyVersionRewriter.cs`
- Rewrites `AssemblyVersion`, `AssemblyFileVersion`, and `AssemblyInformationalVersion` via **Mono.Cecil**.
- **Compact timestamp encoding:**
  ```
  Revision = (DaysSinceEpoch(2020-01-01) × 100_000) + SecondsInDay
  ```
  Strictly monotonically increasing per second; fits in `Int32` until year 2078.
- Human-readable display version: `#.#.#.YYYY_MM_DD_HH_mm_ss` stored in `AssemblyInformationalVersion`.
- Creates or updates `AssemblyFileVersionAttribute` and `AssemblyInformationalVersionAttribute` custom attributes.
- `IsManagedAssembly(path)` — PE header check via `AssemblyName.GetAssemblyName()`.
- Source files are **never modified**; output always written to a new path.
- Full error logging with actionable hints.
- Returns `RewriteResult` with original version, new version, display version, compact timestamp.

#### 3. `DllFilterSpec.cs`
- **Whitelist/blacklist/no-filter** modes.
- `ShouldRewrite(fileName)` — full precedence chain:
  1. Non-DLL → false
  2. AutoCAD SDK DLL → always false (hard blacklist)
  3. `MainDllName` → always true (safety net)
  4. Whitelist (IncludePatterns) → true iff any pattern matches
  5. Blacklist (ExcludePatterns) → false iff any pattern matches
  6. No filter → true
- `GetFilterReason(fileName)` — human-readable explanation for manifest logging.
- `GlobMatch(input, pattern)` — `*` and `?` wildcards, case-insensitive.
- **Config loaders** with priority chain:
  - `FromArgs(string[])` — CLI `--include`, `--exclude`, `--main`, `--verbose`
  - `FromJson(path)` — reads `cadwiki-reload.json` via `System.Text.Json`
  - `FromIni(path)` — reads `[DllFilter]` section via `cadwiki.NetUtils.IniFile`
  - `LoadFilterSpec(...)` — resolves the highest-priority source automatically
- **AutoCAD SDK hard blacklist** (14 DLLs, read-only):
  `AcCoreMgd.dll`, `AcCui.dll`, `AcDbMgd.dll`, `acdbmgdbrep.dll`, `AcDx.dll`,
  `AcMgd.dll`, `AcMr.dll`, `AcSeamless.dll`, `AcTcMgd.dll`, `AcWindows.dll`,
  `AdUIMgd.dll`, `AdUiPalettes.dll`, `AdWindows.dll`, `cadwiki.AcRemoveCmdGroup.dll`

#### 4. `StagingManifest.cs`
- `StagingManifest` — full metadata record for a staging run.
- `DllManifestEntry` — per-DLL record (version before/after, filter reason, PDB status).
- `StagingManifestSerializer.Write(manifest, folder)` — writes `_manifest.json`.
  - Uses **self-contained JSON builder** (no runtime dependency on System.Text.Json).
  - Produces human-readable, properly escaped JSON.

#### 5. `StagingCopier.cs`
- Staging folder path: `%TEMP%\cadwiki.PluginStaging\{PluginName}\{yyyyMMdd--HH_mm_ss}--Build-{n}\`
- Auto-increments `Build-{n}` per plugin across runs.
- For each DLL:
  - Checks managed/native via `IsManagedAssembly()`.
  - Applies `DllFilterSpec.ShouldRewrite()`.
  - Version-rewrites (managed + should rewrite) or copies verbatim.
  - Preserves `.pdb` alongside every DLL.
- Copies all non-DLL files verbatim (`.json`, `.xml`, `.config`, etc.).
- Writes `_manifest.json` after staging.
- **Cleanup:** retains last 5 staging folders per plugin, deletes older ones.
- Returns `StagingResult` with folder path, counts, rewrite results, errors list.

#### 6. `PluginReloadPipeline.cs`
- Top-level orchestration entry point.
- Bootstraps logger, resolves filter spec, creates `StagingCopier`, creates `ReloadOrchestrator`.
- Writes timing summary to log.
- Returns `PipelineResult` (never throws unhandled exceptions — all errors captured).
- `PipelineResult.OpenLog()` — opens the `_reload.log` in the default app.

---

### Phase 2 — AutoCAD Integration

#### 7. `ReloadOrchestrator.cs` (in `cadwiki.PluginReloadService`)
- Thin bridge: translates `StagingResult` → `AutoCADAppDomainDllReloader.ReloadFromStagedFolder()`.
- Handles null-document case (falls back to `Application.DocumentManager.MdiActiveDocument`).
- Full logging around the reload invocation.

#### 8. `AutoCADAppDomainDllReloader.ReloadFromStagedFolder()` (in `cadwiki.DllReloader`)
- **New method** in existing class `cadwiki.DllReloader.AutoCAD.AutoCADAppDomainDllReloader`.
- Signature: `public void ReloadFromStagedFolder(Document doc, string stagedFolderPath, string mainDllName)`
- **Zero code duplication** — delegates directly to the existing `ReloadAll()` pipeline.
- Handles the null-assembly case for external plugins.
- Sets `IExtensionApplicationClassName` from `mainDllName` if not already configured.
- `TryRemoveAllCommandsExternal()` — scans AppDomain for existing assemblies matching `mainDllName` to remove their commands before reload.

---

### Unit Tests

**Added to existing `UnitTests` project:**
`UnitTests/cadwiki.PluginReloadService/TestPluginReloadService.cs`

| Test class | Coverage |
|---|---|
| `TestAssemblyVersionRewriter` | Compact timestamp encoding (6 tests), display version formatting, `IsManagedAssembly`, round-trip rewrite |
| `TestDllFilterSpec` | SDK blacklist (2 tests), glob matching (7 tests), `ShouldRewrite` precedence (7 tests), `GetFilterReason` (3 tests), `FromJson` (3 tests), `FromArgs` (4 tests) |
| `TestStagingCopier` | Folder creation (2 tests), build number increment, manifest writing (2 tests), DLL count, managed rewrite, native copy, non-DLL copy, whitelist filter, cleanup retention, missing-dir exception |
| `TestStagingManifest` | JSON serialization round-trip |

Total: **~40 test cases** covering all core logic paths.

---

## NuGet Dependencies

Run `nuget restore cadwiki-nuget.sln` (or right-click Solution → Restore NuGet Packages in VS) before building.

| Package | Version | Purpose |
|---|---|---|
| `Mono.Cecil` | 0.11.5 | Binary assembly rewriting |
| `System.Text.Json` | 6.0.0 | `DllFilterSpec.FromJson()` parser |
| `System.Text.Encodings.Web` | 6.0.0 | Transitive dep of System.Text.Json |
| `System.Runtime.CompilerServices.Unsafe` | 6.0.0 | Transitive dep |
| `System.Memory` | 4.5.5 | Transitive dep |
| `System.Buffers` | 4.5.1 | Transitive dep |

> **Note:** `StagingManifestSerializer` uses manual JSON string building and does NOT require `System.Text.Json` at runtime for manifest output. Only `DllFilterSpec.FromJson()` requires it.

---

## Files Changed / Added

### New files (Phase 0 — build-targets)
```
build-targets/
  UpdateAssemblyVersionTimestamp.targets   ← drop-in MSBuild target for auto-versioning
  README.md                                ← usage docs, compatibility matrix, examples
```

### New files (Phases 1–2)
```
cadwiki.PluginReloadService/
  cadwiki.PluginReloadService.csproj
  packages.config
  Properties/AssemblyInfo.cs
  PluginReloadLogger.cs
  AssemblyVersionRewriter.cs
  DllFilterSpec.cs
  StagingManifest.cs
  StagingCopier.cs
  PluginReloadPipeline.cs
  ReloadOrchestrator.cs
  examples/cadwiki-reload-whitelist.json
  examples/cadwiki-reload-blacklist.json
  examples/_reload.log.sample
  IMPLEMENTATION_SUMMARY.md
  README.md

UnitTests/cadwiki.PluginReloadService/
  TestPluginReloadService.cs
```

### Modified files
```
cadwiki.DllReloader/AutoCAD/AutoCADAppDomainDllReloader.cs
  + ReloadFromStagedFolder(doc, stagedFolderPath, mainDllName)
  + TryRemoveAllCommandsExternal(doc, stagedFolderPath, mainDllName)

UnitTests/UnitTests.csproj
  + <Compile Include="cadwiki.PluginReloadService\TestPluginReloadService.cs" />
  + <ProjectReference Include="..\cadwiki.PluginReloadService\..." />

cadwiki-nuget.sln
  + Project entry for cadwiki.PluginReloadService
  + Build configuration entries (all 8 configurations)
```

---

## Known Limitations

1. **Windows-only:** `cadwiki.NetUtils.IniFile` uses Win32 P/Invoke (`kernel32.dll`).
   `DllFilterSpec.FromIni()` inherits this constraint. All other code is platform-neutral.

2. **Mono.Cecil PDB write:** The current implementation does NOT re-write PDB files for
   version-rewritten DLLs. The original PDB is copied verbatim alongside the rewritten DLL.
   This means the PDB may not exactly match the rewritten binary's debug info offsets in some
   edge cases. This is acceptable for hot-reload scenarios where full debug accuracy is not
   required. A future enhancement could use `WriterParameters { WriteSymbols = true }`.

3. **`FromJson` System.Text.Json dependency:** If the NuGet package is not restored,
   `DllFilterSpec.FromJson()` will not compile. The fallback is to use `FromArgs` or `FromIni`.
   The `StagingManifestSerializer` has no such dependency (uses manual JSON building).

4. **AutoCAD AppDomain constraint:** `ReloadFromStagedFolder` runs inside the AutoCAD AppDomain.
   Like the existing `ReloadDll` method, it cannot be called from a background thread without
   first marshalling to the AutoCAD main thread.

5. **`IsManagedAssembly` lock:** `AssemblyName.GetAssemblyName()` will throw `FileLoadException`
   if the DLL is locked by another process. The current code treats this as "managed" and
   proceeds; the subsequent Mono.Cecil read will fail with a more descriptive error.

---

## Usage Example

```csharp
// In your Ribbon button click handler (cadwiki.DllReloader project):
private void OnReloadButtonClick(object sender, RoutedEventArgs e)
{
    var doc     = Application.DocumentManager.MdiActiveDocument;
    var pipeline = new cadwiki.PluginReloadService.PluginReloadPipeline(
        pluginName:     "MyPlugin",
        sourceBuildDir: @"C:\repos\MyPlugin\bin\Debug\",
        mainDllName:    "MyPlugin.dll",
        reloader:       _reloader   // your AutoCADAppDomainDllReloader instance
    );

    var result = pipeline.Execute(doc);

    if (!result.IsSuccess)
        MessageBox.Show($"Reload failed: {result.Error?.Message}\nSee log: {result.LogFilePath}");
    else if (!result.ReloadTriggered)
        MessageBox.Show($"Staged OK but reload not triggered.\nStaged at: {result.StagingResult?.StagingFolder}");
    else
        doc.Editor.WriteMessage($"\nPlugin reloaded in {result.Elapsed.TotalMilliseconds:0}ms\n");
}
```

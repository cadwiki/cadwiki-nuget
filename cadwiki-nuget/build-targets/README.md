# cadwiki build-targets

Reusable MSBuild `.targets` files for cadwiki plugin projects.

---

## `cadwiki.VersionAutoIncrement.targets`

**Pluggable version auto-increment for AssemblyInfo.cs — replaces the inline versioning logic from buildThisFirst.csproj.**

Supports multiple strategies via a single property.  One import line per project.

### Strategies

| Strategy | Format | Description |
|---|---|---|
| `AutoIncrement` | `major.minor.MMdd.N` | N resets to 1 each day, increments per build. Reads prior version from built assembly. |
| `HardCoded` | `major.minor.build.rev` | All four segments explicitly set via properties. |
| `DateBased` | `major.minor.MMdd.HHmm` | Full date-time stamp, no prior build needed. |

### Quick start

```xml
<!-- Minimal: uses AutoIncrement strategy, discovers all AssemblyInfo.cs under BuildRoot -->
<Import Project="..\build-targets\cadwiki.VersionAutoIncrement.targets" />
```

### Configuration-driven strategy (how buildThisFirst uses it)

```xml
<!-- Map build configurations to strategies -->
<PropertyGroup Condition="'$(Configuration)' == 'DebugAuto'">
  <VersionStrategy>AutoIncrement</VersionStrategy>
</PropertyGroup>
<PropertyGroup Condition="'$(Configuration)' == 'DebugHC'">
  <VersionStrategy>HardCoded</VersionStrategy>
  <VersionMajor>4</VersionMajor>
  <VersionMinor>1</VersionMinor>
  <VersionBuild>0</VersionBuild>
  <VersionRevision>1</VersionRevision>
</PropertyGroup>
<Import Project="..\build-targets\cadwiki.VersionAutoIncrement.targets" />
```

### All configuration properties

| Property | Default | Description |
|---|---|---|
| `VersionAutoIncrementEnabled` | `true` | Master on/off switch |
| `VersionStrategy` | `AutoIncrement` | `AutoIncrement` \| `HardCoded` \| `DateBased` |
| `VersionMajor` | *(from assembly)* | Major version (required for HardCoded) |
| `VersionMinor` | *(from assembly)* | Minor version (required for HardCoded) |
| `VersionBuild` | *(computed)* | Build number (required for HardCoded) |
| `VersionRevision` | *(computed)* | Revision (required for HardCoded) |
| `VersionExtensionPackDll` | *(auto-detected)* | Path to MSBuild.ExtensionPack.dll |
| `VersionAssemblyInfoGlob` | `$(BuildRoot)\**\AssemblyInfo.cs` | Glob pattern for file discovery |
| `VersionVerbose` | `false` | Extra diagnostic logging |

### AssemblyInfo file selection

**Option A — Glob (default):** All `AssemblyInfo.cs` files under `BuildRoot`:
```xml
<Import Project="..\build-targets\cadwiki.VersionAutoIncrement.targets" />
```

**Option B — Custom glob:**
```xml
<PropertyGroup>
  <VersionAssemblyInfoGlob>$(BuildRoot)\cadwiki.*\**\AssemblyInfo.cs</VersionAssemblyInfoGlob>
</PropertyGroup>
<Import Project="..\build-targets\cadwiki.VersionAutoIncrement.targets" />
```

**Option C — Explicit file list:**
```xml
<PropertyGroup>
  <VersionAssemblyInfoGlob></VersionAssemblyInfoGlob><!-- disable glob -->
</PropertyGroup>
<ItemGroup>
  <VersionAssemblyInfoFiles Include="..\ProjectA\Properties\AssemblyInfo.cs" />
  <VersionAssemblyInfoFiles Include="..\ProjectB\Properties\AssemblyInfo.cs" />
</ItemGroup>
<Import Project="..\build-targets\cadwiki.VersionAutoIncrement.targets" />
```

### Disable from command line

```bash
msbuild MyProject.csproj /p:VersionAutoIncrementEnabled=false
```

### Adding a new versioning strategy

1. Open `cadwiki.VersionAutoIncrement.targets`
2. Add a new `<PropertyGroup Condition="'$(VersionStrategy)' == 'YourStrategy'">` block
3. Set the internal properties: `_VersionMajor`, `_VersionMinor`, `_VersionBuildNumber`, `_VersionRevision`, plus optional `_VersionBuildNumberType`, `_VersionBuildNumberFormat`, `_VersionRevisionType`, `_VersionRevisionFormat`
4. The generic AssemblyInfo task calls at the bottom handle the rest

### Requirements

- **MSBuild.Extension.Pack** NuGet package (provides the `AssemblyInfo` task)
- For `AutoIncrement` strategy: a previously-built assembly at `$(TargetPath)` (bootstrap with `HardCoded` first, or do one normal build)

---

## `UpdateAssemblyVersionTimestamp.targets`

**Auto-stamps every build with a strictly-increasing, deterministic version number.**

No external tools, no NuGet packages, no Python scripts — pure MSBuild inline C#.

### How the revision is computed

```
Revision = (DaysSinceEpoch(UTC 2020-01-01) × 100_000) + SecondsOfDay(UTC)
```

| Component | Example (2026-05-01 20:33:45 UTC) |
|---|---|
| Days since 2020-01-01 | 2313 |
| Seconds in day | 74025 (20×3600 + 33×60 + 45) |
| **Revision** | **231_374_025** |
| **Full version** | **4.1.0.231374025** |

This matches the encoding in `AssemblyVersionRewriter.cs` (PluginReloadService).

**Guarantees:**
- Same UTC second → same revision (fully deterministic)
- Later second → larger revision (strictly monotonic)
- Timezone of build machine is irrelevant (always UTC)
- No overflow until ~year 2079

---

### Usage

#### 1. Add one import line to your `.csproj`

```xml
<Project ToolsVersion="4.0" DefaultTargets="Build"
         xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

  <!-- ... your existing content ... -->

  <!-- Auto-stamp AssemblyVersion revision on every build -->
  <Import Project="..\build-targets\UpdateAssemblyVersionTimestamp.targets" />

</Project>
```

That's it. On the next build, `Properties\AssemblyInfo.cs` is updated in-place
and the new version is compiled into the output assembly.

#### 2. Your `AssemblyInfo.cs` (before build)

```csharp
[assembly: AssemblyVersion("4.1.0.1")]
[assembly: AssemblyFileVersion("4.1.0.1")]
[assembly: AssemblyInformationalVersion("4.1.0.1")]
```

#### 3. Your `AssemblyInfo.cs` (after build at 2026-05-01 20:33:45 UTC)

```csharp
[assembly: AssemblyVersion("4.1.0.231374025")]
[assembly: AssemblyFileVersion("4.1.0.1")]
[assembly: AssemblyInformationalVersion("4.1.0.1")]
```

> **Note:** Only `AssemblyVersion` is updated by this target (that is what the
> CLR uses for binding). `AssemblyFileVersion` and `AssemblyInformationalVersion`
> are left for you to manage (or use PluginReloadService's `AssemblyVersionRewriter`
> at runtime for those).

---

### Configuration properties

All properties are optional.  Override in your `.csproj` or on the MSBuild command line.

| Property | Default | Description |
|---|---|---|
| `UpdateAssemblyVersion` | `true` | Set to `false` to skip entirely |
| `UpdateAssemblyVersionInfoFile` | `Properties\AssemblyInfo.cs` | Relative path to AssemblyInfo.cs |
| `UpdateAssemblyVersionVerbose` | `false` | Log extra details (days, secs, revision) |

#### Disable from command line

```bash
msbuild MyPlugin.csproj /p:UpdateAssemblyVersion=false
```

#### Custom AssemblyInfo path

```xml
<PropertyGroup>
  <UpdateAssemblyVersionInfoFile>MyFolder\VersionInfo.cs</UpdateAssemblyVersionInfoFile>
</PropertyGroup>
<Import Project="..\build-targets\UpdateAssemblyVersionTimestamp.targets" />
```

#### Enable verbose output

```xml
<PropertyGroup>
  <UpdateAssemblyVersionVerbose>true</UpdateAssemblyVersionVerbose>
</PropertyGroup>
```

This will print:
```
[UpdateAssemblyVersionTimestamp] AssemblyInfo.cs: 4.1.0.1 → 4.1.0.231374025  (UTC 2026-05-01 20:33:45)
[UpdateAssemblyVersionTimestamp]   days_since_epoch=2313  secs_in_day=74025  revision=231374025
```

---

### Full `.csproj` example

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="Build"
         xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"
          Condition="Exists('$(MSBuildExtensionsPath)\...')" />

  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{YOUR-GUID-HERE}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>MyPlugin</RootNamespace>
    <AssemblyName>MyPlugin</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>

    <!-- Optional: disable for CI release builds that set version externally -->
    <!-- <UpdateAssemblyVersion>false</UpdateAssemblyVersion> -->
  </PropertyGroup>

  <!-- ... ItemGroups, References, etc. ... -->

  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />

  <!-- ✅ Drop-in: auto-stamps AssemblyVersion on every build -->
  <Import Project="..\build-targets\UpdateAssemblyVersionTimestamp.targets" />

</Project>
```

---

### Compatibility

| Environment | Works? | Notes |
|---|---|---|
| Visual Studio 2015–2022 (classic .csproj) | ✅ | Uses `CodeTaskFactory` |
| `dotnet build` / .NET SDK projects | ✅ | Uses `RoslynCodeTaskFactory` |
| MSBuild 14+ on build server (no VS) | ✅ | Classic factory |
| Mono / macOS / Linux | ⚠️ | Untested; `CodeTaskFactory` may not be available |
| Visual Studio 2013 or earlier | ⚠️ | `CodeTaskFactory` may be available but untested |

---

---

## `cadwiki.DevToolsDriver.targets`

**Post-build launcher for CadDevToolsDriver — test your AutoCAD addin without creating a throwaway project.**

Integrates into any existing `.csproj` with 2–3 lines. After a successful Debug build, the CadDevTools launcher UI opens with your DLL pre-staged, ready to start AutoCAD.

### Quick start

```xml
<PropertyGroup>
  <CadDevToolsEnabled>true</CadDevToolsEnabled>
</PropertyGroup>
<Import Project="..\build-targets\cadwiki.DevToolsDriver.targets" />
```

### What it does

1. **Stages** your build output to a temp folder (`%TEMP%\cadwiki.YourProject\...`)
2. **Launches** CadDevToolsDriver.exe (non-blocking, so your build completes immediately)
3. The CadDevTools UI picks up the staged DLLs and is ready to launch AutoCAD

### Configuration properties

| Property | Default | Description |
|---|---|---|
| `CadDevToolsEnabled` | `false` | Master switch — must be `true` to activate |
| `CadDevToolsConfiguration` | `Debug` | Only runs when Configuration matches this |
| `CadDevToolsDriverExe` | *(auto-detected)* | Full path to CadDevToolsDriver.exe |
| `CadDevToolsAutoCADExe` | `C:\...\AutoCAD 2025\acad.exe` | Path to acad.exe |
| `CadDevToolsStartupSwitches` | `/p VANILLA` | AutoCAD startup switches |
| `CadDevToolsDllPatterns` | `$(TargetFileName)` | Semicolon-delimited DLL wildcard patterns |
| `CadDevToolsTempSubfolder` | `cadwiki.$(MSBuildProjectName)` | Staging subfolder under `%TEMP%` |
| `CadDevToolsStaleFolderDays` | `1` | Days before stale staging folders are cleaned |
| `CadDevToolsLogFile` | `$(IntermediateOutputPath)caddevtools.log` | Log file path |
| `CadDevToolsWaitForExit` | `false` | Block build until launcher closes |

### Examples

**Custom AutoCAD version:**
```xml
<PropertyGroup>
  <CadDevToolsEnabled>true</CadDevToolsEnabled>
  <CadDevToolsAutoCADExe>C:\Program Files\Autodesk\AutoCAD 2024\acad.exe</CadDevToolsAutoCADExe>
</PropertyGroup>
<Import Project="..\build-targets\cadwiki.DevToolsDriver.targets" />
```

**Multiple DLL patterns:**
```xml
<PropertyGroup>
  <CadDevToolsEnabled>true</CadDevToolsEnabled>
  <CadDevToolsDllPatterns>*MyPlugin.dll;*MyPlugin.Core.dll</CadDevToolsDllPatterns>
</PropertyGroup>
<Import Project="..\build-targets\cadwiki.DevToolsDriver.targets" />
```

**CLI-only opt-in (no .csproj change needed):**
```bash
msbuild MyAddin.csproj /p:CadDevToolsEnabled=true
```

**Blocking mode with log capture (for CI debugging):**
```xml
<PropertyGroup>
  <CadDevToolsEnabled>true</CadDevToolsEnabled>
  <CadDevToolsWaitForExit>true</CadDevToolsWaitForExit>
</PropertyGroup>
<Import Project="..\build-targets\cadwiki.DevToolsDriver.targets" />
```

### Requirements

- **CadDevToolsDriver** project must be built first (the targets file auto-detects the exe in the solution)
- Windows only (AutoCAD is Windows-only)
- Only runs for the configured build configuration (default: Debug), so Release/CI builds are unaffected

### Disabling

```bash
msbuild MyAddin.csproj /p:CadDevToolsEnabled=false
```

Or simply don't set `CadDevToolsEnabled` to `true` — it defaults to `false`.

---

### Why not use `$(BuildId)` or GitVersion?

Those require external tooling, network access, or git history.
This target:
- works **offline**
- needs **no packages**
- is **100% deterministic** — the same build at the same UTC second produces the exact same binary
- is **a single file** — drop it in and import it

If you need `major.minor.patch` bumped intentionally, just edit `AssemblyInfo.cs` manually.
The target never touches the first three segments.

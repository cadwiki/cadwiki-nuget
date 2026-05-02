# cadwiki build-targets

Reusable MSBuild `.targets` files for cadwiki plugin projects.

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

### Why not use `$(BuildId)` or GitVersion?

Those require external tooling, network access, or git history.
This target:
- works **offline**
- needs **no packages**
- is **100% deterministic** — the same build at the same UTC second produces the exact same binary
- is **a single file** — drop it in and import it

If you need `major.minor.patch` bumped intentionally, just edit `AssemblyInfo.cs` manually.
The target never touches the first three segments.

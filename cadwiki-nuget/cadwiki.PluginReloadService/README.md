# cadwiki.PluginReloadService

Zero-touch hot-reload service for external AutoCAD C# plugins.

Eliminates the need to restart AutoCAD when iterating on a plugin. Uses binary
assembly version rewriting (via Mono.Cecil) to make each staged copy appear as
a "newer version" to the existing cadwiki AppDomain loader, which then reloads
it in-process.

## Quick Start

1. **Add `cadwiki-reload.json` to your plugin's build output directory** (optional but recommended):

```json
{
  "dllFilter": {
    "include": ["MyPlugin.dll", "MyPlugin.Core.dll"],
    "verbose": false
  }
}
```

2. **Add a reload button** to your cadwiki ribbon (in your `cadwiki.DllReloader` project):

```csharp
var pipeline = new cadwiki.PluginReloadService.PluginReloadPipeline(
    pluginName:     "MyPlugin",
    sourceBuildDir: @"C:\repos\MyPlugin\bin\Debug\",
    mainDllName:    "MyPlugin.dll",
    reloader:       myReloaderInstance
);
var result = pipeline.Execute(doc);
result.OpenLog();  // Opens _reload.log in Notepad
```

3. **Build your plugin** in Visual Studio, then click the reload button in AutoCAD.
   No AutoCAD restart required.

## How It Works

```
Build output dir           Staging folder                     AutoCAD AppDomain
─────────────────          ──────────────────────             ─────────────────
MyPlugin.dll (v1.0.0.0) → MyPlugin.dll (v1.0.0.231371580) → AppDomain.Load()
MyPlugin.Core.dll       → MyPlugin.Core.dll (rewritten)   →   (sees new version)
Newtonsoft.Json.dll     → Newtonsoft.Json.dll (verbatim)  → (skipped; same ver)
AcMgd.dll               → AcMgd.dll (verbatim)            → (SDK DLL, skipped)
```

The compact timestamp revision `(DaysSinceEpoch × 100,000) + SecondsInDay`
ensures each build always produces a strictly higher version number.

## Configuration

Filter configuration is resolved in priority order:

1. **CLI args:** `--include Pattern.dll --exclude *Test*.dll`
2. **`cadwiki-reload.json`** in the plugin's build output directory
3. **INI `[DllFilter]` section:** `Include=MyPlugin*.dll` / `Exclude=*Test*.dll`
4. **Default:** rewrite all managed DLLs except AutoCAD SDK DLLs

See `examples/` for sample configuration files.

## AutoCAD SDK DLL Blacklist

These DLLs are **never** version-rewritten regardless of configuration:

```
AcCoreMgd.dll   AcCui.dll       AcDbMgd.dll     acdbmgdbrep.dll
AcDx.dll        AcMgd.dll       AcMr.dll        AcSeamless.dll
AcTcMgd.dll     AcWindows.dll   AdUIMgd.dll     AdUiPalettes.dll
AdWindows.dll   cadwiki.AcRemoveCmdGroup.dll
```

## NuGet Setup

Run before first build:
```
nuget restore cadwiki-nuget.sln
```

Or right-click the solution in Visual Studio → **Restore NuGet Packages**.

Required packages: `Mono.Cecil 0.11.5`, `System.Text.Json 6.0.0`.

## Diagnostic Logs

Each run writes a structured log to:
```
%TEMP%\cadwiki.PluginStaging\{PluginName}\{timestamp}\_reload.log
```

And a JSON manifest to each staging folder:
```
%TEMP%\cadwiki.PluginStaging\{PluginName}\{timestamp}--Build-{n}\_manifest.json
```

Call `PipelineResult.OpenLog()` to open the log in the default text editor.

# GAoverlay

GAoverlay is a Windows .NET 8 WinForms overlay host built for Fortnite-first workflows while staying extensible for other games through plugins, per-game layouts, and file-driven adapters.

## What is in this package now

This package upgrades the original skeleton into a more complete source package with:

- GAoverlay naming applied throughout the solution
- a first-run setup workflow for install paths, launchers, icons, and preinstall options
- tooltip coverage for primary host buttons and HUD tiles
- per-game layout profiles
- window capture calibration UI
- plugin manifest auto-generation when a compatible DLL is present without `plugin.json`
- a plugin build-and-stage script for moving compiled DLLs into runtime plugin folders
- fully fleshed-out source projects for all preinstalled plugins

## Preinstalled plugin source projects

- `plugin-sdk/ExamplePlugin`
- `plugin-sdk/GAoverlay.AudioTools`
- `plugin-sdk/GAoverlay.ExtraHudZones`
- `plugin-sdk/GAoverlay.ReticleOverlay`
- `plugin-sdk/GAoverlay.ThemeTools`
- `plugin-sdk/GAoverlay.Plugin.OcrAdapter`
- `plugin-sdk/GAoverlay.Plugin.PostMatchParser`
- `plugin-sdk/GAoverlay.Plugin.EpicStatsSync`

## What the plugins actually do

- **Audio Tools** registers sample audio helper macros and optional configured launchers.
- **Extra HUD Zones** loads extra zones from plugin settings and registers them with the host.
- **Reticle Overlay** seeds reticle settings and publishes readiness markers.
- **Theme Tools** seeds theme preset settings.
- **OCR Adapter** processes JSON or TXT observation drops from `data/ocr-input` and publishes normalized live-state values such as health, shield, ammo, and kill feed.
- **Post-Match Parser** reads exported JSON or TXT summaries from `captures/post-match` and normalizes them into `data/post-match` while publishing summary live-state values.
- **Epic Stat Sync** reads manually exported JSON from `data/epic-import`, normalizes it to `data/epic-sync`, and publishes safe stat snapshot values.

These are still lawful, file-driven workflows. No magical anti-cheat-poking goblin code is being claimed here.

## Build on Windows

Install the .NET 8 SDK, then:

```powershell
dotnet build GAoverlay.sln
.\Build-And-Stage-Plugins.ps1
.\Start-Host.ps1
```

Or do it in one swing:

```powershell
.\Start-Host.ps1 -BuildFirst -StagePlugins
```

## Runtime folders

- `src/GAoverlay.Contracts` - shared interfaces and models
- `src/GAoverlay.Host` - WinForms overlay host
- `plugin-sdk` - plugin source projects
- `plugins` - runtime plugin folders for staged DLLs and manifests
- `config` - editable runtime config JSON
- `captures` - drop post-match input files here
- `data` - normalized OCR/stat/post-match output
- `samples` - sample macro scripts
- `assets` - icons and images the setup workflow can point at

## Honest limitation

I repaired and expanded the source package, but I could not run a real Windows build in this Linux container because the .NET SDK and Windows desktop runtime are not installed here. So this package is source-completed and statically checked by inspection, not locally executed in this environment.

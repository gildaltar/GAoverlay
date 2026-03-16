param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$map = @{
    "plugin-sdk\ExamplePlugin\ExamplePlugin.csproj" = "plugins\example-plugin"
    "plugin-sdk\GAoverlay.AudioTools\GAoverlay.AudioTools.csproj" = "plugins\audio-tools"
    "plugin-sdk\GAoverlay.ExtraHudZones\GAoverlay.ExtraHudZones.csproj" = "plugins\extra-hud-zones"
    "plugin-sdk\GAoverlay.ReticleOverlay\GAoverlay.ReticleOverlay.csproj" = "plugins\reticle-overlay"
    "plugin-sdk\GAoverlay.ThemeTools\GAoverlay.ThemeTools.csproj" = "plugins\theme-tools"
    "plugin-sdk\GAoverlay.Plugin.OcrAdapter\GAoverlay.Plugin.OcrAdapter.csproj" = "plugins\ocr-adapter"
    "plugin-sdk\GAoverlay.Plugin.PostMatchParser\GAoverlay.Plugin.PostMatchParser.csproj" = "plugins\post-match-parser"
    "plugin-sdk\GAoverlay.Plugin.EpicStatsSync\GAoverlay.Plugin.EpicStatsSync.csproj" = "plugins\epic-stat-sync"
}

foreach ($project in $map.Keys) {
    dotnet build $project -c $Configuration | Out-Host
    $projDir = Split-Path $project -Parent
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($project)
    $dll = Join-Path $projDir "bin\$Configuration\net8.0\$projName.dll"
    $dest = Join-Path $root $map[$project]
    New-Item -ItemType Directory -Path $dest -Force | Out-Null
    if (Test-Path $dll) {
        Copy-Item $dll $dest -Force
        Write-Host "Staged $dll -> $dest"
    }
}

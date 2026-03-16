param(
    [switch]$BuildFirst,
    [switch]$StagePlugins
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

if ($BuildFirst) {
    dotnet build .\GAoverlay.sln
}

if ($StagePlugins) {
    .\Build-And-Stage-Plugins.ps1
}

dotnet run --project .\src\GAoverlay.Host\GAoverlay.Host.csproj

namespace GAoverlay.Contracts;

public sealed class PluginManifest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string MainAssembly { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public List<string> SupportedGames { get; set; } = [];
    public List<string> Prerequisites { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string SettingsFile { get; set; } = "settings.json";
    public bool EnabledByDefault { get; set; } = true;
}

public sealed class HudZoneDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "Zone";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 220;
    public int Height { get; set; } = 78;
    public string ContentKey { get; set; } = "generic";
    public bool Visible { get; set; } = true;
    public string? ForeColor { get; set; }
    public string? BackColor { get; set; }
}

public sealed class LauncherItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

public sealed class MacroItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}

public sealed class ThemeSettings
{
    public string AccentColor { get; set; } = "#00B7FF";
    public string HudForeground { get; set; } = "#FFFFFF";
    public string HudBackground { get; set; } = "#44000000";
    public string ReticleColor { get; set; } = "#FF4040";
}

public sealed class CaptureCalibration
{
    public string GameKey { get; set; } = "fortnite";
    public string WindowTitleHint { get; set; } = "Fortnite";
    public int CaptureX { get; set; } = 0;
    public int CaptureY { get; set; } = 0;
    public int CaptureWidth { get; set; } = 1920;
    public int CaptureHeight { get; set; } = 1080;
    public int UiScalePercent { get; set; } = 100;
    public bool BorderlessWindowExpected { get; set; } = true;
    public bool Enabled { get; set; } = true;
}

public sealed class LayoutProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Default";
    public string GameKey { get; set; } = "fortnite";
    public bool IsDefault { get; set; }
    public List<HudZoneDefinition> Zones { get; set; } = [];
}

public sealed class InstallOptions
{
    public bool FirstRunCompleted { get; set; }
    public string PreferredInstallDirectory { get; set; } = string.Empty;
    public string GamesDirectory { get; set; } = string.Empty;
    public string IconsDirectory { get; set; } = string.Empty;
    public string FortniteExecutablePath { get; set; } = string.Empty;
    public string EpicLauncherPath { get; set; } = string.Empty;
    public string SteelSeriesPath { get; set; } = string.Empty;
    public bool CreateDesktopShortcut { get; set; } = true;
    public bool CreateStartMenuShortcut { get; set; } = true;
    public bool RegisterSampleMacros { get; set; } = true;
    public bool EnablePreinstalledPlugins { get; set; } = true;
}

public sealed class OverlayObservation
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PostMatchSummary
{
    public string GameKey { get; set; } = "fortnite";
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Playlist { get; set; } = string.Empty;
    public int Placement { get; set; }
    public int Kills { get; set; }
    public int Assists { get; set; }
    public int Damage { get; set; }
    public string RawSource { get; set; } = string.Empty;
}

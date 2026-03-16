using GAoverlay.Contracts;

namespace GAoverlay.Host.Services;

public sealed class HostBridge : IPluginHost
{
    private readonly ConfigService _config;
    private readonly LiveStateService _state;
    private readonly List<HudZoneDefinition> _zones;
    private readonly List<LauncherItem> _launchers;
    private readonly List<MacroItem> _macros;
    private readonly List<LayoutProfile> _layoutProfiles;

    public string BaseDirectory { get; }
    public ThemeSettings Theme { get; private set; }
    public CaptureCalibration Calibration { get; private set; }
    public InstallOptions InstallOptions { get; private set; }
    public IReadOnlyDictionary<string, object?> LiveState => _state.CurrentState;
    public IReadOnlyList<HudZoneDefinition> Zones => _zones;
    public IReadOnlyList<LauncherItem> Launchers => _launchers;
    public IReadOnlyList<MacroItem> Macros => _macros;
    public IReadOnlyList<LayoutProfile> LayoutProfiles => _layoutProfiles;

    public HostBridge(string baseDirectory, ConfigService config, LiveStateService state)
    {
        BaseDirectory = baseDirectory;
        _config = config;
        _state = state;
        _zones = _config.LoadZones();
        _launchers = _config.LoadLaunchers();
        _macros = _config.LoadMacros();
        _layoutProfiles = _config.LoadLayoutProfiles();
        Theme = _config.LoadTheme();
        Calibration = _config.LoadCalibration();
        InstallOptions = _config.LoadInstallOptions();
    }

    public void RegisterHudZone(HudZoneDefinition zone)
    {
        if (_zones.Any(z => z.Id == zone.Id || z.Title == zone.Title && z.ContentKey == zone.ContentKey))
        {
            return;
        }

        _zones.Add(zone);
        _config.SaveZones(_zones);
    }

    public void ReplaceZones(IEnumerable<HudZoneDefinition> zones)
    {
        _zones.Clear();
        _zones.AddRange(zones);
        _config.SaveZones(_zones);
    }

    public void RegisterMacro(MacroItem macro)
    {
        if (_macros.Any(m => string.Equals(m.Name, macro.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _macros.Add(macro);
        _config.SaveMacros(_macros);
    }

    public void RegisterLauncher(LauncherItem launcher)
    {
        if (_launchers.Any(l => string.Equals(l.Name, launcher.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _launchers.Add(launcher);
        _config.SaveLaunchers(_launchers);
    }

    public void UpdateTheme(ThemeSettings theme)
    {
        Theme = theme;
        _config.SaveTheme(theme);
    }

    public void SaveCalibration(CaptureCalibration calibration)
    {
        Calibration = calibration;
        _config.SaveCalibration(calibration);
    }

    public void SaveLayoutProfiles(IEnumerable<LayoutProfile> profiles)
    {
        _layoutProfiles.Clear();
        _layoutProfiles.AddRange(profiles);
        _config.SaveLayoutProfiles(_layoutProfiles);
    }

    public void SaveInstallOptions(InstallOptions options)
    {
        InstallOptions = options;
        _config.SaveInstallOptions(options);

        var summaryPath = MapPath("config", "setup-summary.txt");
        File.WriteAllText(summaryPath, string.Join(Environment.NewLine, new[]
        {
            "GAoverlay setup summary",
            $"Preferred install directory: {options.PreferredInstallDirectory}",
            $"Games directory: {options.GamesDirectory}",
            $"Icons directory: {options.IconsDirectory}",
            $"Fortnite executable: {options.FortniteExecutablePath}",
            $"Epic launcher: {options.EpicLauncherPath}",
            $"SteelSeries GG: {options.SteelSeriesPath}",
            $"Create desktop shortcut notes: {options.CreateDesktopShortcut}",
            $"Create Start menu shortcut notes: {options.CreateStartMenuShortcut}",
            $"Register sample macros: {options.RegisterSampleMacros}",
            $"Enable preinstalled plugins: {options.EnablePreinstalledPlugins}"
        }));
    }

    public void PublishLiveState(string key, object? value) => _state.Publish(key, value);
    public void PublishLiveState(IReadOnlyDictionary<string, object?> values) => _state.Publish(values);

    public string GetPluginSettingsPath(PluginManifest manifest)
    {
        var safeFileName = string.IsNullOrWhiteSpace(manifest.SettingsFile)
            ? $"{manifest.Id}.settings.json"
            : manifest.SettingsFile;

        return Path.Combine(_config.PluginSettingsDirectory, safeFileName);
    }

    public string GetPluginDirectory(PluginManifest manifest)
    {
        var folder = Path.Combine(BaseDirectory, "plugins", manifest.Id.Replace('.', '-'));
        if (Directory.Exists(folder))
        {
            return folder;
        }

        return Path.Combine(BaseDirectory, "plugins", Directory.GetDirectories(Path.Combine(BaseDirectory, "plugins"))
            .Select(Path.GetFileName)
            .FirstOrDefault(x => string.Equals(x, manifest.Id, StringComparison.OrdinalIgnoreCase)) ?? manifest.Id);
    }

    public void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");

    public string MapPath(params string[] parts)
    {
        var all = new List<string> { BaseDirectory };
        all.AddRange(parts);
        return Path.Combine(all.ToArray());
    }
}

namespace GAoverlay.Contracts;

public interface IOverlayPlugin
{
    string Id { get; }
    string Name { get; }
    void Initialize(IPluginHost host, PluginManifest manifest);
    void Start();
    void Stop();
}

public interface IPluginHost
{
    string BaseDirectory { get; }
    ThemeSettings Theme { get; }
    IReadOnlyDictionary<string, object?> LiveState { get; }
    IReadOnlyList<LayoutProfile> LayoutProfiles { get; }
    CaptureCalibration Calibration { get; }
    InstallOptions InstallOptions { get; }

    void RegisterHudZone(HudZoneDefinition zone);
    void ReplaceZones(IEnumerable<HudZoneDefinition> zones);
    void RegisterMacro(MacroItem macro);
    void RegisterLauncher(LauncherItem launcher);
    void UpdateTheme(ThemeSettings theme);
    void SaveCalibration(CaptureCalibration calibration);
    void SaveLayoutProfiles(IEnumerable<LayoutProfile> profiles);
    void SaveInstallOptions(InstallOptions options);
    void PublishLiveState(string key, object? value);
    void PublishLiveState(IReadOnlyDictionary<string, object?> values);
    string GetPluginSettingsPath(PluginManifest manifest);
    string GetPluginDirectory(PluginManifest manifest);
    void Log(string message);
    string MapPath(params string[] parts);
}

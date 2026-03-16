using System.Text.Json;
using GAoverlay.Contracts;

namespace GAoverlay.Host.Services;

public sealed class ConfigService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string ConfigDirectory { get; }
    public string PluginSettingsDirectory => Path.Combine(ConfigDirectory, "plugin-settings");
    public string ZonesPath => Path.Combine(ConfigDirectory, "zones.json");
    public string LaunchersPath => Path.Combine(ConfigDirectory, "launchers.json");
    public string MacrosPath => Path.Combine(ConfigDirectory, "macros.json");
    public string ThemePath => Path.Combine(ConfigDirectory, "theme.json");
    public string CalibrationPath => Path.Combine(ConfigDirectory, "capture-calibration.json");
    public string LayoutProfilesPath => Path.Combine(ConfigDirectory, "layout-profiles.json");
    public string InstallOptionsPath => Path.Combine(ConfigDirectory, "install-options.json");
    public string LiveStatePath => Path.Combine(ConfigDirectory, "live-state.json");

    public ConfigService(string configDirectory)
    {
        ConfigDirectory = configDirectory;
        Directory.CreateDirectory(ConfigDirectory);
        Directory.CreateDirectory(PluginSettingsDirectory);
        EnsureDefaults();
    }

    private void EnsureDefaults()
    {
        if (!File.Exists(ZonesPath))
        {
            SaveZones(GetDefaultFortniteZones());
        }

        if (!File.Exists(LaunchersPath))
        {
            SaveLaunchers(
            [
                new LauncherItem
                {
                    Name = "Fortnite",
                    Path = "com.epicgames.launcher://apps/Fortnite?action=launch&silent=true"
                },
                new LauncherItem
                {
                    Name = "Epic Games Launcher",
                    Path = @"C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe"
                },
                new LauncherItem
                {
                    Name = "SteelSeries GG",
                    Path = @"C:\Program Files\SteelSeries\GG\SteelSeriesGG.exe"
                }
            ]);
        }

        if (!File.Exists(MacrosPath))
        {
            SaveMacros(
            [
                new MacroItem { Name = "Sample AHK Macro", Path = @"samples\sample_macro.ahk" },
                new MacroItem { Name = "Sample PowerShell Macro", Path = @"samples\sample_macro.ps1" }
            ]);
        }

        if (!File.Exists(ThemePath))
        {
            SaveTheme(new ThemeSettings());
        }

        if (!File.Exists(CalibrationPath))
        {
            SaveCalibration(new CaptureCalibration());
        }

        if (!File.Exists(LayoutProfilesPath))
        {
            SaveLayoutProfiles(
            [
                new LayoutProfile
                {
                    Name = "Fortnite - Default",
                    GameKey = "fortnite",
                    IsDefault = true,
                    Zones = GetDefaultFortniteZones()
                },
                new LayoutProfile
                {
                    Name = "Valorant - Compact",
                    GameKey = "valorant",
                    Zones =
                    [
                        new HudZoneDefinition { Title = "Health", X = 20, Y = 20, ContentKey = "health" },
                        new HudZoneDefinition { Title = "Armor", X = 20, Y = 110, ContentKey = "shield" },
                        new HudZoneDefinition { Title = "Ammo", X = 20, Y = 200, ContentKey = "ammo" },
                        new HudZoneDefinition { Title = "Round", X = 260, Y = 20, ContentKey = "match" }
                    ]
                },
                new LayoutProfile
                {
                    Name = "Apex - Squad",
                    GameKey = "apex",
                    Zones =
                    [
                        new HudZoneDefinition { Title = "Health", X = 20, Y = 20, ContentKey = "health" },
                        new HudZoneDefinition { Title = "Shield", X = 20, Y = 110, ContentKey = "shield" },
                        new HudZoneDefinition { Title = "Squad", X = 260, Y = 20, Width = 260, Height = 110, ContentKey = "teammates" }
                    ]
                }
            ]);
        }

        if (!File.Exists(InstallOptionsPath))
        {
            SaveInstallOptions(new InstallOptions());
        }
    }

    public List<HudZoneDefinition> LoadZones() => Load<List<HudZoneDefinition>>(ZonesPath) ?? [];
    public void SaveZones(List<HudZoneDefinition> zones) => Save(ZonesPath, zones);

    public List<LauncherItem> LoadLaunchers() => Load<List<LauncherItem>>(LaunchersPath) ?? [];
    public void SaveLaunchers(List<LauncherItem> items) => Save(LaunchersPath, items);

    public List<MacroItem> LoadMacros() => Load<List<MacroItem>>(MacrosPath) ?? [];
    public void SaveMacros(List<MacroItem> items) => Save(MacrosPath, items);

    public ThemeSettings LoadTheme() => Load<ThemeSettings>(ThemePath) ?? new ThemeSettings();
    public void SaveTheme(ThemeSettings theme) => Save(ThemePath, theme);

    public CaptureCalibration LoadCalibration() => Load<CaptureCalibration>(CalibrationPath) ?? new CaptureCalibration();
    public void SaveCalibration(CaptureCalibration calibration) => Save(CalibrationPath, calibration);

    public List<LayoutProfile> LoadLayoutProfiles() => Load<List<LayoutProfile>>(LayoutProfilesPath) ?? [];
    public void SaveLayoutProfiles(List<LayoutProfile> profiles) => Save(LayoutProfilesPath, profiles);

    public InstallOptions LoadInstallOptions() => Load<InstallOptions>(InstallOptionsPath) ?? new InstallOptions();
    public void SaveInstallOptions(InstallOptions options) => Save(InstallOptionsPath, options);

    public T? LoadPluginSettings<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllText(filePath), _jsonOptions);
    }

    public void SavePluginSettings<T>(string filePath, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        Save(filePath, value);
    }

    private List<HudZoneDefinition> GetDefaultFortniteZones()
    {
        return
        [
            new HudZoneDefinition { Title = "Health", X = 20, Y = 20, ContentKey = "health" },
            new HudZoneDefinition { Title = "Shield", X = 20, Y = 110, ContentKey = "shield" },
            new HudZoneDefinition { Title = "Ammo", X = 20, Y = 200, ContentKey = "ammo" },
            new HudZoneDefinition { Title = "Kill Feed", X = 20, Y = 290, Width = 300, Height = 96, ContentKey = "killFeed" },
            new HudZoneDefinition { Title = "Teammates", X = 340, Y = 20, Width = 260, Height = 120, ContentKey = "teammates" },
            new HudZoneDefinition { Title = "Match", X = 340, Y = 160, Width = 260, Height = 96, ContentKey = "match" }
        ];
    }

    private T? Load<T>(string path)
    {
        if (!File.Exists(path))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), _jsonOptions);
    }

    private void Save<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ConfigDirectory);
        File.WriteAllText(path, JsonSerializer.Serialize(value, _jsonOptions));
    }
}

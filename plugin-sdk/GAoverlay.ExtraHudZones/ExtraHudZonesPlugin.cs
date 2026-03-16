using System.Text.Json;
using GAoverlay.Contracts;

namespace GAoverlay.ExtraHudZones;

public sealed class ExtraHudZonesPlugin : IOverlayPlugin
{
    private IPluginHost? _host;
    private PluginManifest? _manifest;

    public string Id => "gaoverlay.extra.hud.zones";
    public string Name => "Extra HUD Zones";

    public void Initialize(IPluginHost host, PluginManifest manifest)
    {
        _host = host;
        _manifest = manifest;
    }

    public void Start()
    {
        if (_host is null || _manifest is null)
        {
            return;
        }

        var settingsPath = _host.GetPluginSettingsPath(_manifest);
        List<HudZoneDefinition> zones;
        if (!File.Exists(settingsPath))
        {
            zones =
            [
                new HudZoneDefinition { Title = "Performance", X = 620, Y = 220, Width = 240, Height = 90, ContentKey = "performance" },
                new HudZoneDefinition { Title = "Rank", X = 620, Y = 320, Width = 240, Height = 90, ContentKey = "rank" }
            ];
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(zones, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            zones = JsonSerializer.Deserialize<List<HudZoneDefinition>>(File.ReadAllText(settingsPath)) ?? [];
        }

        foreach (var zone in zones)
        {
            _host.RegisterHudZone(zone);
        }
    }

    public void Stop()
    {
    }
}

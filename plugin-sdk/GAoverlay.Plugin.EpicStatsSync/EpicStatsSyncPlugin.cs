using System.Text.Json;
using GAoverlay.Contracts;

namespace GAoverlay.Plugin.EpicStatsSync;

public sealed class EpicStatsSyncPlugin : IOverlayPlugin
{
    private IPluginHost? _host;
    private PluginManifest? _manifest;

    public string Id => "gaoverlay.epic.stats.sync";
    public string Name => "Epic Stat Sync Plugin";

    public void Initialize(IPluginHost host, PluginManifest manifest)
    {
        _host = host;
        _manifest = manifest;
        host.Log("Initialized Epic stat sync plugin.");
    }

    public void Start()
    {
        if (_host is null || _manifest is null)
        {
            return;
        }

        var settingsPath = _host.GetPluginSettingsPath(_manifest);
        var importDir = _host.MapPath("data", "epic-import");
        var outDir = _host.MapPath("data", "epic-sync");
        Directory.CreateDirectory(importDir);
        Directory.CreateDirectory(outDir);

        if (!File.Exists(settingsPath))
        {
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(new
            {
                enabled = true,
                mode = "manual-public-flow",
                importFolder = importDir,
                outputFolder = outDir,
                notes = new[]
                {
                    "Use only approved/public endpoints, exported data, or user-authorized browser/device flows.",
                    "Do not hardcode secrets.",
                    "Do not scrape private endpoints that violate terms."
                }
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        SeedSampleIfMissing(Path.Combine(importDir, "sample-epic-stats.json"));

        foreach (var file in Directory.GetFiles(importDir, "*.json"))
        {
            try
            {
                var raw = JsonSerializer.Deserialize<Dictionary<string, object?>>(File.ReadAllText(file)) ?? [];
                var normalized = new Dictionary<string, object?>
                {
                    ["wins"] = raw.GetValueOrDefault("wins", 0),
                    ["kd"] = raw.GetValueOrDefault("kd", 0),
                    ["rank"] = raw.GetValueOrDefault("rank", "Unranked"),
                    ["statSync"] = "updated"
                };

                _host.PublishLiveState(normalized);
                File.WriteAllText(Path.Combine(outDir, Path.GetFileName(file)), JsonSerializer.Serialize(normalized, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                _host.Log($"Epic stat sync failed for '{file}': {ex.Message}");
            }
        }

        _host.RegisterHudZone(new HudZoneDefinition
        {
            Title = "Stat Sync",
            X = 620,
            Y = 124,
            Width = 260,
            Height = 88,
            ContentKey = "statSync"
        });
    }

    private static void SeedSampleIfMissing(string path)
    {
        if (File.Exists(path))
        {
            return;
        }

        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            wins = 12,
            kd = 2.35,
            rank = "Gold"
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Stop()
    {
    }
}

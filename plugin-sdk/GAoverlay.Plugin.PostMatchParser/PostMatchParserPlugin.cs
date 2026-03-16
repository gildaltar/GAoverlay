using System.Text.Json;
using GAoverlay.Contracts;

namespace GAoverlay.Plugin.PostMatchParser;

public sealed class PostMatchParserPlugin : IOverlayPlugin
{
    private IPluginHost? _host;
    private PluginManifest? _manifest;

    public string Id => "gaoverlay.post.match.parser";
    public string Name => "Post-Match Parser Plugin";

    public void Initialize(IPluginHost host, PluginManifest manifest)
    {
        _host = host;
        _manifest = manifest;
        host.Log("Initialized post-match parser plugin.");
    }

    public void Start()
    {
        if (_host is null || _manifest is null)
        {
            return;
        }

        var watchDir = _host.MapPath("captures", "post-match");
        var outDir = _host.MapPath("data", "post-match");
        Directory.CreateDirectory(watchDir);
        Directory.CreateDirectory(outDir);

        var settingsPath = _host.GetPluginSettingsPath(_manifest);
        if (!File.Exists(settingsPath))
        {
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(new
            {
                watchFolder = watchDir,
                outputFolder = outDir,
                filePatterns = new[] { "*.json", "*.txt" },
                notes = "Drop exported summaries or OCR text dumps here and the plugin normalizes them."
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        SeedSampleIfMissing(Path.Combine(watchDir, "sample-summary.txt"));

        foreach (var file in Directory.GetFiles(watchDir))
        {
            var summary = TryParse(file);
            if (summary is null)
            {
                continue;
            }

            var outPath = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(file)}.normalized.json");
            File.WriteAllText(outPath, JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));
            _host.PublishLiveState(new Dictionary<string, object?>
            {
                ["match"] = $"P{summary.Placement} K{summary.Kills} Dmg {summary.Damage}",
                ["lastPlaylist"] = summary.Playlist,
                ["lastPlacement"] = summary.Placement
            });
        }
    }

    private static PostMatchSummary? TryParse(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext == ".json")
        {
            return JsonSerializer.Deserialize<PostMatchSummary>(File.ReadAllText(path));
        }

        if (ext == ".txt")
        {
            var map = File.ReadAllLines(path)
                .Select(line => line.Split('=', 2, StringSplitOptions.TrimEntries))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

            return new PostMatchSummary
            {
                GameKey = map.GetValueOrDefault("gameKey", "fortnite"),
                Playlist = map.GetValueOrDefault("playlist", string.Empty),
                Placement = int.TryParse(map.GetValueOrDefault("placement", "0"), out var placement) ? placement : 0,
                Kills = int.TryParse(map.GetValueOrDefault("kills", "0"), out var kills) ? kills : 0,
                Assists = int.TryParse(map.GetValueOrDefault("assists", "0"), out var assists) ? assists : 0,
                Damage = int.TryParse(map.GetValueOrDefault("damage", "0"), out var damage) ? damage : 0,
                RawSource = path
            };
        }

        return null;
    }

    private static void SeedSampleIfMissing(string path)
    {
        if (File.Exists(path))
        {
            return;
        }

        File.WriteAllLines(path, new[]
        {
            "gameKey=fortnite",
            "playlist=Zero Build",
            "placement=7",
            "kills=4",
            "assists=1",
            "damage=1024"
        });
    }

    public void Stop()
    {
    }
}

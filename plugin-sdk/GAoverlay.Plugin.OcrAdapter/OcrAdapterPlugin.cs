using System.Text.Json;
using GAoverlay.Contracts;

namespace GAoverlay.Plugin.OcrAdapter;

public sealed class OcrAdapterPlugin : IOverlayPlugin
{
    private IPluginHost? _host;
    private PluginManifest? _manifest;

    public string Id => "gaoverlay.ocr.adapter";
    public string Name => "OCR Adapter Plugin";

    public void Initialize(IPluginHost host, PluginManifest manifest)
    {
        _host = host;
        _manifest = manifest;
        host.Log($"Initialized OCR adapter for: {string.Join(", ", manifest.SupportedGames)}");
    }

    public void Start()
    {
        if (_host is null || _manifest is null)
        {
            return;
        }

        _host.RegisterHudZone(new HudZoneDefinition
        {
            Title = "OCR Status",
            X = 620,
            Y = 20,
            Width = 260,
            Height = 88,
            ContentKey = "ocrStatus"
        });

        var settingsPath = _host.GetPluginSettingsPath(_manifest);
        var watchDir = _host.MapPath("data", "ocr-input");
        var outDir = _host.MapPath("data", "ocr-output");
        Directory.CreateDirectory(watchDir);
        Directory.CreateDirectory(outDir);

        if (!File.Exists(settingsPath))
        {
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(new
            {
                enabled = true,
                watchFolder = watchDir,
                outputFolder = outDir,
                targets = new[] { "health", "shield", "ammo", "killFeed" },
                acceptedFormats = new[] { ".json", ".txt" },
                ocrEngine = "Tesseract-compatible"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        SeedSampleIfMissing(Path.Combine(watchDir, "sample-ocr.json"));
        ProcessInputs(watchDir, outDir);
    }

    private void ProcessInputs(string watchDir, string outDir)
    {
        if (_host is null)
        {
            return;
        }

        foreach (var file in Directory.GetFiles(watchDir))
        {
            try
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                Dictionary<string, object?> values = ext switch
                {
                    ".json" => JsonSerializer.Deserialize<Dictionary<string, object?>>(File.ReadAllText(file)) ?? new Dictionary<string, object?>(),
                    ".txt" => ParseText(File.ReadAllLines(file)),
                    _ => []
                };

                if (values.Count == 0)
                {
                    continue;
                }

                values["ocrStatus"] = $"processed {Path.GetFileName(file)}";
                _host.PublishLiveState(values);
                File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(file) + ".normalized.json"), JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                _host.Log($"OCR adapter failed for '{file}': {ex.Message}");
            }
        }
    }

    private static Dictionary<string, object?> ParseText(IEnumerable<string> lines)
    {
        var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in lines)
        {
            var parts = raw.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                data[parts[0]] = parts[1];
            }
        }
        return data;
    }

    private static void SeedSampleIfMissing(string path)
    {
        if (File.Exists(path))
        {
            return;
        }

        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            health = 100,
            shield = 50,
            ammo = 30,
            killFeed = "You eliminated Sample Bot"
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Stop()
    {
    }
}

using System.Text.Json;
using GAoverlay.Contracts;

namespace GAoverlay.ThemeTools;

public sealed class ThemeToolsPlugin : IOverlayPlugin
{
    private IPluginHost? _host;
    private PluginManifest? _manifest;

    public string Id => "gaoverlay.theme.tools";
    public string Name => "Theme Tools";

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
        if (!File.Exists(settingsPath))
        {
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(new
            {
                presets = new[]
                {
                    new { name = "Default Cyan", accentColor = "#00B7FF", hudForeground = "#FFFFFF", hudBackground = "#44000000", reticleColor = "#FF4040" },
                    new { name = "Steel Gold", accentColor = "#FFB612", hudForeground = "#FFFFFF", hudBackground = "#55000000", reticleColor = "#FFFFFF" }
                }
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        _host.PublishLiveState("themePlugin", "ready");
    }

    public void Stop()
    {
    }
}

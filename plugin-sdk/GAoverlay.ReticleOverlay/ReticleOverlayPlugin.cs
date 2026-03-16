using System.Text.Json;
using GAoverlay.Contracts;

namespace GAoverlay.ReticleOverlay;

public sealed class ReticleOverlayPlugin : IOverlayPlugin
{
    private IPluginHost? _host;
    private PluginManifest? _manifest;

    public string Id => "gaoverlay.reticle.overlay";
    public string Name => "Reticle Overlay";

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
                enabled = true,
                reticleColor = _host.Theme.ReticleColor,
                style = "crosshair"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        _host.PublishLiveState("reticlePlugin", "loaded");
    }

    public void Stop()
    {
    }
}

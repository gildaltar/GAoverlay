using GAoverlay.Contracts;

namespace ExamplePlugin;

public sealed class ExamplePlugin : IOverlayPlugin
{
    private IPluginHost? _host;

    public string Id => "example.plugin";
    public string Name => "Example Plugin";

    public void Initialize(IPluginHost host, PluginManifest manifest)
    {
        _host = host;
        host.Log($"Initialized {manifest.Name}");
    }

    public void Start()
    {
        if (_host is null)
        {
            return;
        }

        _host.RegisterHudZone(new HudZoneDefinition
        {
            Title = "Example Zone",
            X = 600,
            Y = 40,
            Width = 220,
            Height = 90,
            ContentKey = "match"
        });
    }

    public void Stop()
    {
    }
}

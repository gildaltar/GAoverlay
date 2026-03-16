using System.Text.Json;
using GAoverlay.Contracts;

namespace GAoverlay.AudioTools;

public sealed class AudioToolsPlugin : IOverlayPlugin
{
    private IPluginHost? _host;
    private PluginManifest? _manifest;

    public string Id => "gaoverlay.audio.tools";
    public string Name => "Audio Tools";

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
                addMuteMicMacro = true,
                addMuteHeadsetMacro = true,
                registerSteelSeriesLauncher = true
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        _host.RegisterMacro(new MacroItem { Name = "Mute Mic Helper", Path = @"samples\sample_macro.ps1", Arguments = "-Action MuteMic" });
        _host.RegisterMacro(new MacroItem { Name = "Mute Headset Helper", Path = @"samples\sample_macro.ps1", Arguments = "-Action MuteHeadset" });

        if (!string.IsNullOrWhiteSpace(_host.InstallOptions.SteelSeriesPath))
        {
            _host.RegisterLauncher(new LauncherItem { Name = "SteelSeries GG (Configured)", Path = _host.InstallOptions.SteelSeriesPath });
        }

        _host.PublishLiveState("audioStatus", "ready");
    }

    public void Stop()
    {
    }
}

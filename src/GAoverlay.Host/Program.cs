using GAoverlay.Host.Services;

namespace GAoverlay.Host;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var baseDir = AppContext.BaseDirectory;
        var configDir = Path.Combine(baseDir, "config");
        var pluginDir = Path.Combine(baseDir, "plugins");

        Directory.CreateDirectory(configDir);
        Directory.CreateDirectory(pluginDir);

        var stateService = new LiveStateService(Path.Combine(configDir, "live-state.json"));
        var configService = new ConfigService(configDir);
        var hostBridge = new HostBridge(baseDir, configService, stateService);
        var pluginLoader = new PluginLoader(pluginDir, hostBridge);
        pluginLoader.LoadAll();

        using var mainForm = new MainForm(configService, stateService, pluginLoader, hostBridge);
        Application.Run(mainForm);
    }
}

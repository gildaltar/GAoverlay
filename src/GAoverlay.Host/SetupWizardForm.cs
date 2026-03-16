using GAoverlay.Contracts;

namespace GAoverlay.Host;

public sealed class SetupWizardForm : Form
{
    private readonly string _accentHex;
    private readonly TextBox _installDirectory = new() { Dock = DockStyle.Top, PlaceholderText = @"C:\Games\GAoverlay" };
    private readonly TextBox _gamesDirectory = new() { Dock = DockStyle.Top, PlaceholderText = @"C:\Program Files" };
    private readonly TextBox _iconsDirectory = new() { Dock = DockStyle.Top, PlaceholderText = @"C:\Games\GAoverlay\assets" };
    private readonly TextBox _fortnitePath = new() { Dock = DockStyle.Top, PlaceholderText = "com.epicgames.launcher://apps/Fortnite?action=launch&silent=true" };
    private readonly TextBox _epicPath = new() { Dock = DockStyle.Top, PlaceholderText = @"C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe" };
    private readonly TextBox _steelSeriesPath = new() { Dock = DockStyle.Top, PlaceholderText = @"C:\Program Files\SteelSeries\GG\SteelSeriesGG.exe" };
    private readonly CheckBox _desktopShortcut = new() { AutoSize = true, Text = "Create desktop shortcut notes" };
    private readonly CheckBox _startMenuShortcut = new() { AutoSize = true, Text = "Create Start menu shortcut notes" };
    private readonly CheckBox _sampleMacros = new() { AutoSize = true, Text = "Register sample macros" };
    private readonly CheckBox _enablePlugins = new() { AutoSize = true, Text = "Enable preinstalled plugins" };

    public InstallOptions Options { get; private set; }

    public SetupWizardForm(InstallOptions current, string accentHex)
    {
        _accentHex = string.IsNullOrWhiteSpace(accentHex) ? HostUi.DefaultAccentHex : accentHex;
        Text = "GAoverlay Setup";
        Width = 780;
        Height = 720;
        MinimumSize = new Size(760, 680);
        StartPosition = FormStartPosition.CenterParent;
        HostUi.ApplyDialogChrome(this);

        Options = current;
        _installDirectory.Text = current.PreferredInstallDirectory;
        _gamesDirectory.Text = current.GamesDirectory;
        _iconsDirectory.Text = current.IconsDirectory;
        _fortnitePath.Text = current.FortniteExecutablePath;
        _epicPath.Text = current.EpicLauncherPath;
        _steelSeriesPath.Text = current.SteelSeriesPath;
        _desktopShortcut.Checked = current.CreateDesktopShortcut;
        _startMenuShortcut.Checked = current.CreateStartMenuShortcut;
        _sampleMacros.Checked = current.RegisterSampleMacros;
        _enablePlugins.Checked = current.EnablePreinstalledPlugins;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = BackColor
        };
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle());

        var bodyHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20, 0, 20, 20),
            BackColor = BackColor
        };

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 5
        };

        body.Controls.Add(HostUi.CreateHeroPanel(
            _accentHex,
            "FIRST RUN",
            "Set up the quiet surface first. Discover the deeper tools right after.",
            "This should take about two minutes. Start with paths and the recommended toggles, then return to the overlay to explore launchers, profiles, macros, and plugin-powered extras at your own pace."), 0, 0);
        body.Controls.Add(HostUi.CreateSectionPanel(
            "Start here",
            "These paths unlock the calm, everyday setup. You can leave anything blank and come back later.",
            HostUi.CreateFieldBlock("Install directory", "Where you want GAoverlay related files and notes to live.", _installDirectory),
            HostUi.CreateFieldBlock("Games directory", "Helpful when you usually keep launchers and game executables in one place.", _gamesDirectory),
            HostUi.CreateFieldBlock("Icons and image assets", "Used for tray icons, window icons, and plugin artwork when you add them.", _iconsDirectory)), 0, 1);
        body.Controls.Add(HostUi.CreateSectionPanel(
            "Optional integrations",
            "Point GAoverlay at the tools you expect to launch most often. Leave these empty if you would rather wire them later.",
            HostUi.CreateFieldBlock("Fortnite executable or URI", "Accepts a direct EXE path or an Epic launcher URI.", _fortnitePath),
            HostUi.CreateFieldBlock("Epic Games Launcher", "Used by the Launchers action when you want one-click access.", _epicPath),
            HostUi.CreateFieldBlock("SteelSeries GG", "Optional helper if part of your gaming setup runs through GG.", _steelSeriesPath)), 0, 2);
        body.Controls.Add(HostUi.CreateSectionPanel(
            "Recommended switches",
            "These defaults aim for fast value without making the app feel overconfigured.",
            HostUi.CreateCheckBlock(_enablePlugins, "Keep the preinstalled plugins available so the app has more depth from the start."),
            HostUi.CreateCheckBlock(_sampleMacros, "Seed a couple of sample macros so you can inspect how the workflow is wired."),
            HostUi.CreateCheckBlock(_desktopShortcut, "Write shortcut guidance into the setup summary without pretending the app installed anything silently."),
            HostUi.CreateCheckBlock(_startMenuShortcut, "Do the same for Start menu placement notes.")), 0, 3);
        body.Controls.Add(HostUi.CreateSectionPanel(
            "What happens next",
            "Save to write config only. Nothing is secretly installed. After this, the fastest path to the first 'whoa' moment is opening Profiles, toggling plugins, and seeing how much sits behind one quiet overlay.",
            HostUi.CreateCallout("GAoverlay writes a plain-text setup summary to config/setup-summary.txt so you always know what was captured.", _accentHex)), 0, 4);

        bodyHost.Controls.Add(body);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(20, 12, 20, 20),
            Height = 72,
            BackColor = HostUi.Surface
        };
        var save = new Button { Text = "Save and continue", AutoSize = true };
        var skip = new Button { Text = "Skip for now", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true };
        HostUi.StyleButton(save, _accentHex, primary: true);
        HostUi.StyleButton(skip, _accentHex, primary: false);
        HostUi.StyleButton(cancel, _accentHex, primary: false);
        save.Click += (_, _) =>
        {
            Options = new InstallOptions
            {
                FirstRunCompleted = true,
                PreferredInstallDirectory = _installDirectory.Text.Trim(),
                GamesDirectory = _gamesDirectory.Text.Trim(),
                IconsDirectory = _iconsDirectory.Text.Trim(),
                FortniteExecutablePath = _fortnitePath.Text.Trim(),
                EpicLauncherPath = _epicPath.Text.Trim(),
                SteelSeriesPath = _steelSeriesPath.Text.Trim(),
                CreateDesktopShortcut = _desktopShortcut.Checked,
                CreateStartMenuShortcut = _startMenuShortcut.Checked,
                RegisterSampleMacros = _sampleMacros.Checked,
                EnablePreinstalledPlugins = _enablePlugins.Checked
            };
            DialogResult = DialogResult.OK;
            Close();
        };
        skip.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        footer.Controls.Add(save);
        footer.Controls.Add(skip);
        footer.Controls.Add(cancel);

        root.Controls.Add(HostUi.CreateTopBand(_accentHex), 0, 0);
        root.Controls.Add(bodyHost, 0, 1);
        root.Controls.Add(footer, 0, 2);

        Controls.Add(root);
        AcceptButton = save;
        CancelButton = cancel;
    }
}

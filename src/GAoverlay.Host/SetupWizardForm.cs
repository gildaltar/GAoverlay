using GAoverlay.Contracts;

namespace GAoverlay.Host;

public sealed class SetupWizardForm : Form
{
    private readonly TextBox _installDirectory = new() { Dock = DockStyle.Top };
    private readonly TextBox _gamesDirectory = new() { Dock = DockStyle.Top };
    private readonly TextBox _iconsDirectory = new() { Dock = DockStyle.Top };
    private readonly TextBox _fortnitePath = new() { Dock = DockStyle.Top };
    private readonly TextBox _epicPath = new() { Dock = DockStyle.Top };
    private readonly TextBox _steelSeriesPath = new() { Dock = DockStyle.Top };
    private readonly CheckBox _desktopShortcut = new() { Dock = DockStyle.Top, Text = "Create desktop shortcut notes" };
    private readonly CheckBox _startMenuShortcut = new() { Dock = DockStyle.Top, Text = "Create Start menu shortcut notes" };
    private readonly CheckBox _sampleMacros = new() { Dock = DockStyle.Top, Text = "Register sample macros" };
    private readonly CheckBox _enablePlugins = new() { Dock = DockStyle.Top, Text = "Enable preinstalled plugins" };

    public InstallOptions Options { get; private set; }

    public SetupWizardForm(InstallOptions current)
    {
        Text = "GAoverlay Setup Workflow";
        Width = 560;
        Height = 580;
        StartPosition = FormStartPosition.CenterParent;

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

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            AutoScroll = true,
            ColumnCount = 1,
            RowCount = 20
        };

        body.Controls.Add(new Label { Text = "Recommended install directory", AutoSize = true });
        body.Controls.Add(_installDirectory);
        body.Controls.Add(new Label { Text = "Games directory (where your launchers/exes usually live)", AutoSize = true });
        body.Controls.Add(_gamesDirectory);
        body.Controls.Add(new Label { Text = "Icons/images directory for tray icon, window icon, plugin icons", AutoSize = true });
        body.Controls.Add(_iconsDirectory);
        body.Controls.Add(new Label { Text = "Fortnite executable or URI", AutoSize = true });
        body.Controls.Add(_fortnitePath);
        body.Controls.Add(new Label { Text = "Epic Games Launcher path", AutoSize = true });
        body.Controls.Add(_epicPath);
        body.Controls.Add(new Label { Text = "SteelSeries GG path", AutoSize = true });
        body.Controls.Add(_steelSeriesPath);
        body.Controls.Add(_desktopShortcut);
        body.Controls.Add(_startMenuShortcut);
        body.Controls.Add(_sampleMacros);
        body.Controls.Add(_enablePlugins);
        body.Controls.Add(new Label
        {
            Text = "This workflow writes config only. Shortcut creation is documented into config/setup-summary.txt so the app never pretends it secretly installed things behind your back.",
            AutoSize = true,
            MaximumSize = new Size(500, 0)
        });

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42 };
        var save = new Button { Text = "Save", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true };
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
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        footer.Controls.Add(save);
        footer.Controls.Add(cancel);

        Controls.Add(body);
        Controls.Add(footer);
    }
}

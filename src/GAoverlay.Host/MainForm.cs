using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using GAoverlay.Contracts;
using GAoverlay.Host.Services;

namespace GAoverlay.Host;

public sealed class MainForm : Form
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x20;
    private const int WsExLayered = 0x80000;

    private readonly ConfigService _config;
    private readonly LiveStateService _state;
    private readonly PluginLoader _plugins;
    private readonly HostBridge _host;
    private readonly NotifyIcon _notifyIcon = new();
    private readonly FlowLayoutPanel _zonePanel = new() { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true };
    private readonly Panel _reticlePanel = new() { Width = 32, Height = 32, BackColor = Color.Transparent, Visible = false };
    private readonly ToolTip _toolTips = new() { AutomaticDelay = 180, AutoPopDelay = 12000, ReshowDelay = 120 };
    private readonly Panel _onboardingPanel = new() { Dock = DockStyle.Top, Height = 170, Padding = new Padding(0), Visible = false };
    private readonly Panel _onboardingAccentBar = new() { Dock = DockStyle.Left, Width = 5 };
    private readonly Label _onboardingEyebrow = new() { AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly Label _onboardingTitle = new() { AutoSize = true, MaximumSize = new Size(760, 0), Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold) };
    private readonly Label _onboardingBody = new() { AutoSize = true, MaximumSize = new Size(760, 0), Font = new Font("Segoe UI", 9.5f, FontStyle.Regular) };
    private readonly FlowLayoutPanel _onboardingActions = new() { AutoSize = true, WrapContents = true, Margin = new Padding(0, 16, 0, 0) };
    private readonly Button _onboardingPrimaryButton = new() { AutoSize = true };
    private readonly Button _onboardingSecondaryButton = new() { AutoSize = true };
    private readonly Button _onboardingTertiaryButton = new() { AutoSize = true };

    private bool _clickThroughEnabled;
    private bool _onboardingDismissedForSession;
    private Action? _onboardingPrimaryAction;
    private Action? _onboardingSecondaryAction;
    private Action? _onboardingTertiaryAction;

    public MainForm(ConfigService config, LiveStateService state, PluginLoader plugins, HostBridge host)
    {
        _config = config;
        _state = state;
        _plugins = plugins;
        _host = host;

        Text = "GAoverlay";
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = true;
        BackColor = Color.LimeGreen;
        TransparencyKey = Color.LimeGreen;
        WindowState = FormWindowState.Maximized;

        var topBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 54,
            BackColor = HostUi.ToolbarOverlay,
            Padding = new Padding(6)
        };

        AddButton(topBar, "Click-Through", "Make the overlay ignore mouse input so your game keeps focus.", (_, _) => ToggleClickThrough());
        AddButton(topBar, "Plugins", "Inspect loaded plugin manifests and load status.", (_, _) => new PluginManagerForm(_plugins, _host.Theme.AccentColor).ShowDialog(this));
        AddButton(topBar, "Launchers", "Run configured launchers and helpers.", (_, _) => ShowLaunchers());
        AddButton(topBar, "Macros", "Run configured macro files.", (_, _) => ShowMacros());
        AddButton(topBar, "Theme", "Pick an accent color for the overlay.", (_, _) => EditTheme());
        AddButton(topBar, "Calibrate", "Adjust capture region and window hints.", (_, _) => ShowCalibration());
        AddButton(topBar, "Profiles", "Edit per-game layout profiles and zone sets.", (_, _) => ShowProfiles());
        AddButton(topBar, "Setup", "First-run workflow for install location, shortcuts, game paths, and icons.", (_, _) => ShowSetupWizard());
        AddButton(topBar, "Reticle", "Toggle a simple center reticle overlay.", (_, _) => _reticlePanel.Visible = !_reticlePanel.Visible);
        AddButton(topBar, "Hide", "Hide the overlay to the tray icon.", (_, _) => Hide());

        BuildOnboardingPanel();
        ApplyOnboardingTheme();

        Controls.Add(_zonePanel);
        Controls.Add(_onboardingPanel);
        Controls.Add(topBar);
        Controls.Add(_reticlePanel);

        _reticlePanel.Paint += (_, e) =>
        {
            using var pen = new Pen(ParseColor(_host.Theme.ReticleColor), 2);
            e.Graphics.DrawLine(pen, 16, 0, 16, 10);
            e.Graphics.DrawLine(pen, 16, 22, 16, 32);
            e.Graphics.DrawLine(pen, 0, 16, 10, 16);
            e.Graphics.DrawLine(pen, 22, 16, 32, 16);
        };

        Resize += (_, _) => CenterReticle();
        Load += (_, _) =>
        {
            CenterReticle();
            RenderZones();
            RefreshOnboardingSurface();
        };

        _notifyIcon.Visible = true;
        _notifyIcon.Text = "GAoverlay";
        _notifyIcon.Icon = SystemIcons.Application;
        _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
        _notifyIcon.ContextMenuStrip.Items.Add("Show", null, (_, _) =>
        {
            Show();
            WindowState = FormWindowState.Maximized;
            Activate();
        });
        _notifyIcon.ContextMenuStrip.Items.Add("Toggle Click-Through", null, (_, _) => ToggleClickThrough());
        _notifyIcon.ContextMenuStrip.Items.Add("Setup", null, (_, _) => ShowSetupWizard());
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (_, _) => Close());
        _notifyIcon.DoubleClick += (_, _) =>
        {
            Show();
            Activate();
        };

        _state.StateChanged += () =>
        {
            if (IsHandleCreated)
            {
                BeginInvoke(() =>
                {
                    RenderZones();
                    RefreshOnboardingSurface();
                });
            }
        };
    }

    private void AddButton(Control container, string text, string toolTip, EventHandler click)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(6)
        };
        HostUi.StyleButton(button, _host.Theme.AccentColor, primary: false);
        button.Click += click;
        _toolTips.SetToolTip(button, toolTip);
        container.Controls.Add(button);
    }

    private void CenterReticle() => _reticlePanel.Location = new Point((Width / 2) - 16, (Height / 2) - 16);

    private void RenderZones()
    {
        _zonePanel.SuspendLayout();
        _zonePanel.Controls.Clear();

        foreach (var zone in _host.Zones.Where(z => z.Visible))
        {
            var label = new Label
            {
                Width = zone.Width,
                Height = zone.Height,
                Text = $"{zone.Title}{Environment.NewLine}{ResolveValue(zone.ContentKey)}",
                ForeColor = ParseColor(zone.ForeColor ?? _host.Theme.HudForeground),
                BackColor = ParseColor(zone.BackColor ?? _host.Theme.HudBackground),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8),
                Margin = new Padding(12)
            };

            _toolTips.SetToolTip(label, $"{zone.Title} [{zone.ContentKey}] at {zone.X},{zone.Y} size {zone.Width}x{zone.Height}");
            _zonePanel.Controls.Add(label);
        }

        _zonePanel.ResumeLayout();
        _reticlePanel.Invalidate();
        RefreshOnboardingSurface();
    }

    private string ResolveValue(string key)
    {
        return _state.CurrentState.TryGetValue(key, out var value)
            ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
            : "n/a";
    }

    private static Color ParseColor(string hex) => HostUi.ParseColor(hex, Color.White);

    private void ToggleClickThrough()
    {
        _clickThroughEnabled = !_clickThroughEnabled;
        var exStyle = GetWindowLong(Handle, GwlExStyle);

        if (_clickThroughEnabled)
        {
            SetWindowLong(Handle, GwlExStyle, exStyle | WsExTransparent | WsExLayered);
            Opacity = 0.92;
        }
        else
        {
            SetWindowLong(Handle, GwlExStyle, exStyle & ~WsExTransparent);
            Opacity = 1.0;
        }
    }

    private void ShowLaunchers()
    {
        foreach (var launcher in _host.Launchers)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = launcher.Path,
                    Arguments = launcher.Arguments,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _host.Log($"Launcher failed for '{launcher.Name}': {ex.Message}");
            }
        }
    }

    private void ShowMacros()
    {
        foreach (var macro in _host.Macros.Where(m => m.Enabled))
        {
            try
            {
                var fullPath = Path.IsPathRooted(macro.Path)
                    ? macro.Path
                    : Path.Combine(_host.BaseDirectory, macro.Path);

                var ext = Path.GetExtension(fullPath).ToLowerInvariant();

                if (ext == ".ps1")
                {
                    var args = $"-ExecutionPolicy Bypass -File \"{fullPath}\"";
                    Process.Start(new ProcessStartInfo("powershell.exe", args) { UseShellExecute = true });
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullPath,
                        Arguments = macro.Arguments,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                _host.Log($"Macro failed for '{macro.Name}': {ex.Message}");
            }
        }
    }

    private void EditTheme()
    {
        using var dialog = new ColorDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var c = dialog.Color;
        _host.UpdateTheme(new ThemeSettings
        {
            AccentColor = $"#{c.R:X2}{c.G:X2}{c.B:X2}",
            HudForeground = _host.Theme.HudForeground,
            HudBackground = _host.Theme.HudBackground,
            ReticleColor = _host.Theme.ReticleColor
        });

        ApplyOnboardingTheme();
        RenderZones();
    }

    private void ShowCalibration()
    {
        using var form = new CaptureCalibrationForm(_host.Calibration, _host.Theme.AccentColor);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _host.SaveCalibration(form.Calibration);
            _host.Log($"Saved calibration for game '{form.Calibration.GameKey}'.");
        }
    }

    private void ShowProfiles()
    {
        using var form = new LayoutProfilesForm(_host.LayoutProfiles, _host.Theme.AccentColor);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _host.SaveLayoutProfiles(form.Profiles);

        var preferred = form.Profiles.FirstOrDefault(p => p.IsDefault)
            ?? form.Profiles.FirstOrDefault(p => string.Equals(p.GameKey, _host.Calibration.GameKey, StringComparison.OrdinalIgnoreCase))
            ?? form.Profiles.FirstOrDefault();

        if (preferred is not null)
        {
            _host.ReplaceZones(preferred.Zones);
            RenderZones();
        }
    }

    private void ShowSetupWizard()
    {
        using var form = new SetupWizardForm(_host.InstallOptions, _host.Theme.AccentColor);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _host.SaveInstallOptions(form.Options);
        _onboardingDismissedForSession = false;

        if (form.Options.RegisterSampleMacros)
        {
            _host.RegisterMacro(new MacroItem { Name = "Sample AHK Macro", Path = @"samples\sample_macro.ahk" });
            _host.RegisterMacro(new MacroItem { Name = "Sample PowerShell Macro", Path = @"samples\sample_macro.ps1" });
        }

        if (!string.IsNullOrWhiteSpace(form.Options.FortniteExecutablePath))
        {
            _host.RegisterLauncher(new LauncherItem { Name = "Fortnite (Configured)", Path = form.Options.FortniteExecutablePath });
        }

        if (!string.IsNullOrWhiteSpace(form.Options.EpicLauncherPath))
        {
            _host.RegisterLauncher(new LauncherItem { Name = "Epic Games Launcher (Configured)", Path = form.Options.EpicLauncherPath });
        }

        RefreshOnboardingSurface();
    }

    private void BuildOnboardingPanel()
    {
        _onboardingPanel.BackColor = HostUi.Surface;

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            ColumnCount = 1,
            Padding = new Padding(18, 16, 18, 16)
        };

        _onboardingEyebrow.Margin = Padding.Empty;
        _onboardingTitle.Margin = new Padding(0, 6, 0, 0);
        _onboardingBody.Margin = new Padding(0, 10, 0, 0);

        StyleOnboardingButton(_onboardingPrimaryButton, primary: true);
        StyleOnboardingButton(_onboardingSecondaryButton, primary: false);
        StyleOnboardingButton(_onboardingTertiaryButton, primary: false);

        _onboardingPrimaryButton.Click += (_, _) => _onboardingPrimaryAction?.Invoke();
        _onboardingSecondaryButton.Click += (_, _) => _onboardingSecondaryAction?.Invoke();
        _onboardingTertiaryButton.Click += (_, _) => _onboardingTertiaryAction?.Invoke();

        _onboardingActions.Controls.Add(_onboardingPrimaryButton);
        _onboardingActions.Controls.Add(_onboardingSecondaryButton);
        _onboardingActions.Controls.Add(_onboardingTertiaryButton);

        content.Controls.Add(_onboardingEyebrow);
        content.Controls.Add(_onboardingTitle);
        content.Controls.Add(_onboardingBody);
        content.Controls.Add(_onboardingActions);

        _onboardingPanel.Controls.Add(content);
        _onboardingPanel.Controls.Add(_onboardingAccentBar);
    }

    private void StyleOnboardingButton(Button button, bool primary)
    {
        HostUi.StyleButton(button, _host.Theme.AccentColor, primary);
        button.Margin = new Padding(0, 0, 10, 0);
    }

    private void ApplyOnboardingTheme()
    {
        var accent = ParseColor(_host.Theme.AccentColor);
        _onboardingPanel.BackColor = HostUi.Surface;
        _onboardingAccentBar.BackColor = accent;
        _onboardingEyebrow.ForeColor = accent;
        _onboardingTitle.ForeColor = HostUi.TextPrimary;
        _onboardingBody.ForeColor = HostUi.TextSecondary;
        _onboardingPrimaryButton.BackColor = accent;
    }

    private void RefreshOnboardingSurface()
    {
        if (_onboardingDismissedForSession)
        {
            _onboardingPanel.Visible = false;
            return;
        }

        if (!_host.InstallOptions.FirstRunCompleted)
        {
            ConfigureOnboarding(
                "FIRST RUN",
                "One quiet overlay. More depth than it first lets on.",
                "Start with the essentials in about two minutes. GAoverlay stays calm on the surface, then opens into launchers, macros, profiles, and plugin-powered extras when you are ready.",
                ("Start setup", () => ShowSetupWizard()),
                ("Open profiles", () => ShowProfiles()),
                ("Hide for now", () =>
                {
                    _onboardingDismissedForSession = true;
                    RefreshOnboardingSurface();
                }));
            return;
        }

        if (_state.CurrentState.Count == 0)
        {
            ConfigureOnboarding(
                "LIVE FEED WAITING",
                "The overlay is loaded. It just needs a data source.",
                "Your layout is ready, and the n/a values will disappear as soon as a plugin publishes state. Inspect Plugins to see what is staged, or revisit Setup if you want faster launch access first.",
                ("Open plugins", () => new PluginManagerForm(_plugins, _host.Theme.AccentColor).ShowDialog(this)),
                ("Open setup", () => ShowSetupWizard()),
                ("Dismiss", () =>
                {
                    _onboardingDismissedForSession = true;
                    RefreshOnboardingSurface();
                }));
            return;
        }

        _onboardingPanel.Visible = false;
    }

    private void ConfigureOnboarding(
        string eyebrow,
        string title,
        string body,
        (string Text, Action Handler) primary,
        (string Text, Action Handler) secondary,
        (string Text, Action Handler) tertiary)
    {
        _onboardingEyebrow.Text = eyebrow;
        _onboardingTitle.Text = title;
        _onboardingBody.Text = body;
        ApplyOnboardingTheme();
        SetOnboardingAction(_onboardingPrimaryButton, primary.Text, primary.Handler, ref _onboardingPrimaryAction);
        SetOnboardingAction(_onboardingSecondaryButton, secondary.Text, secondary.Handler, ref _onboardingSecondaryAction);
        SetOnboardingAction(_onboardingTertiaryButton, tertiary.Text, tertiary.Handler, ref _onboardingTertiaryAction);
        _onboardingPanel.Visible = true;
    }

    private static void SetOnboardingAction(Button button, string text, Action handler, ref Action? target)
    {
        button.Text = text;
        button.Visible = true;
        target = handler;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _notifyIcon.Visible = false;
        base.OnFormClosing(e);
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}

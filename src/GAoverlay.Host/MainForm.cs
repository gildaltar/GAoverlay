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
    private readonly ToolTip _toolTips = new();

    private bool _clickThroughEnabled;

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
            Height = 46,
            BackColor = Color.FromArgb(72, 0, 0, 0),
            Padding = new Padding(6)
        };

        AddButton(topBar, "Click-Through", "Make the overlay ignore mouse input so your game keeps focus.", (_, _) => ToggleClickThrough());
        AddButton(topBar, "Plugins", "Inspect loaded plugin manifests and load status.", (_, _) => new PluginManagerForm(_plugins).ShowDialog(this));
        AddButton(topBar, "Launchers", "Run configured launchers and helpers.", (_, _) => ShowLaunchers());
        AddButton(topBar, "Macros", "Run configured macro files.", (_, _) => ShowMacros());
        AddButton(topBar, "Theme", "Pick an accent color for the overlay.", (_, _) => EditTheme());
        AddButton(topBar, "Calibrate", "Adjust capture region and window hints.", (_, _) => ShowCalibration());
        AddButton(topBar, "Profiles", "Edit per-game layout profiles and zone sets.", (_, _) => ShowProfiles());
        AddButton(topBar, "Setup", "First-run workflow for install location, shortcuts, game paths, and icons.", (_, _) => ShowSetupWizard());
        AddButton(topBar, "Reticle", "Toggle a simple center reticle overlay.", (_, _) => _reticlePanel.Visible = !_reticlePanel.Visible);
        AddButton(topBar, "Hide", "Hide the overlay to the tray icon.", (_, _) => Hide());

        Controls.Add(_zonePanel);
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
            if (!_host.InstallOptions.FirstRunCompleted)
            {
                ShowSetupWizard();
            }
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
                BeginInvoke(RenderZones);
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
    }

    private string ResolveValue(string key)
    {
        return _state.CurrentState.TryGetValue(key, out var value)
            ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
            : "n/a";
    }

    private static Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');

        if (hex.Length == 8)
        {
            return Color.FromArgb(
                Convert.ToInt32(hex[..2], 16),
                Convert.ToInt32(hex[2..4], 16),
                Convert.ToInt32(hex[4..6], 16),
                Convert.ToInt32(hex[6..8], 16));
        }

        if (hex.Length == 6)
        {
            return Color.FromArgb(
                Convert.ToInt32(hex[..2], 16),
                Convert.ToInt32(hex[2..4], 16),
                Convert.ToInt32(hex[4..6], 16));
        }

        return Color.White;
    }

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

        RenderZones();
    }

    private void ShowCalibration()
    {
        using var form = new CaptureCalibrationForm(_host.Calibration);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _host.SaveCalibration(form.Calibration);
            _host.Log($"Saved calibration for game '{form.Calibration.GameKey}'.");
        }
    }

    private void ShowProfiles()
    {
        using var form = new LayoutProfilesForm(_host.LayoutProfiles);
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
        using var form = new SetupWizardForm(_host.InstallOptions);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _host.SaveInstallOptions(form.Options);

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

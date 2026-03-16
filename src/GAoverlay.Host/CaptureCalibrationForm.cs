using GAoverlay.Contracts;

namespace GAoverlay.Host;

public sealed class CaptureCalibrationForm : Form
{
    private readonly string _accentHex;
    private readonly TextBox _gameKey = new() { Dock = DockStyle.Top };
    private readonly TextBox _windowTitle = new() { Dock = DockStyle.Top };
    private readonly NumericUpDown _x = new() { Dock = DockStyle.Top, Maximum = 10000 };
    private readonly NumericUpDown _y = new() { Dock = DockStyle.Top, Maximum = 10000 };
    private readonly NumericUpDown _width = new() { Dock = DockStyle.Top, Maximum = 10000, Minimum = 1 };
    private readonly NumericUpDown _height = new() { Dock = DockStyle.Top, Maximum = 10000, Minimum = 1 };
    private readonly NumericUpDown _uiScale = new() { Dock = DockStyle.Top, Maximum = 400, Minimum = 25 };
    private readonly CheckBox _borderless = new() { AutoSize = true, Text = "Borderless / windowed expectation" };
    private readonly CheckBox _enabled = new() { AutoSize = true, Text = "Calibration enabled" };

    public CaptureCalibration Calibration { get; private set; }

    public CaptureCalibrationForm(CaptureCalibration current, string accentHex)
    {
        _accentHex = string.IsNullOrWhiteSpace(accentHex) ? HostUi.DefaultAccentHex : accentHex;
        Text = "Window Capture Calibration";
        Width = 720;
        Height = 650;
        MinimumSize = new Size(680, 600);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        HostUi.ApplyDialogChrome(this);

        Calibration = current;

        _gameKey.Text = current.GameKey;
        _windowTitle.Text = current.WindowTitleHint;
        _x.Value = current.CaptureX;
        _y.Value = current.CaptureY;
        _width.Value = current.CaptureWidth;
        _height.Value = current.CaptureHeight;
        _uiScale.Value = current.UiScalePercent;
        _borderless.Checked = current.BorderlessWindowExpected;
        _enabled.Checked = current.Enabled;

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
            ColumnCount = 1
        };

        body.Controls.Add(HostUi.CreateHeroPanel(
            _accentHex,
            "CAPTURE SETUP",
            "Tune the overlay against the game window without guesswork.",
            "Use this when the capture region, window title, or expected UI scale changes. The goal is a stable, trustworthy read of the game surface rather than a noisy setup ritual."), 0, 0);
        body.Controls.Add(HostUi.CreateSectionPanel(
            "Target window",
            "Identify the game and the title hint GAoverlay should look for when pairing to a running window.",
            HostUi.CreateFieldBlock("Game key", "A short identifier like fortnite, apex, or valorant.", _gameKey),
            HostUi.CreateFieldBlock("Window title hint", "Helpful when the overlay needs a human-readable clue for the expected window.", _windowTitle)), 0, 1);
        body.Controls.Add(HostUi.CreateSectionPanel(
            "Capture region",
            "Dial in the exact rectangle the overlay should consider. These values are easier to trust when they are explicit.",
            HostUi.CreateFieldBlock("Capture X", "Horizontal starting point in screen pixels.", _x),
            HostUi.CreateFieldBlock("Capture Y", "Vertical starting point in screen pixels.", _y),
            HostUi.CreateFieldBlock("Capture width", "Visible width of the region that matters.", _width),
            HostUi.CreateFieldBlock("Capture height", "Visible height of the region that matters.", _height),
            HostUi.CreateFieldBlock("UI scale %", "Match the game's interface scale so downstream plugins interpret the frame correctly.", _uiScale),
            HostUi.CreateCheckBlock(_borderless, "Keep this enabled when the game is expected to run in borderless or windowed mode."),
            HostUi.CreateCheckBlock(_enabled, "Turn this off only when you want the saved profile to exist without actively driving capture.")), 0, 2);
        body.Controls.Add(HostUi.CreateSectionPanel(
            "Confidence check",
            "If capture feels off, start by confirming the title hint, then verify the X/Y origin before adjusting size. Small errors here ripple into everything downstream.",
            HostUi.CreateCallout("Calibration writes explicit values so you can revisit and compare them later instead of relying on guesswork.", _accentHex)), 0, 3);

        bodyHost.Controls.Add(body);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(20, 12, 20, 20),
            Height = 72,
            BackColor = HostUi.Surface
        };
        var save = new Button { Text = "Save calibration", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true };
        HostUi.StyleButton(save, _accentHex, primary: true);
        HostUi.StyleButton(cancel, _accentHex, primary: false);
        save.Click += (_, _) =>
        {
            Calibration = new CaptureCalibration
            {
                GameKey = _gameKey.Text.Trim(),
                WindowTitleHint = _windowTitle.Text.Trim(),
                CaptureX = (int)_x.Value,
                CaptureY = (int)_y.Value,
                CaptureWidth = (int)_width.Value,
                CaptureHeight = (int)_height.Value,
                UiScalePercent = (int)_uiScale.Value,
                BorderlessWindowExpected = _borderless.Checked,
                Enabled = _enabled.Checked
            };
            DialogResult = DialogResult.OK;
            Close();
        };
        cancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        footer.Controls.Add(save);
        footer.Controls.Add(cancel);

        root.Controls.Add(HostUi.CreateTopBand(_accentHex), 0, 0);
        root.Controls.Add(bodyHost, 0, 1);
        root.Controls.Add(footer, 0, 2);

        Controls.Add(root);
        AcceptButton = save;
        CancelButton = cancel;
    }
}

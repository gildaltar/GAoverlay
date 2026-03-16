using GAoverlay.Contracts;

namespace GAoverlay.Host;

public sealed class CaptureCalibrationForm : Form
{
    private readonly TextBox _gameKey = new() { Dock = DockStyle.Top };
    private readonly TextBox _windowTitle = new() { Dock = DockStyle.Top };
    private readonly NumericUpDown _x = new() { Dock = DockStyle.Top, Maximum = 10000 };
    private readonly NumericUpDown _y = new() { Dock = DockStyle.Top, Maximum = 10000 };
    private readonly NumericUpDown _width = new() { Dock = DockStyle.Top, Maximum = 10000, Minimum = 1 };
    private readonly NumericUpDown _height = new() { Dock = DockStyle.Top, Maximum = 10000, Minimum = 1 };
    private readonly NumericUpDown _uiScale = new() { Dock = DockStyle.Top, Maximum = 400, Minimum = 25 };
    private readonly CheckBox _borderless = new() { Dock = DockStyle.Top, Text = "Borderless / windowed expectation" };
    private readonly CheckBox _enabled = new() { Dock = DockStyle.Top, Text = "Calibration enabled" };

    public CaptureCalibration Calibration { get; private set; }

    public CaptureCalibrationForm(CaptureCalibration current)
    {
        Text = "Window Capture Calibration";
        Width = 420;
        Height = 420;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

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

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 16,
            Padding = new Padding(12),
            AutoScroll = true
        };

        panel.Controls.Add(new Label { Text = "Game Key", AutoSize = true });
        panel.Controls.Add(_gameKey);
        panel.Controls.Add(new Label { Text = "Window Title Hint", AutoSize = true });
        panel.Controls.Add(_windowTitle);
        panel.Controls.Add(new Label { Text = "Capture X", AutoSize = true });
        panel.Controls.Add(_x);
        panel.Controls.Add(new Label { Text = "Capture Y", AutoSize = true });
        panel.Controls.Add(_y);
        panel.Controls.Add(new Label { Text = "Capture Width", AutoSize = true });
        panel.Controls.Add(_width);
        panel.Controls.Add(new Label { Text = "Capture Height", AutoSize = true });
        panel.Controls.Add(_height);
        panel.Controls.Add(new Label { Text = "UI Scale %", AutoSize = true });
        panel.Controls.Add(_uiScale);
        panel.Controls.Add(_borderless);
        panel.Controls.Add(_enabled);

        var buttonBar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42 };
        var save = new Button { Text = "Save", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true };
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
        buttonBar.Controls.Add(save);
        buttonBar.Controls.Add(cancel);

        Controls.Add(panel);
        Controls.Add(buttonBar);
    }
}

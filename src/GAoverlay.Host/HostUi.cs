using System.Drawing;

namespace GAoverlay.Host;

internal static class HostUi
{
    public const string DefaultAccentHex = "#6FB0D8";

    public static readonly Color WindowBackground = Color.FromArgb(16, 20, 28);
    public static readonly Color Surface = Color.FromArgb(22, 28, 38);
    public static readonly Color SurfaceElevated = Color.FromArgb(24, 31, 44);
    public static readonly Color SurfaceInset = Color.FromArgb(14, 18, 26);
    public static readonly Color TextPrimary = Color.FromArgb(228, 233, 242);
    public static readonly Color TextSecondary = Color.FromArgb(180, 189, 204);
    public static readonly Color TextMuted = Color.FromArgb(164, 173, 189);
    public static readonly Color Border = Color.FromArgb(64, 88, 120);
    public static readonly Color BorderSoft = Color.FromArgb(80, 94, 118);
    public static readonly Color ToolbarOverlay = Color.FromArgb(84, 12, 18, 26);

    public static void ApplyDialogChrome(Form form)
    {
        form.BackColor = WindowBackground;
        form.ForeColor = TextPrimary;
        form.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
    }

    public static Color ParseColor(string? hex, Color? fallback = null)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return fallback ?? TextPrimary;
        }

        hex = hex.TrimStart('#');

        try
        {
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
        }
        catch
        {
            // Fall back to the normalized palette when a config value is malformed.
        }

        return fallback ?? TextPrimary;
    }

    public static Color GetReadableForeground(Color background)
    {
        var luminance = (background.R * 0.299) + (background.G * 0.587) + (background.B * 0.114);
        return luminance > 148 ? Color.FromArgb(8, 14, 20) : TextPrimary;
    }

    public static Control CreateTopBand(string accentHex)
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            Height = 18,
            BackColor = ParseColor(accentHex, ParseColor(DefaultAccentHex))
        };
    }

    public static Control CreateHeroPanel(string accentHex, string eyebrow, string title, string body)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 160,
            Padding = new Padding(24, 24, 24, 20),
            Margin = new Padding(0, 20, 0, 18),
            BackColor = SurfaceElevated
        };

        var accent = ParseColor(accentHex, ParseColor(DefaultAccentHex));
        var eyebrowLabel = new Label
        {
            AutoSize = true,
            Text = eyebrow,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = accent,
            Location = new Point(24, 20)
        };
        var titleLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(760, 0),
            Text = title,
            Font = new Font("Segoe UI Semibold", 17f, FontStyle.Bold),
            ForeColor = TextPrimary,
            Location = new Point(24, 46)
        };
        var bodyLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(760, 0),
            Text = body,
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            ForeColor = TextSecondary,
            Location = new Point(24, 92)
        };

        panel.Controls.Add(eyebrowLabel);
        panel.Controls.Add(titleLabel);
        panel.Controls.Add(bodyLabel);
        return panel;
    }

    public static Control CreateSectionPanel(string title, string description, params Control[] content)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(20),
            Margin = new Padding(0, 0, 0, 16),
            BackColor = Surface
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1
        };

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Text = title,
            Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
            ForeColor = TextPrimary
        });
        layout.Controls.Add(new Label
        {
            AutoSize = true,
            MaximumSize = new Size(760, 0),
            Margin = new Padding(0, 6, 0, 14),
            Text = description,
            Font = new Font("Segoe UI", 9.25f, FontStyle.Regular),
            ForeColor = TextSecondary
        });

        foreach (var control in content)
        {
            layout.Controls.Add(control);
        }

        panel.Controls.Add(layout);
        return panel;
    }

    public static Control CreateFieldBlock(string label, string helpText, Control input, int width = 680)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 92,
            Margin = new Padding(0, 0, 0, 10)
        };

        var title = new Label
        {
            AutoSize = true,
            Text = label,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = TextPrimary,
            Location = new Point(0, 0)
        };
        var help = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(width, 0),
            Text = helpText,
            Font = new Font("Segoe UI", 8.75f, FontStyle.Regular),
            ForeColor = TextMuted,
            Location = new Point(0, 22)
        };

        input.Location = new Point(0, 56);
        input.Width = width;
        input.Margin = Padding.Empty;
        input.Dock = DockStyle.None;
        input.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        StyleInput(input);

        panel.Controls.Add(title);
        panel.Controls.Add(help);
        panel.Controls.Add(input);
        return panel;
    }

    public static Control CreateCheckBlock(CheckBox checkBox, string helpText, int width = 650)
    {
        StyleCheckBox(checkBox);

        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 58,
            Margin = new Padding(0, 0, 0, 10)
        };

        checkBox.Location = new Point(0, 0);
        var help = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(width, 0),
            Text = helpText,
            Font = new Font("Segoe UI", 8.75f, FontStyle.Regular),
            ForeColor = TextMuted,
            Location = new Point(4, 24)
        };

        panel.Controls.Add(checkBox);
        panel.Controls.Add(help);
        return panel;
    }

    public static Control CreateCallout(string text, string accentHex)
    {
        return new Label
        {
            AutoSize = true,
            MaximumSize = new Size(760, 0),
            Text = text,
            Font = new Font("Segoe UI", 9f, FontStyle.Italic),
            ForeColor = ParseColor(accentHex, ParseColor(DefaultAccentHex))
        };
    }

    public static void StyleToolbar(FlowLayoutPanel toolbar)
    {
        toolbar.BackColor = Surface;
        toolbar.Padding = new Padding(12, 10, 12, 10);
        toolbar.WrapContents = true;
        toolbar.AutoSize = true;
    }

    public static void StyleButton(Button button, string accentHex, bool primary)
    {
        var accent = ParseColor(accentHex, ParseColor(DefaultAccentHex));
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = BorderSoft;
        button.Padding = new Padding(14, 8, 14, 8);
        button.Margin = new Padding(10, 0, 0, 0);
        button.BackColor = primary ? accent : Surface;
        button.ForeColor = primary ? GetReadableForeground(accent) : TextPrimary;
        button.UseVisualStyleBackColor = false;
    }

    public static void StyleCheckBox(CheckBox checkBox)
    {
        checkBox.ForeColor = TextPrimary;
        checkBox.BackColor = Color.Transparent;
        checkBox.Margin = new Padding(0);
    }

    public static void StyleInput(Control input)
    {
        switch (input)
        {
            case TextBox textBox:
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.BackColor = SurfaceInset;
                textBox.ForeColor = TextPrimary;
                break;
            case NumericUpDown numericUpDown:
                numericUpDown.BorderStyle = BorderStyle.FixedSingle;
                numericUpDown.BackColor = SurfaceInset;
                numericUpDown.ForeColor = TextPrimary;
                break;
        }
    }

    public static void StyleListBox(ListBox listBox)
    {
        listBox.BackColor = SurfaceInset;
        listBox.ForeColor = TextPrimary;
        listBox.BorderStyle = BorderStyle.FixedSingle;
        listBox.IntegralHeight = false;
    }

    public static void StylePropertyGrid(PropertyGrid propertyGrid)
    {
        propertyGrid.ViewBackColor = SurfaceInset;
        propertyGrid.ViewForeColor = TextPrimary;
        propertyGrid.HelpBackColor = Surface;
        propertyGrid.HelpForeColor = TextSecondary;
        propertyGrid.CommandsBackColor = Surface;
        propertyGrid.CommandsForeColor = TextSecondary;
        propertyGrid.LineColor = BorderSoft;
        propertyGrid.CategoryForeColor = TextPrimary;
        propertyGrid.DisabledItemForeColor = TextMuted;
    }

    public static void StyleDataGridView(DataGridView grid, string accentHex)
    {
        var accent = ParseColor(accentHex, ParseColor(DefaultAccentHex));
        grid.BackgroundColor = SurfaceInset;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = BorderSoft;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Surface;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SurfaceElevated;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.DefaultCellStyle.BackColor = SurfaceInset;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, accent.R, accent.G, accent.B);
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
    }
}

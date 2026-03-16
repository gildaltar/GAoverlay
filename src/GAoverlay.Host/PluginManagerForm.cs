using GAoverlay.Host.Services;

namespace GAoverlay.Host;

public sealed class PluginManagerForm : Form
{
    public PluginManagerForm(PluginLoader loader, string accentHex)
    {
        var resolvedAccent = string.IsNullOrWhiteSpace(accentHex) ? HostUi.DefaultAccentHex : accentHex;

        Text = "Plugin Manager";
        Width = 1080;
        Height = 680;
        MinimumSize = new Size(960, 600);
        StartPosition = FormStartPosition.CenterParent;
        HostUi.ApplyDialogChrome(this);

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false
        };
        HostUi.StyleDataGridView(grid, resolvedAccent);
        grid.Height = 430;

        grid.DataSource = loader.Loaded.Select(x => new
        {
            x.Manifest.Id,
            x.Manifest.Name,
            x.Manifest.Author,
            x.Manifest.Version,
            x.Manifest.Category,
            SupportedGames = string.Join(", ", x.Manifest.SupportedGames),
            Permissions = string.Join(", ", x.Manifest.Permissions),
            x.Status
        }).ToList();

        var summary = HostUi.CreateSectionPanel(
            "Runtime plugins",
            "This is the fast trust check for what GAoverlay has loaded, who authored it, and what surface area it claims. It should feel operational, not mysterious.",
            grid);
        summary.Dock = DockStyle.Fill;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = BackColor
        };
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle());

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(20, 12, 20, 20),
            Height = 72,
            BackColor = HostUi.Surface
        };
        var closeButton = new Button { Text = "Close", AutoSize = true };
        HostUi.StyleButton(closeButton, resolvedAccent, primary: false);
        closeButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        footer.Controls.Add(closeButton);

        root.Controls.Add(HostUi.CreateTopBand(resolvedAccent), 0, 0);
        root.Controls.Add(HostUi.CreateHeroPanel(
            resolvedAccent,
            "PLUGIN STACK",
            "See the quiet engine behind the overlay.",
            "This view is where advanced capability becomes concrete. A future beginner should still be able to understand what is loaded without feeling dumped into a wall of internals."), 0, 1);
        root.Controls.Add(summary, 0, 2);
        root.Controls.Add(footer, 0, 3);

        Controls.Add(root);
        CancelButton = closeButton;
    }
}

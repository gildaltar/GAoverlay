using GAoverlay.Host.Services;

namespace GAoverlay.Host;

public sealed class PluginManagerForm : Form
{
    public PluginManagerForm(PluginLoader loader)
    {
        Text = "Plugin Manager";
        Width = 980;
        Height = 520;
        StartPosition = FormStartPosition.CenterParent;

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false
        };

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

        Controls.Add(grid);
    }
}

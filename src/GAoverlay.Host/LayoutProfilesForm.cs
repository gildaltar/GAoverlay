using GAoverlay.Contracts;

namespace GAoverlay.Host;

public sealed class LayoutProfilesForm : Form
{
    private readonly ListBox _list = new() { Dock = DockStyle.Left, Width = 240 };
    private readonly PropertyGrid _grid = new() { Dock = DockStyle.Fill };
    private readonly List<LayoutProfile> _profiles;

    public List<LayoutProfile> Profiles => _profiles;

    public LayoutProfilesForm(IEnumerable<LayoutProfile> profiles)
    {
        Text = "Per-Game Layout Profiles";
        Width = 960;
        Height = 560;
        StartPosition = FormStartPosition.CenterParent;

        _profiles = profiles.Select(Clone).ToList();

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
        var addButton = new Button { Text = "Add Profile", AutoSize = true };
        var duplicateButton = new Button { Text = "Duplicate", AutoSize = true };
        var deleteButton = new Button { Text = "Delete", AutoSize = true };
        var applyButton = new Button { Text = "Save", AutoSize = true };

        addButton.Click += (_, _) =>
        {
            var profile = new LayoutProfile
            {
                Name = "New Profile",
                GameKey = "custom"
            };
            _profiles.Add(profile);
            Rebind();
            _list.SelectedItem = profile;
        };

        duplicateButton.Click += (_, _) =>
        {
            if (_list.SelectedItem is not LayoutProfile selected)
            {
                return;
            }

            var clone = Clone(selected);
            clone.Id = Guid.NewGuid().ToString("N");
            clone.Name = $"{clone.Name} Copy";
            clone.IsDefault = false;
            _profiles.Add(clone);
            Rebind();
            _list.SelectedItem = clone;
        };

        deleteButton.Click += (_, _) =>
        {
            if (_list.SelectedItem is not LayoutProfile selected)
            {
                return;
            }

            _profiles.Remove(selected);
            Rebind();
        };

        applyButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        toolbar.Controls.Add(addButton);
        toolbar.Controls.Add(duplicateButton);
        toolbar.Controls.Add(deleteButton);
        toolbar.Controls.Add(applyButton);

        _list.DisplayMember = nameof(LayoutProfile.Name);
        _list.SelectedIndexChanged += (_, _) => _grid.SelectedObject = _list.SelectedItem;
        _grid.HelpVisible = true;
        _grid.ToolbarVisible = false;

        Controls.Add(_grid);
        Controls.Add(_list);
        Controls.Add(toolbar);

        Rebind();
    }

    private void Rebind()
    {
        _list.DataSource = null;
        _list.DataSource = _profiles;
        _list.DisplayMember = nameof(LayoutProfile.Name);
        if (_profiles.Count > 0 && _list.SelectedIndex < 0)
        {
            _list.SelectedIndex = 0;
        }
    }

    private static LayoutProfile Clone(LayoutProfile source)
    {
        return new LayoutProfile
        {
            Id = source.Id,
            Name = source.Name,
            GameKey = source.GameKey,
            IsDefault = source.IsDefault,
            Zones = source.Zones
                .Select(z => new HudZoneDefinition
                {
                    Id = z.Id,
                    Title = z.Title,
                    X = z.X,
                    Y = z.Y,
                    Width = z.Width,
                    Height = z.Height,
                    ContentKey = z.ContentKey,
                    Visible = z.Visible,
                    ForeColor = z.ForeColor,
                    BackColor = z.BackColor
                })
                .ToList()
        };
    }
}

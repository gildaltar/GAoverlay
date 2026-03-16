using GAoverlay.Contracts;

namespace GAoverlay.Host;

public sealed class LayoutProfilesForm : Form
{
    private readonly string _accentHex;
    private readonly ListBox _list = new() { Dock = DockStyle.Fill };
    private readonly PropertyGrid _grid = new() { Dock = DockStyle.Fill };
    private readonly List<LayoutProfile> _profiles;

    public List<LayoutProfile> Profiles => _profiles;

    public LayoutProfilesForm(IEnumerable<LayoutProfile> profiles, string accentHex)
    {
        _accentHex = string.IsNullOrWhiteSpace(accentHex) ? HostUi.DefaultAccentHex : accentHex;
        Text = "Per-Game Layout Profiles";
        Width = 1080;
        Height = 720;
        MinimumSize = new Size(980, 640);
        StartPosition = FormStartPosition.CenterParent;
        HostUi.ApplyDialogChrome(this);

        _profiles = profiles.Select(Clone).ToList();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = BackColor
        };
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle());

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52 };
        HostUi.StyleToolbar(toolbar);
        var addButton = new Button { Text = "Add profile", AutoSize = true };
        var duplicateButton = new Button { Text = "Duplicate", AutoSize = true };
        var deleteButton = new Button { Text = "Delete", AutoSize = true };
        var applyButton = new Button { Text = "Save layout", AutoSize = true };
        HostUi.StyleButton(addButton, _accentHex, primary: false);
        HostUi.StyleButton(duplicateButton, _accentHex, primary: false);
        HostUi.StyleButton(deleteButton, _accentHex, primary: false);
        HostUi.StyleButton(applyButton, _accentHex, primary: true);

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

        HostUi.StyleListBox(_list);
        HostUi.StylePropertyGrid(_grid);
        _list.Height = 460;
        _grid.Height = 460;
        _list.DisplayMember = nameof(LayoutProfile.Name);
        _list.SelectedIndexChanged += (_, _) => _grid.SelectedObject = _list.SelectedItem;
        _grid.HelpVisible = true;
        _grid.ToolbarVisible = false;

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            BackColor = BackColor,
            FixedPanel = FixedPanel.Panel1,
            SplitterDistance = 300,
            Panel1MinSize = 260,
            Panel2MinSize = 420
        };
        split.Panel1.Padding = new Padding(20, 0, 12, 20);
        split.Panel2.Padding = new Padding(12, 0, 20, 20);
        var listSection = HostUi.CreateSectionPanel(
            "Profiles",
            "Pick a game profile to inspect or duplicate. This list should stay calm and easy to scan.",
            _list);
        listSection.Dock = DockStyle.Fill;
        split.Panel1.Controls.Add(listSection);

        var detailSection = HostUi.CreateSectionPanel(
            "Details",
            "Edit the selected profile here. Keep defaults simple, and let per-game nuance live inside the zone definitions.",
            _grid);
        detailSection.Dock = DockStyle.Fill;
        split.Panel2.Controls.Add(detailSection);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(20, 12, 20, 20),
            Height = 72,
            BackColor = HostUi.Surface
        };
        var closeButton = new Button { Text = "Cancel", AutoSize = true };
        HostUi.StyleButton(closeButton, _accentHex, primary: false);
        closeButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        footer.Controls.Add(applyButton);
        footer.Controls.Add(closeButton);

        root.Controls.Add(HostUi.CreateTopBand(_accentHex), 0, 0);
        root.Controls.Add(HostUi.CreateHeroPanel(
            _accentHex,
            "PROFILE STUDIO",
            "Shape what the overlay reveals for each game.",
            "Profiles are where GAoverlay shifts from a quiet default into something tailored. Keep each profile understandable, then let plugins and zones add the deeper magic."), 0, 1);
        root.Controls.Add(toolbar, 0, 2);
        root.Controls.Add(split, 0, 3);
        root.Controls.Add(footer, 0, 4);

        Controls.Add(root);
        CancelButton = closeButton;
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

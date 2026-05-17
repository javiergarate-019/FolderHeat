using FolderHeat.Application;

namespace FolderHeat.App;

internal sealed class PopupForm : Form
{
    private readonly FolderCatalogService catalog;
    private readonly TextBox searchBox;
    private readonly ListView folderList;
    private IReadOnlyList<FolderGroup> groups = Array.Empty<FolderGroup>();

    public PopupForm(FolderCatalogService catalog)
    {
        this.catalog = catalog;

        Text = "FolderHeat";
        Width = 620;
        Height = 460;
        StartPosition = FormStartPosition.Manual;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        KeyPreview = true;

        searchBox = new TextBox
        {
            Dock = DockStyle.Top,
            PlaceholderText = "Search folders",
            Margin = new Padding(8),
        };
        searchBox.TextChanged += (_, _) => RenderFolders();
        searchBox.KeyDown += SearchBox_KeyDown;

        folderList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            MultiSelect = false,
        };
        folderList.Columns.Add("Folder", 220);
        folderList.Columns.Add("Path", 300);
        folderList.Columns.Add("Heat", 70);
        folderList.DoubleClick += async (_, _) => await OpenSelectedAsync();
        folderList.KeyDown += FolderList_KeyDown;

        var addButton = new Button
        {
            Text = "Add folder",
            AutoSize = true,
        };
        addButton.Click += async (_, _) => await AddFolderAsync();

        var pinButton = new Button
        {
            Text = "Pin",
            AutoSize = true,
        };
        pinButton.Click += async (_, _) => await PinSelectedAsync();

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 42,
            Padding = new Padding(8),
        };
        bottomPanel.Controls.Add(addButton);
        bottomPanel.Controls.Add(pinButton);

        Controls.Add(folderList);
        Controls.Add(bottomPanel);
        Controls.Add(searchBox);

        Deactivate += (_, _) => Hide();
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                Hide();
            }
        };
    }

    public void ShowNearCursor()
    {
        RefreshFolders();

        var screen = Screen.FromPoint(Cursor.Position).WorkingArea;
        Left = Math.Min(Cursor.Position.X, screen.Right - Width);
        Top = Math.Min(Cursor.Position.Y, screen.Bottom - Height);

        Show();
        Activate();
        searchBox.Focus();
        searchBox.SelectAll();
    }

    public async void RefreshFolders()
    {
        groups = await catalog.GetPopupGroupsAsync();
        RenderFolders();
    }

    private void RenderFolders()
    {
        var query = searchBox.Text.Trim();
        folderList.BeginUpdate();
        folderList.Items.Clear();
        folderList.Groups.Clear();

        foreach (var group in groups)
        {
            var listGroup = new ListViewGroup(group.Title);
            folderList.Groups.Add(listGroup);

            foreach (var folder in group.Folders.Where(folder => Matches(folder, query)))
            {
                var item = new ListViewItem(folder.Name, listGroup)
                {
                    Tag = folder,
                };
                item.SubItems.Add(folder.Path);
                item.SubItems.Add(Math.Round(folder.Heat).ToString());
                folderList.Items.Add(item);
            }
        }

        if (folderList.Items.Count > 0)
        {
            folderList.Items[0].Selected = true;
        }

        folderList.EndUpdate();
    }

    private async Task AddFolderAsync()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Add folder",
            UseDescriptionForTitle = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            await catalog.AddFolderAsync(dialog.SelectedPath);
            RefreshFolders();
        }
    }

    private async Task PinSelectedAsync()
    {
        var selected = GetSelectedCandidate();
        if (selected is null)
        {
            return;
        }

        await catalog.PinFolderAsync(selected.Path, !selected.IsPinned);
        RefreshFolders();
    }

    private async Task OpenSelectedAsync()
    {
        var selected = GetSelectedCandidate();
        if (selected is null)
        {
            return;
        }

        await catalog.OpenFolderAsync(selected.Path);
        Hide();
    }

    private FolderCandidate? GetSelectedCandidate()
    {
        return folderList.SelectedItems.Count == 0
            ? null
            : folderList.SelectedItems[0].Tag as FolderCandidate;
    }

    private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Down && folderList.Items.Count > 0)
        {
            folderList.Focus();
            folderList.Items[0].Selected = true;
            e.Handled = true;
        }

        if (e.KeyCode == Keys.Enter)
        {
            _ = OpenSelectedAsync();
            e.Handled = true;
        }
    }

    private void FolderList_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            _ = OpenSelectedAsync();
            e.Handled = true;
        }
    }

    private static bool Matches(FolderCandidate folder, string query)
    {
        return query.Length == 0
            || folder.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || folder.Path.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}

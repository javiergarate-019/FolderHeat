using FolderHeat.Application;

namespace FolderHeat.App;

internal sealed class PopupForm : Form
{
    private readonly FolderCatalogService catalog;
    private readonly TextBox searchBox;
    private readonly ListView folderList;
    private readonly Button pinButton;
    private readonly Button ignoreButton;
    private IReadOnlyList<FolderGroup> groups = Array.Empty<FolderGroup>();

    public PopupForm(FolderCatalogService catalog)
    {
        this.catalog = catalog;

        Text = "FolderHeat";
        Icon = AppIcons.FolderHeat;
        Width = 760;
        Height = 650;
        MinimumSize = new Size(560, 360);
        StartPosition = FormStartPosition.Manual;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
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
            ShowItemToolTips = true,
        };
        folderList.Columns.Add("Folder", 190);
        folderList.Columns.Add("Path", 360);
        folderList.Columns.Add("Heat", 70);
        folderList.Columns.Add("Why", 90);
        folderList.DoubleClick += async (_, _) => await OpenSelectedAsync();
        folderList.KeyDown += FolderList_KeyDown;
        folderList.Resize += (_, _) => ResizeColumns();

        var addButton = new Button
        {
            Text = "Add folder",
            AutoSize = true,
        };
        ConfigureButton(addButton, UiIconKind.Add);
        addButton.Click += async (_, _) => await AddFolderAsync();

        pinButton = new Button
        {
            Text = "Pin",
            AutoSize = true,
        };
        ConfigureButton(pinButton, UiIconKind.Pin);
        pinButton.Click += async (_, _) => await PinSelectedAsync();

        ignoreButton = new Button
        {
            Text = "Ignore",
            AutoSize = true,
        };
        ConfigureButton(ignoreButton, UiIconKind.Ignore);
        ignoreButton.Click += async (_, _) => await IgnoreSelectedAsync();
        folderList.SelectedIndexChanged += (_, _) => UpdateSelectionButtons();

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 42,
            Padding = new Padding(8),
        };
        bottomPanel.Controls.Add(addButton);
        bottomPanel.Controls.Add(ignoreButton);
        bottomPanel.Controls.Add(pinButton);

        Controls.Add(folderList);
        Controls.Add(bottomPanel);
        Controls.Add(searchBox);

        ResizeColumns();

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
        ResizeColumns();
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
                    ToolTipText = folder.Path,
                };
                item.SubItems.Add(folder.Path);
                item.SubItems.Add(Math.Round(folder.Heat).ToString());
                item.SubItems.Add(folder.RankReason);
                folderList.Items.Add(item);
            }
        }

        if (folderList.Items.Count > 0)
        {
            folderList.Items[0].Selected = true;
        }

        UpdateSelectionButtons();

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

    private async Task IgnoreSelectedAsync()
    {
        var selected = GetSelectedCandidate();
        if (selected is null)
        {
            return;
        }

        await catalog.IgnoreFolderAsync(selected.Path, true);
        RefreshFolders();
    }

    private void UpdateSelectionButtons()
    {
        var selected = GetSelectedCandidate();
        pinButton.Enabled = selected is not null;
        pinButton.Text = selected?.IsPinned == true ? "Unpin" : "Pin";
        pinButton.Image = UiIconFactory.Create(selected?.IsPinned == true ? UiIconKind.Unpin : UiIconKind.Pin);
        ignoreButton.Enabled = selected is not null;
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

    private void ResizeColumns()
    {
        if (folderList.Columns.Count < 4)
        {
            return;
        }

        var availableWidth = Math.Max(480, folderList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8);
        folderList.Columns[0].Width = Math.Max(160, (int)(availableWidth * 0.27));
        folderList.Columns[2].Width = 70;
        folderList.Columns[3].Width = 90;
        folderList.Columns[1].Width = Math.Max(180, availableWidth - folderList.Columns[0].Width - folderList.Columns[2].Width - folderList.Columns[3].Width);
    }

    private static void ConfigureButton(Button button, UiIconKind iconKind)
    {
        button.Image = UiIconFactory.Create(iconKind);
        button.ImageAlign = ContentAlignment.MiddleLeft;
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
        button.FlatStyle = FlatStyle.Standard;
        button.Padding = new Padding(4, 0, 6, 0);
        button.Height = 28;
    }
}

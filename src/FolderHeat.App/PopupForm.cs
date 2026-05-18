using FolderHeat.Application;

namespace FolderHeat.App;

internal sealed class PopupForm : Form
{
    private readonly FolderCatalogService catalog;
    private readonly TextBox searchBox;
    private readonly ListView folderList;
    private readonly Label emptyLabel;
    private readonly ToolTip toolTip = new();
    private readonly Button openButton;
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
            BorderStyle = BorderStyle.FixedSingle,
        };
        folderList.Columns.Add("Folder", 190);
        folderList.Columns.Add("Path", 430);
        folderList.Columns.Add("Reason", 110);
        folderList.DoubleClick += async (_, _) => await OpenSelectedAsync();
        folderList.KeyDown += FolderList_KeyDown;
        folderList.Resize += (_, _) => ResizeColumns();

        emptyLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "No folders to show",
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = SystemColors.GrayText,
            Visible = false,
        };

        openButton = new Button
        {
            Text = "Open",
            AutoSize = true,
        };
        ConfigureButton(openButton, UiIconKind.Open);
        openButton.Click += async (_, _) => await OpenSelectedAsync();
        toolTip.SetToolTip(openButton, "Open selected folder");

        var addButton = new Button
        {
            Text = "Add folder",
            AutoSize = true,
        };
        ConfigureButton(addButton, UiIconKind.Add);
        addButton.Click += async (_, _) => await AddFolderAsync();
        toolTip.SetToolTip(addButton, "Add a folder");

        pinButton = new Button
        {
            Text = "Pin",
            AutoSize = true,
        };
        ConfigureButton(pinButton, UiIconKind.Pin);
        pinButton.Click += async (_, _) => await PinSelectedAsync();
        toolTip.SetToolTip(pinButton, "Pin selected folder");

        ignoreButton = new Button
        {
            Text = "Ignore",
            AutoSize = true,
        };
        ConfigureButton(ignoreButton, UiIconKind.Ignore);
        ignoreButton.Click += async (_, _) => await IgnoreSelectedAsync();
        toolTip.SetToolTip(ignoreButton, "Ignore selected folder");
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
        bottomPanel.Controls.Add(openButton);

        Controls.Add(folderList);
        Controls.Add(emptyLabel);
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

            if (!searchBox.Focused && e.Control && e.KeyCode == Keys.P)
            {
                _ = PinSelectedAsync();
                e.Handled = true;
            }

            if (!searchBox.Focused && e.KeyCode == Keys.Delete)
            {
                _ = IgnoreSelectedAsync();
                e.Handled = true;
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
            var matchingFolders = group.Folders
                .Where(folder => Matches(folder, query))
                .ToArray();

            if (matchingFolders.Length == 0)
            {
                continue;
            }

            var listGroup = new ListViewGroup($"{group.Title.ToUpperInvariant()} ({matchingFolders.Length})");
            folderList.Groups.Add(listGroup);

            foreach (var folder in matchingFolders)
            {
                var reason = GetReasonLabel(folder.RankReason);
                var item = new ListViewItem(folder.Name, listGroup)
                {
                    Tag = folder,
                    ToolTipText = $"{folder.Path}{Environment.NewLine}{GetReasonTooltip(folder.RankReason)}",
                };
                item.SubItems.Add(folder.Path);
                item.SubItems.Add(reason);
                folderList.Items.Add(item);
            }
        }

        if (folderList.Items.Count > 0)
        {
            folderList.Items[0].Selected = true;
        }

        UpdateSelectionButtons();
        emptyLabel.Visible = folderList.Items.Count == 0;
        emptyLabel.BringToFront();

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
        openButton.Enabled = selected is not null;
        pinButton.Enabled = selected is not null;
        pinButton.Text = selected?.IsPinned == true ? "Unpin" : "Pin";
        pinButton.Image = UiIconFactory.Create(selected?.IsPinned == true ? UiIconKind.Unpin : UiIconKind.Pin);
        toolTip.SetToolTip(pinButton, selected?.IsPinned == true ? "Unpin selected folder" : "Pin selected folder");
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

        if (e.Control && e.KeyCode == Keys.P)
        {
            _ = PinSelectedAsync();
            e.Handled = true;
        }

        if (e.KeyCode == Keys.Delete)
        {
            _ = IgnoreSelectedAsync();
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
        if (folderList.Columns.Count < 3)
        {
            return;
        }

        var availableWidth = Math.Max(480, folderList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8);
        folderList.Columns[0].Width = Math.Max(170, (int)(availableWidth * 0.28));
        folderList.Columns[2].Width = 110;
        folderList.Columns[1].Width = Math.Max(220, availableWidth - folderList.Columns[0].Width - folderList.Columns[2].Width);
    }

    private static string GetReasonLabel(string reason)
    {
        return reason switch
        {
            "Explorer" => "Current",
            "Next" => "Next",
            "Related" => "Related",
            "Pinned" => "Pinned",
            "Recent" => "Recent",
            "Frequent" => "Frequent",
            _ => "Tracked",
        };
    }

    private static string GetReasonTooltip(string reason)
    {
        return reason switch
        {
            "Explorer" => "Active folder detected from your current context",
            "Next" => "Usually opened after your current context",
            "Related" => "Related to your current context",
            "Pinned" => "Pinned by you",
            "Recent" => "Opened recently",
            "Frequent" => "Opened often",
            _ => "Tracked by FolderHeat",
        };
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

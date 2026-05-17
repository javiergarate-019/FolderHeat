using FolderHeat.Application;

namespace FolderHeat.App;

internal sealed class IgnoredFoldersForm : Form
{
    private readonly FolderCatalogService catalog;
    private readonly ListView folderList;
    private readonly Button restoreButton;

    public IgnoredFoldersForm(FolderCatalogService catalog)
    {
        this.catalog = catalog;

        Text = "Ignored folders";
        Icon = AppIcons.FolderHeat;
        Width = 620;
        Height = 360;
        MinimumSize = new Size(480, 300);
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = true;

        folderList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            MultiSelect = false,
        };
        folderList.Columns.Add("Folder", 180);
        folderList.Columns.Add("Path", 380);
        folderList.SelectedIndexChanged += (_, _) => UpdateButtons();

        restoreButton = new Button
        {
            Text = "Restore",
            AutoSize = true,
            Enabled = false,
        };
        ConfigureButton(restoreButton, UiIconKind.Restore);
        restoreButton.Click += async (_, _) => await RestoreSelectedAsync();

        var closeButton = new Button
        {
            Text = "Close",
            AutoSize = true,
        };
        ConfigureButton(closeButton, UiIconKind.Ignore);
        closeButton.Click += (_, _) => Close();

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 42,
            Padding = new Padding(8),
        };
        bottomPanel.Controls.Add(closeButton);
        bottomPanel.Controls.Add(restoreButton);

        Controls.Add(folderList);
        Controls.Add(bottomPanel);

        Shown += async (_, _) => await RefreshFoldersAsync();
    }

    private async Task RefreshFoldersAsync()
    {
        var folders = await catalog.GetIgnoredFoldersAsync();

        folderList.BeginUpdate();
        folderList.Items.Clear();
        foreach (var folder in folders)
        {
            var item = new ListViewItem(folder.Name)
            {
                Tag = folder,
            };
            item.SubItems.Add(folder.Path);
            folderList.Items.Add(item);
        }

        folderList.EndUpdate();
        UpdateButtons();
    }

    private async Task RestoreSelectedAsync()
    {
        if (folderList.SelectedItems.Count == 0 ||
            folderList.SelectedItems[0].Tag is not FolderCandidate folder)
        {
            return;
        }

        await catalog.IgnoreFolderAsync(folder.Path, false);
        await RefreshFoldersAsync();
    }

    private void UpdateButtons()
    {
        restoreButton.Enabled = folderList.SelectedItems.Count > 0;
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

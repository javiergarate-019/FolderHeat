using FolderHeat.Application;

namespace FolderHeat.App;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly FolderCatalogService catalog;
    private readonly AppSettingsStore settingsStore;
    private readonly NotifyIcon notifyIcon;
    private GlobalHotkey hotkey;
    private PopupForm? popup;
    private HotkeySettings hotkeySettings;

    public TrayApplicationContext(FolderCatalogService catalog, AppSettingsStore settingsStore)
    {
        this.catalog = catalog;
        this.settingsStore = settingsStore;
        hotkeySettings = settingsStore.LoadHotkey();

        notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = $"FolderHeat ({hotkeySettings.DisplayText})",
            Visible = true,
            ContextMenuStrip = BuildContextMenu(),
        };
        notifyIcon.MouseClick += NotifyIcon_MouseClick;

        hotkey = RegisterHotkey(hotkeySettings);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            popup?.Dispose();
            hotkey.Dispose();
            notifyIcon.Dispose();
        }

        base.Dispose(disposing);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open FolderHeat", null, (_, _) => ShowPopup());
        menu.Items.Add("Add current folder", null, async (_, _) => await AddCurrentFolderAsync());
        menu.Items.Add("Add folder...", null, async (_, _) => await AddFolderAsync());
        menu.Items.Add("Ignored folders...", null, (_, _) => ShowIgnoredFolders());
        menu.Items.Add("Settings...", null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
        return menu;
    }

    private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ShowPopup();
        }
    }

    private void ShowPopup()
    {
        if (popup is null || popup.IsDisposed)
        {
            popup = new PopupForm(catalog);
        }

        popup.ShowNearCursor();
    }

    private async Task AddFolderAsync()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Add folder",
            UseDescriptionForTitle = true,
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            await catalog.AddFolderAsync(dialog.SelectedPath);
            popup?.RefreshFolders();
        }
    }

    private async Task AddCurrentFolderAsync()
    {
        if (await catalog.AddCurrentFolderAsync())
        {
            popup?.RefreshFolders();
            return;
        }

        MessageBox.Show(
            "No active Explorer folder was detected.",
            "FolderHeat",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ShowIgnoredFolders()
    {
        using var form = new IgnoredFoldersForm(catalog);
        form.ShowDialog();
        popup?.RefreshFolders();
    }

    private void ShowSettings()
    {
        using var form = new SettingsForm(hotkeySettings);
        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        hotkeySettings = form.SelectedHotkey;
        settingsStore.SaveHotkey(hotkeySettings);
        hotkey.Dispose();
        hotkey = RegisterHotkey(hotkeySettings);
        notifyIcon.Text = $"FolderHeat ({hotkeySettings.DisplayText})";
    }

    private GlobalHotkey RegisterHotkey(HotkeySettings settings)
    {
        var configuredHotkey = new GlobalHotkey(settings.Key, settings.Modifiers);
        configuredHotkey.Pressed += (_, _) => ShowPopup();
        configuredHotkey.Register();
        return configuredHotkey;
    }
}

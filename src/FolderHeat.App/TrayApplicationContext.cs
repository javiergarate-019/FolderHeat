using FolderHeat.Application;

namespace FolderHeat.App;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly FolderCatalogService catalog;
    private readonly NotifyIcon notifyIcon;
    private readonly GlobalHotkey hotkey;
    private PopupForm? popup;

    public TrayApplicationContext(FolderCatalogService catalog)
    {
        this.catalog = catalog;

        notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "FolderHeat",
            Visible = true,
            ContextMenuStrip = BuildContextMenu(),
        };
        notifyIcon.MouseClick += NotifyIcon_MouseClick;

        hotkey = new GlobalHotkey(Keys.Space, HotkeyModifiers.Control | HotkeyModifiers.Alt);
        hotkey.Pressed += (_, _) => ShowPopup();
        hotkey.Register();
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
        menu.Items.Add("Add folder...", null, async (_, _) => await AddFolderAsync());
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
}

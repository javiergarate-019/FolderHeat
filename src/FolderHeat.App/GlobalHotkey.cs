using System.Runtime.InteropServices;

namespace FolderHeat.App;

[Flags]
internal enum HotkeyModifiers
{
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008,
}

internal sealed class GlobalHotkey : NativeWindow, IDisposable
{
    private const int WmHotkey = 0x0312;
    private readonly int id;
    private readonly Keys key;
    private readonly HotkeyModifiers modifiers;
    private bool registered;

    public GlobalHotkey(Keys key, HotkeyModifiers modifiers)
    {
        this.key = key;
        this.modifiers = modifiers;
        id = GetHashCode();
        CreateHandle(new CreateParams());
    }

    public event EventHandler? Pressed;

    public void Register()
    {
        if (registered)
        {
            return;
        }

        registered = RegisterHotKey(Handle, id, (uint)modifiers, (uint)key);
    }

    public void Dispose()
    {
        if (registered)
        {
            UnregisterHotKey(Handle, id);
            registered = false;
        }

        DestroyHandle();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && m.WParam.ToInt32() == id)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        base.WndProc(ref m);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

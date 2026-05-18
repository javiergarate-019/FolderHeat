using Microsoft.Win32;

namespace FolderHeat.App;

internal sealed class WindowsStartupRegistration
{
    private const string AppName = "FolderHeat";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return !string.IsNullOrWhiteSpace(key?.GetValue(AppName) as string);
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (enabled)
        {
            key.SetValue(AppName, QuotePath(GetExecutablePath()), RegistryValueKind.String);
            return;
        }

        key.DeleteValue(AppName, throwOnMissingValue: false);
    }

    private static string GetExecutablePath()
    {
        return Environment.ProcessPath ?? System.Windows.Forms.Application.ExecutablePath;
    }

    private static string QuotePath(string path)
    {
        return path.StartsWith('"') ? path : $"\"{path}\"";
    }
}

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using FolderHeat.Application;

namespace FolderHeat.Infrastructure;

public sealed class WindowsRecentFolderSource : IActiveFolderSource
{
    public Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var recentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        if (!Directory.Exists(recentDirectory))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        try
        {
            var paths = Directory
                .EnumerateFiles(recentDirectory, "*.lnk")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Take(20)
                .Select(ResolveShortcutTarget)
                .Where(path => path is not null)
                .Select(path => ActiveFolderPath.FromPathOrFile(path!))
                .Where(path => path is not null)
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToArray();

            return Task.FromResult<IReadOnlyList<string>>(paths);
        }
        catch (IOException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (COMException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
    }

    [SupportedOSPlatform("windows")]
    private static string? ResolveShortcutTarget(string shortcutPath)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType is null)
        {
            return null;
        }

        dynamic? shell = Activator.CreateInstance(shellType);
        if (shell is null)
        {
            return null;
        }

        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        string targetPath = shortcut.TargetPath;
        return string.IsNullOrWhiteSpace(targetPath) ? null : targetPath;
    }
}

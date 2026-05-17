using System.Runtime.InteropServices;
using FolderHeat.Application;
using Microsoft.CSharp.RuntimeBinder;

namespace FolderHeat.Infrastructure;

public sealed class ExplorerActiveFolderSource : IActiveFolderSource
{
    public Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var foregroundWindow = GetForegroundWindow();
        var folders = new List<ExplorerFolder>();

        try
        {
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType is null)
            {
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
            }

            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell is null)
            {
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
            }

            foreach (dynamic window in shell.Windows())
            {
                cancellationToken.ThrowIfCancellationRequested();

                string? path = TryGetExplorerFolderPath(window);
                if (path is null || !Directory.Exists(path))
                {
                    continue;
                }

                var handle = new IntPtr(Convert.ToInt64(window.HWND));
                folders.Add(new ExplorerFolder(path, handle == foregroundWindow));
            }
        }
        catch (COMException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (RuntimeBinderException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        var paths = folders
            .OrderByDescending(folder => folder.IsForeground)
            .Select(folder => folder.Path)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult<IReadOnlyList<string>>(paths);
    }

    private static string? TryGetExplorerFolderPath(dynamic window)
    {
        string locationUrl = window.LocationURL;
        if (string.IsNullOrWhiteSpace(locationUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(locationUrl, UriKind.Absolute, out var uri) || !uri.IsFile)
        {
            return null;
        }

        var path = uri.LocalPath;
        return Path.IsPathFullyQualified(path) ? path : null;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private sealed record ExplorerFolder(string Path, bool IsForeground);
}

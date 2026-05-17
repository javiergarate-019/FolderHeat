using System.Diagnostics;
using FolderHeat.Application;

namespace FolderHeat.Infrastructure;

public sealed class WindowsFolderLauncher : IFolderLauncher
{
    public void OpenFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException(path);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
        });
    }
}

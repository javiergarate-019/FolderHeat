namespace FolderHeat.App;

internal static class AppIcons
{
    public static Icon FolderHeat { get; } = LoadIcon();

    private static Icon LoadIcon()
    {
        var localPath = Path.Combine(AppContext.BaseDirectory, "Assets", "FolderHeat.ico");
        if (File.Exists(localPath))
        {
            return new Icon(localPath);
        }

        var projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FolderHeat.ico");
        return File.Exists(projectPath) ? new Icon(projectPath) : SystemIcons.Application;
    }
}

namespace FolderHeat.App;

internal static class AppPaths
{
    public static string DataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FolderHeat");

    public static string DatabasePath => Path.Combine(DataDirectory, "folderheat.db");
}

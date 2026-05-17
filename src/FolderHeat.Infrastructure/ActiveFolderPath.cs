namespace FolderHeat.Infrastructure;

internal static class ActiveFolderPath
{
    public static string? FromPathOrFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (Directory.Exists(path))
        {
            return path;
        }

        if (File.Exists(path))
        {
            return Path.GetDirectoryName(path);
        }

        return null;
    }

    public static string? FromUri(string uriText)
    {
        if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri) || !uri.IsFile)
        {
            return null;
        }

        return FromPathOrFile(uri.LocalPath);
    }
}

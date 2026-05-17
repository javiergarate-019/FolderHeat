namespace FolderHeat.Domain;

public sealed class FolderEntry
{
    public FolderEntry(
        string path,
        DateTimeOffset createdAt,
        DateTimeOffset? lastAccessedAt = null,
        int accessCount = 0,
        bool isPinned = false,
        bool isIgnored = false)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Folder path is required.", nameof(path));
        }

        Path = NormalizePath(path);
        CreatedAt = createdAt;
        LastAccessedAt = lastAccessedAt;
        AccessCount = accessCount;
        IsPinned = isPinned;
        IsIgnored = isIgnored;
    }

    public string Path { get; }

    public string Name
    {
        get
        {
            var trimmed = Path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            return System.IO.Path.GetFileName(trimmed).Length == 0 ? Path : System.IO.Path.GetFileName(trimmed);
        }
    }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? LastAccessedAt { get; private set; }

    public int AccessCount { get; private set; }

    public bool IsPinned { get; private set; }

    public bool IsIgnored { get; private set; }

    public void RecordAccess(DateTimeOffset when)
    {
        AccessCount++;
        LastAccessedAt = when;
    }

    public void SetPinned(bool isPinned)
    {
        IsPinned = isPinned;
    }

    public void SetIgnored(bool isIgnored)
    {
        IsIgnored = isIgnored;
    }

    public static string NormalizePath(string path)
    {
        return System.IO.Path.GetFullPath(path).TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
    }
}

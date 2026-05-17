namespace FolderHeat.Application;

public sealed record FolderCandidate(
    string Path,
    string Name,
    double Heat,
    int AccessCount,
    DateTimeOffset? LastAccessedAt,
    bool IsPinned);

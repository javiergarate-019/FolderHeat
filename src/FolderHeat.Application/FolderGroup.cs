namespace FolderHeat.Application;

public sealed record FolderGroup(string Title, IReadOnlyList<FolderCandidate> Folders);

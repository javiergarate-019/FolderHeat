namespace FolderHeat.Application;

public sealed record FolderTransition(string FromPath, string ToPath, int Count);

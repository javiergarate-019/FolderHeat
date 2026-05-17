namespace FolderHeat.Application;

public interface IActiveFolderSource
{
    Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default);
}

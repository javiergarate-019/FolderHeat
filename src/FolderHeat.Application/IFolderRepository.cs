using FolderHeat.Domain;

namespace FolderHeat.Application;

public interface IFolderRepository
{
    Task<IReadOnlyList<FolderEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<FolderEntry?> GetByPathAsync(string path, CancellationToken cancellationToken = default);

    Task SaveAsync(FolderEntry folder, CancellationToken cancellationToken = default);

    Task RecordTransitionAsync(string fromPath, string toPath, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FolderTransition>> GetTransitionTargetsAsync(string fromPath, CancellationToken cancellationToken = default);
}

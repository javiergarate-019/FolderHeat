using FolderHeat.Domain;

namespace FolderHeat.Application;

public interface IFolderRepository
{
    Task<IReadOnlyList<FolderEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<FolderEntry?> GetByPathAsync(string path, CancellationToken cancellationToken = default);

    Task SaveAsync(FolderEntry folder, CancellationToken cancellationToken = default);
}

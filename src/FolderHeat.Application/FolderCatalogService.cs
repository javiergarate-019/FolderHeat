using FolderHeat.Domain;

namespace FolderHeat.Application;

public sealed class FolderCatalogService
{
    private readonly IFolderRepository repository;
    private readonly IClock clock;
    private readonly IFolderLauncher launcher;

    public FolderCatalogService(IFolderRepository repository, IClock clock, IFolderLauncher launcher)
    {
        this.repository = repository;
        this.clock = clock;
        this.launcher = launcher;
    }

    public async Task AddFolderAsync(string path, CancellationToken cancellationToken = default)
    {
        var normalizedPath = FolderEntry.NormalizePath(path);
        var existing = await repository.GetByPathAsync(normalizedPath, cancellationToken);
        if (existing is not null)
        {
            existing.SetIgnored(false);
            await repository.SaveAsync(existing, cancellationToken);
            return;
        }

        await repository.SaveAsync(new FolderEntry(normalizedPath, clock.Now), cancellationToken);
    }

    public async Task PinFolderAsync(string path, bool isPinned, CancellationToken cancellationToken = default)
    {
        var folder = await GetRequiredFolderAsync(path, cancellationToken);
        folder.SetPinned(isPinned);
        await repository.SaveAsync(folder, cancellationToken);
    }

    public async Task IgnoreFolderAsync(string path, bool isIgnored, CancellationToken cancellationToken = default)
    {
        var folder = await GetRequiredFolderAsync(path, cancellationToken);
        folder.SetIgnored(isIgnored);
        await repository.SaveAsync(folder, cancellationToken);
    }

    public async Task OpenFolderAsync(string path, CancellationToken cancellationToken = default)
    {
        var normalizedPath = FolderEntry.NormalizePath(path);
        var folder = await repository.GetByPathAsync(normalizedPath, cancellationToken)
            ?? new FolderEntry(normalizedPath, clock.Now);

        folder.RecordAccess(clock.Now);
        await repository.SaveAsync(folder, cancellationToken);
        launcher.OpenFolder(normalizedPath);
    }

    public async Task<IReadOnlyList<FolderGroup>> GetPopupGroupsAsync(CancellationToken cancellationToken = default)
    {
        var folders = (await repository.GetAllAsync(cancellationToken))
            .Where(folder => !folder.IsIgnored)
            .ToArray();

        var pinned = folders
            .Where(folder => folder.IsPinned)
            .OrderByDescending(folder => FolderHeatScore.Calculate(folder, clock.Now))
            .Take(8)
            .Select(ToCandidate)
            .ToArray();

        var recent = folders
            .Where(folder => folder.LastAccessedAt is not null)
            .OrderByDescending(folder => folder.LastAccessedAt)
            .Take(8)
            .Select(ToCandidate)
            .ToArray();

        var frequent = folders
            .Where(folder => folder.AccessCount > 0)
            .OrderByDescending(folder => folder.AccessCount)
            .ThenByDescending(folder => folder.LastAccessedAt)
            .Take(8)
            .Select(ToCandidate)
            .ToArray();

        var activeNow = folders
            .OrderByDescending(folder => FolderHeatScore.Calculate(folder, clock.Now))
            .Take(8)
            .Select(ToCandidate)
            .ToArray();

        return new[]
        {
            new FolderGroup("Active Now", activeNow),
            new FolderGroup("Pinned", pinned),
            new FolderGroup("Recent", recent),
            new FolderGroup("Frequent", frequent),
        };
    }

    private async Task<FolderEntry> GetRequiredFolderAsync(string path, CancellationToken cancellationToken)
    {
        var normalizedPath = FolderEntry.NormalizePath(path);
        return await repository.GetByPathAsync(normalizedPath, cancellationToken)
            ?? throw new InvalidOperationException($"Folder is not tracked: {normalizedPath}");
    }

    private FolderCandidate ToCandidate(FolderEntry folder)
    {
        return new FolderCandidate(
            folder.Path,
            folder.Name,
            FolderHeatScore.Calculate(folder, clock.Now),
            folder.AccessCount,
            folder.LastAccessedAt,
            folder.IsPinned);
    }
}

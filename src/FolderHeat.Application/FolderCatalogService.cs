using FolderHeat.Domain;

namespace FolderHeat.Application;

public sealed class FolderCatalogService
{
    private readonly IFolderRepository repository;
    private readonly IClock clock;
    private readonly IFolderLauncher launcher;
    private readonly IActiveFolderSource activeFolderSource;
    private string? lastOpenedPath;

    public FolderCatalogService(
        IFolderRepository repository,
        IClock clock,
        IFolderLauncher launcher,
        IActiveFolderSource? activeFolderSource = null)
    {
        this.repository = repository;
        this.clock = clock;
        this.launcher = launcher;
        this.activeFolderSource = activeFolderSource ?? new EmptyActiveFolderSource();
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

        if (lastOpenedPath is not null &&
            !string.Equals(lastOpenedPath, normalizedPath, StringComparison.OrdinalIgnoreCase))
        {
            await repository.RecordTransitionAsync(lastOpenedPath, normalizedPath, cancellationToken);
        }

        lastOpenedPath = normalizedPath;
        launcher.OpenFolder(normalizedPath);
    }

    public async Task<bool> AddCurrentFolderAsync(CancellationToken cancellationToken = default)
    {
        var activePaths = await GetNormalizedActiveFolderPathsAsync(cancellationToken);
        var currentPath = activePaths.FirstOrDefault();
        if (currentPath is null)
        {
            return false;
        }

        await AddFolderAsync(currentPath, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<FolderCandidate>> GetIgnoredFoldersAsync(CancellationToken cancellationToken = default)
    {
        var folders = await repository.GetAllAsync(cancellationToken);
        return folders
            .Where(folder => folder.IsIgnored)
            .OrderBy(folder => folder.Name)
            .ThenBy(folder => folder.Path)
            .Select(ToCandidate)
            .ToArray();
    }

    public async Task<IReadOnlyList<FolderGroup>> GetPopupGroupsAsync(CancellationToken cancellationToken = default)
    {
        var activePaths = await GetNormalizedActiveFolderPathsAsync(cancellationToken);
        foreach (var activePath in activePaths)
        {
            if (await repository.GetByPathAsync(activePath, cancellationToken) is null)
            {
                await repository.SaveAsync(new FolderEntry(activePath, clock.Now), cancellationToken);
            }
        }

        var activePathSet = activePaths.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var transitionPathSet = await GetTransitionPathSetAsync(activePaths, cancellationToken);
        var folders = (await repository.GetAllAsync(cancellationToken))
            .Where(folder => !folder.IsIgnored)
            .ToArray();

        var usedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var activeNow = folders
            .OrderByDescending(folder => CalculateHeat(folder, activePathSet, transitionPathSet))
            .Take(8)
            .Select(folder => ToCandidate(folder, activePathSet, transitionPathSet))
            .Where(folder => usedPaths.Add(folder.Path))
            .ToArray();

        var pinned = folders
            .Where(folder => folder.IsPinned)
            .OrderByDescending(folder => FolderHeatScore.Calculate(folder, clock.Now))
            .Take(8)
            .Select(ToCandidate)
            .Where(folder => usedPaths.Add(folder.Path))
            .ToArray();

        var recent = folders
            .Where(folder => folder.LastAccessedAt is not null)
            .OrderByDescending(folder => folder.LastAccessedAt)
            .Take(8)
            .Select(ToCandidate)
            .Where(folder => usedPaths.Add(folder.Path))
            .ToArray();

        var frequent = folders
            .Where(folder => folder.AccessCount > 0)
            .OrderByDescending(folder => folder.AccessCount)
            .ThenByDescending(folder => folder.LastAccessedAt)
            .Take(8)
            .Select(ToCandidate)
            .Where(folder => usedPaths.Add(folder.Path))
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

    private async Task<IReadOnlyList<string>> GetNormalizedActiveFolderPathsAsync(CancellationToken cancellationToken)
    {
        var activePaths = await activeFolderSource.GetActiveFolderPathsAsync(cancellationToken);
        return activePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(FolderEntry.NormalizePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<IReadOnlySet<string>> GetTransitionPathSetAsync(
        IReadOnlyList<string> activePaths,
        CancellationToken cancellationToken)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var activePath in activePaths.Take(3))
        {
            var transitions = await repository.GetTransitionTargetsAsync(activePath, cancellationToken);
            foreach (var transition in transitions.OrderByDescending(transition => transition.Count).Take(4))
            {
                paths.Add(transition.ToPath);
            }
        }

        return paths;
    }

    private double CalculateHeat(
        FolderEntry folder,
        IReadOnlySet<string>? activePathSet = null,
        IReadOnlySet<string>? transitionPathSet = null)
    {
        var heat = FolderHeatScore.Calculate(folder, clock.Now);
        if (activePathSet?.Contains(folder.Path) == true)
        {
            heat += 250;
        }
        else if (transitionPathSet?.Contains(folder.Path) == true)
        {
            heat += 150;
        }
        else if (activePathSet is not null && IsRelatedToActiveFolder(folder.Path, activePathSet))
        {
            heat += 75;
        }

        return heat;
    }

    private FolderCandidate ToCandidate(FolderEntry folder)
    {
        return ToCandidate(folder, null);
    }

    private FolderCandidate ToCandidate(FolderEntry folder, IReadOnlySet<string>? activePathSet)
    {
        return ToCandidate(folder, activePathSet, null);
    }

    private FolderCandidate ToCandidate(
        FolderEntry folder,
        IReadOnlySet<string>? activePathSet,
        IReadOnlySet<string>? transitionPathSet)
    {
        var isActive = activePathSet?.Contains(folder.Path) == true;
        var isNext = !isActive && transitionPathSet?.Contains(folder.Path) == true;
        var isRelated = !isActive && activePathSet is not null && IsRelatedToActiveFolder(folder.Path, activePathSet);
        return new FolderCandidate(
            folder.Path,
            folder.Name,
            CalculateHeat(folder, activePathSet, transitionPathSet),
            folder.AccessCount,
            folder.LastAccessedAt,
            folder.IsPinned,
            isActive,
            GetRankReason(folder, isActive, isNext, isRelated));
    }

    private static bool IsRelatedToActiveFolder(string path, IReadOnlySet<string> activePathSet)
    {
        foreach (var activePath in activePathSet)
        {
            if (IsAncestorOrDescendant(path, activePath) || HaveSameParent(path, activePath))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAncestorOrDescendant(string first, string second)
    {
        return first.StartsWith(second + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || second.StartsWith(first + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HaveSameParent(string first, string second)
    {
        var firstParent = Path.GetDirectoryName(first);
        var secondParent = Path.GetDirectoryName(second);
        return !string.IsNullOrWhiteSpace(firstParent)
            && string.Equals(firstParent, secondParent, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetRankReason(FolderEntry folder, bool isActive, bool isNext, bool isRelated)
    {
        if (isActive)
        {
            return "Explorer";
        }

        if (isNext)
        {
            return "Next";
        }

        if (isRelated)
        {
            return "Related";
        }

        if (folder.IsPinned)
        {
            return "Pinned";
        }

        if (folder.LastAccessedAt is not null)
        {
            return "Recent";
        }

        if (folder.AccessCount > 0)
        {
            return "Frequent";
        }

        return "Tracked";
    }

    private sealed class EmptyActiveFolderSource : IActiveFolderSource
    {
        public Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
    }
}

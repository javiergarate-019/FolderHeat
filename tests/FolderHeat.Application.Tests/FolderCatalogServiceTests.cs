using FolderHeat.Domain;

namespace FolderHeat.Application.Tests;

public sealed class FolderCatalogServiceTests
{
    [Fact]
    public async Task OpenFolderTracksAccessBeforeLaunching()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var launcher = new CapturingFolderLauncher();
        var service = new FolderCatalogService(repository, clock, launcher);

        await service.AddFolderAsync(@"D:\ERP");
        await service.OpenFolderAsync(@"D:\ERP");

        var folder = await repository.GetByPathAsync(@"D:\ERP");
        Assert.NotNull(folder);
        Assert.Equal(1, folder.AccessCount);
        Assert.Equal(clock.Now, folder.LastAccessedAt);
        Assert.Equal(@"D:\ERP", launcher.LastOpenedPath);
    }

    [Fact]
    public async Task PopupGroupsExcludeIgnoredFolders()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(repository, clock, new CapturingFolderLauncher());

        await service.AddFolderAsync(@"D:\Useful");
        await service.AddFolderAsync(@"D:\Noise");
        await service.IgnoreFolderAsync(@"D:\Noise", true);

        var groups = await service.GetPopupGroupsAsync();

        Assert.Contains(groups.SelectMany(group => group.Folders), folder => folder.Path == @"D:\Useful");
        Assert.DoesNotContain(groups.SelectMany(group => group.Folders), folder => folder.Path == @"D:\Noise");
    }

    [Fact]
    public async Task PopupGroupsIncludeExplorerActiveFolders()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var activeFolders = new FixedActiveFolderSource(@"D:\Current");
        var service = new FolderCatalogService(repository, clock, new CapturingFolderLauncher(), activeFolders);

        var groups = await service.GetPopupGroupsAsync();

        var activeNow = Assert.Single(groups, group => group.Title == "Active Now");
        Assert.Contains(activeNow.Folders, folder => folder.Path == @"D:\Current");

        var saved = await repository.GetByPathAsync(@"D:\Current");
        Assert.NotNull(saved);
        Assert.Equal(0, saved.AccessCount);
        Assert.Null(saved.LastAccessedAt);
    }

    [Fact]
    public async Task PopupGroupsDoNotRepeatFoldersAcrossSections()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(
            repository,
            clock,
            new CapturingFolderLauncher(),
            new FixedActiveFolderSource(@"D:\Current"));

        await service.AddFolderAsync(@"D:\Current");
        await service.PinFolderAsync(@"D:\Current", true);
        await service.OpenFolderAsync(@"D:\Current");

        var groups = await service.GetPopupGroupsAsync();

        Assert.Equal(1, groups.SelectMany(group => group.Folders).Count(folder => folder.Path == @"D:\Current"));
    }

    [Fact]
    public async Task ActiveNowDoesNotConsumeOrdinaryRecentAndPinnedFolders()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(repository, clock, new CapturingFolderLauncher());

        await service.AddFolderAsync(@"D:\Pinned");
        await service.PinFolderAsync(@"D:\Pinned", true);
        await service.OpenFolderAsync(@"D:\Recent");

        var groups = await service.GetPopupGroupsAsync();

        Assert.Empty(groups.Single(group => group.Title == "Active Now").Folders);
        Assert.Contains(groups.Single(group => group.Title == "Pinned").Folders, folder => folder.Path == @"D:\Pinned");
        Assert.Contains(groups.Single(group => group.Title == "Recent").Folders, folder => folder.Path == @"D:\Recent");
    }

    [Fact]
    public async Task RecentGroupBackfillsAfterRemovingFoldersAlreadyShownInActiveNow()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(repository, clock, new CapturingFolderLauncher());

        for (var index = 0; index < 10; index++)
        {
            await service.OpenFolderAsync($@"D:\Recent{index}");
        }

        var groups = await service.GetPopupGroupsAsync();

        var recent = Assert.Single(groups, group => group.Title == "Recent");
        Assert.NotEmpty(recent.Folders);
        Assert.DoesNotContain(recent.Folders, folder => groups
            .Single(group => group.Title == "Active Now")
            .Folders
            .Any(activeFolder => activeFolder.Path == folder.Path));
    }

    [Fact]
    public async Task IgnoredFoldersCanBeListedForManagement()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(repository, clock, new CapturingFolderLauncher());

        await service.AddFolderAsync(@"D:\Useful");
        await service.AddFolderAsync(@"D:\Noise");
        await service.IgnoreFolderAsync(@"D:\Noise", true);

        var ignored = await service.GetIgnoredFoldersAsync();

        var folder = Assert.Single(ignored);
        Assert.Equal(@"D:\Noise", folder.Path);
    }

    [Fact]
    public async Task ActiveNowBoostsFoldersRelatedToActiveContext()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(
            repository,
            clock,
            new CapturingFolderLauncher(),
            new FixedActiveFolderSource(@"D:\ERP\SQL"));

        await service.AddFolderAsync(@"D:\ERP\VB6");

        var groups = await service.GetPopupGroupsAsync();

        var activeNow = Assert.Single(groups, group => group.Title == "Active Now");
        Assert.Contains(activeNow.Folders, folder => folder.Path == @"D:\ERP\VB6" && folder.RankReason == "Related");
    }

    [Fact]
    public async Task OpeningFoldersRecordsTransitionsForLikelyNextFolders()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(repository, clock, new CapturingFolderLauncher());

        await service.OpenFolderAsync(@"D:\ERP\SQL");
        await service.OpenFolderAsync(@"D:\ERP\Deploy");

        var transitions = await repository.GetTransitionTargetsAsync(@"D:\ERP\SQL");

        var transition = Assert.Single(transitions);
        Assert.Equal(@"D:\ERP\Deploy", transition.ToPath);
        Assert.Equal(1, transition.Count);
    }

    [Fact]
    public async Task ActiveNowUsesTransitionsAsLikelyNextSignal()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(
            repository,
            clock,
            new CapturingFolderLauncher(),
            new FixedActiveFolderSource(@"D:\ERP\SQL"));

        await service.OpenFolderAsync(@"D:\ERP\SQL");
        await service.OpenFolderAsync(@"D:\ERP\Deploy");

        var groups = await service.GetPopupGroupsAsync();

        var activeNow = Assert.Single(groups, group => group.Title == "Active Now");
        Assert.Contains(activeNow.Folders, folder => folder.Path == @"D:\ERP\Deploy" && folder.RankReason == "Next");
    }

    [Fact]
    public async Task AddCurrentFolderAddsFirstActiveFolder()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var activeFolders = new FixedActiveFolderSource(@"D:\Current", @"D:\Other");
        var service = new FolderCatalogService(repository, clock, new CapturingFolderLauncher(), activeFolders);

        var added = await service.AddCurrentFolderAsync();

        Assert.True(added);
        Assert.NotNull(await repository.GetByPathAsync(@"D:\Current"));
        Assert.Null(await repository.GetByPathAsync(@"D:\Other"));
    }

    [Fact]
    public async Task AddCurrentFolderReturnsFalseWhenNoActiveFolderExists()
    {
        var repository = new InMemoryFolderRepository();
        var clock = new FixedClock(new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero));
        var service = new FolderCatalogService(
            repository,
            clock,
            new CapturingFolderLauncher(),
            new FixedActiveFolderSource());

        var added = await service.AddCurrentFolderAsync();

        Assert.False(added);
        Assert.Empty(await repository.GetAllAsync());
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset now)
        {
            Now = now;
        }

        public DateTimeOffset Now { get; }
    }

    private sealed class CapturingFolderLauncher : IFolderLauncher
    {
        public string? LastOpenedPath { get; private set; }

        public void OpenFolder(string path)
        {
            LastOpenedPath = path;
        }
    }

    private sealed class FixedActiveFolderSource : IActiveFolderSource
    {
        private readonly IReadOnlyList<string> paths;

        public FixedActiveFolderSource(params string[] paths)
        {
            this.paths = paths;
        }

        public Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(paths);
        }
    }

    private sealed class InMemoryFolderRepository : IFolderRepository
    {
        private readonly Dictionary<string, FolderEntry> folders = new(StringComparer.OrdinalIgnoreCase);

        public Task<IReadOnlyList<FolderEntry>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<FolderEntry>>(folders.Values.ToArray());
        }

        public Task<FolderEntry?> GetByPathAsync(string path, CancellationToken cancellationToken = default)
        {
            folders.TryGetValue(FolderEntry.NormalizePath(path), out var folder);
            return Task.FromResult(folder);
        }

        public Task SaveAsync(FolderEntry folder, CancellationToken cancellationToken = default)
        {
            folders[folder.Path] = folder;
            return Task.CompletedTask;
        }

        public Task RecordTransitionAsync(string fromPath, string toPath, CancellationToken cancellationToken = default)
        {
            var normalizedFromPath = FolderEntry.NormalizePath(fromPath);
            var normalizedToPath = FolderEntry.NormalizePath(toPath);
            var key = (normalizedFromPath, normalizedToPath);
            transitions[key] = transitions.TryGetValue(key, out var count) ? count + 1 : 1;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<FolderTransition>> GetTransitionTargetsAsync(
            string fromPath,
            CancellationToken cancellationToken = default)
        {
            var normalizedFromPath = FolderEntry.NormalizePath(fromPath);
            var results = transitions
                .Where(transition => transition.Key.normalizedFromPath == normalizedFromPath)
                .Select(transition => new FolderTransition(
                    transition.Key.normalizedFromPath,
                    transition.Key.normalizedToPath,
                    transition.Value))
                .OrderByDescending(transition => transition.Count)
                .ThenBy(transition => transition.ToPath)
                .ToArray();

            return Task.FromResult<IReadOnlyList<FolderTransition>>(results);
        }

        private readonly Dictionary<(string normalizedFromPath, string normalizedToPath), int> transitions = new();
    }
}

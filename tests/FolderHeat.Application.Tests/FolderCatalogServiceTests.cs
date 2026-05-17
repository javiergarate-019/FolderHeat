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
    }
}

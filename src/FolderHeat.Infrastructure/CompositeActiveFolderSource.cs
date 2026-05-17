using FolderHeat.Application;

namespace FolderHeat.Infrastructure;

public sealed class CompositeActiveFolderSource : IActiveFolderSource
{
    private readonly IReadOnlyList<IActiveFolderSource> sources;

    public CompositeActiveFolderSource(params IActiveFolderSource[] sources)
    {
        this.sources = sources;
    }

    public async Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default)
    {
        var paths = new List<string>();
        foreach (var source in sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            paths.AddRange(await source.GetActiveFolderPathsAsync(cancellationToken));
        }

        return paths
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(24)
            .ToArray();
    }
}

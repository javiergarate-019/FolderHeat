using FolderHeat.Domain;

namespace FolderHeat.Domain.Tests;

public sealed class FolderHeatScoreTests
{
    [Fact]
    public void PinnedFolderOutranksRecentlyAccessedUnpinnedFolder()
    {
        var now = new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);
        var pinned = new FolderEntry(@"D:\Work", now.AddDays(-10), isPinned: true);
        var recent = new FolderEntry(@"D:\Temp", now.AddMinutes(-1), lastAccessedAt: now.AddMinutes(-1), accessCount: 1);

        var pinnedScore = FolderHeatScore.Calculate(pinned, now);
        var recentScore = FolderHeatScore.Calculate(recent, now);

        Assert.True(pinnedScore > recentScore);
    }

    [Fact]
    public void IgnoredFolderHasNoRankableHeat()
    {
        var now = new DateTimeOffset(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);
        var folder = new FolderEntry(@"D:\Noise", now, isIgnored: true);

        Assert.Equal(double.NegativeInfinity, FolderHeatScore.Calculate(folder, now));
    }
}

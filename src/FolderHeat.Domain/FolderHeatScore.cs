namespace FolderHeat.Domain;

public static class FolderHeatScore
{
    public static double Calculate(FolderEntry folder, DateTimeOffset now)
    {
        if (folder.IsIgnored)
        {
            return double.NegativeInfinity;
        }

        var pinnedBoost = folder.IsPinned ? 1000 : 0;
        var frequencyWeight = Math.Log(folder.AccessCount + 1, 2) * 10;
        var recencyWeight = GetRecencyWeight(folder.LastAccessedAt, now);

        return pinnedBoost + frequencyWeight + recencyWeight;
    }

    private static double GetRecencyWeight(DateTimeOffset? lastAccessedAt, DateTimeOffset now)
    {
        if (lastAccessedAt is null)
        {
            return 0;
        }

        var age = now - lastAccessedAt.Value;
        if (age.TotalMinutes < 5)
        {
            return 100;
        }

        if (age.TotalHours < 1)
        {
            return 80;
        }

        if (age.TotalDays < 1)
        {
            return 50;
        }

        if (age.TotalDays < 7)
        {
            return 25;
        }

        return 5;
    }
}

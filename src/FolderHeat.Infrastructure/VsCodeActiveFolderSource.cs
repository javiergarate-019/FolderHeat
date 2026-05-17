using System.Text.Json;
using FolderHeat.Application;

namespace FolderHeat.Infrastructure;

public sealed class VsCodeActiveFolderSource : IActiveFolderSource
{
    public Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Code",
            "User",
            "globalStorage",
            "storage.json");

        if (!File.Exists(storagePath))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(storagePath));
            if (!document.RootElement.TryGetProperty("openedPathsList", out var openedPaths))
            {
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
            }

            var paths = ReadPathEntries(openedPaths)
                .Select(value => value.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
                    ? ActiveFolderPath.FromUri(value)
                    : ActiveFolderPath.FromPathOrFile(value))
                .Where(path => path is not null)
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToArray();

            return Task.FromResult<IReadOnlyList<string>>(paths);
        }
        catch (JsonException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (IOException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
    }

    private static IEnumerable<string> ReadPathEntries(JsonElement openedPaths)
    {
        foreach (var propertyName in new[] { "entries", "workspaces3", "files2", "folders" })
        {
            if (!openedPaths.TryGetProperty(propertyName, out var entries) || entries.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var entry in entries.EnumerateArray())
            {
                if (entry.ValueKind == JsonValueKind.String)
                {
                    yield return entry.GetString() ?? string.Empty;
                    continue;
                }

                foreach (var valueName in new[] { "folderUri", "workspace", "fileUri", "label" })
                {
                    if (entry.TryGetProperty(valueName, out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        yield return value.GetString() ?? string.Empty;
                    }
                }
            }
        }
    }
}

using System.Xml.Linq;
using FolderHeat.Application;

namespace FolderHeat.Infrastructure;

public sealed class NotepadPlusPlusActiveFolderSource : IActiveFolderSource
{
    public Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sessionPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Notepad++",
            "session.xml");

        if (!File.Exists(sessionPath))
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        try
        {
            var document = XDocument.Load(sessionPath);
            var paths = document
                .Descendants("File")
                .Select(element => element.Attribute("filename")?.Value)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => ActiveFolderPath.FromPathOrFile(path!))
                .Where(path => path is not null)
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToArray();

            return Task.FromResult<IReadOnlyList<string>>(paths);
        }
        catch (IOException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (System.Xml.XmlException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
    }
}

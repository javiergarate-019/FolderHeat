using System.Runtime.InteropServices;
using FolderHeat.Application;
using Microsoft.CSharp.RuntimeBinder;

namespace FolderHeat.Infrastructure;

public sealed class NotepadActiveFolderSource : IActiveFolderSource
{
    public Task<IReadOnlyList<string>> GetActiveFolderPathsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        try
        {
            var locatorType = Type.GetTypeFromProgID("WbemScripting.SWbemLocator");
            if (locatorType is null)
            {
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
            }

            dynamic? locator = Activator.CreateInstance(locatorType);
            if (locator is null)
            {
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
            }

            dynamic services = locator.ConnectServer(".", @"root\cimv2");
            dynamic processes = services.ExecQuery("SELECT CommandLine FROM Win32_Process WHERE Name = 'notepad.exe'");

            var paths = new List<string>();
            foreach (dynamic process in processes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string? commandLine = process.CommandLine;
                if (string.IsNullOrWhiteSpace(commandLine))
                {
                    continue;
                }

                paths.AddRange(GetFolderPathsFromCommandLine(commandLine));
            }

            return Task.FromResult<IReadOnlyList<string>>(paths
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToArray());
        }
        catch (COMException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (RuntimeBinderException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
    }

    private static IEnumerable<string> GetFolderPathsFromCommandLine(string commandLine)
    {
        foreach (var argument in TokenizeCommandLine(commandLine).Skip(1))
        {
            if (argument.StartsWith('/'))
            {
                continue;
            }

            var path = ActiveFolderPath.FromPathOrFile(argument);
            if (path is not null)
            {
                yield return path;
            }
        }
    }

    private static IEnumerable<string> TokenizeCommandLine(string commandLine)
    {
        var current = new List<char>();
        var inQuotes = false;

        foreach (var character in commandLine)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                if (current.Count > 0)
                {
                    yield return new string(current.ToArray());
                    current.Clear();
                }

                continue;
            }

            current.Add(character);
        }

        if (current.Count > 0)
        {
            yield return new string(current.ToArray());
        }
    }
}

using FolderHeat.Application;
using FolderHeat.Domain;
using Microsoft.Data.Sqlite;

namespace FolderHeat.Infrastructure;

public sealed class SqliteFolderRepository : IFolderRepository
{
    private readonly string connectionString;

    public SqliteFolderRepository(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path is required.", nameof(databasePath));
        }

        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS folders (
                path TEXT PRIMARY KEY NOT NULL,
                created_at TEXT NOT NULL,
                last_accessed_at TEXT NULL,
                access_count INTEGER NOT NULL,
                is_pinned INTEGER NOT NULL,
                is_ignored INTEGER NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FolderEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT path, created_at, last_accessed_at, access_count, is_pinned, is_ignored
            FROM folders
            ORDER BY is_pinned DESC, last_accessed_at DESC, path ASC;
            """;

        var folders = new List<FolderEntry>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            folders.Add(ReadFolder(reader));
        }

        return folders;
    }

    public async Task<FolderEntry?> GetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT path, created_at, last_accessed_at, access_count, is_pinned, is_ignored
            FROM folders
            WHERE path = $path;
            """;
        command.Parameters.AddWithValue("$path", FolderEntry.NormalizePath(path));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadFolder(reader) : null;
    }

    public async Task SaveAsync(FolderEntry folder, CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO folders (path, created_at, last_accessed_at, access_count, is_pinned, is_ignored)
            VALUES ($path, $created_at, $last_accessed_at, $access_count, $is_pinned, $is_ignored)
            ON CONFLICT(path) DO UPDATE SET
                created_at = excluded.created_at,
                last_accessed_at = excluded.last_accessed_at,
                access_count = excluded.access_count,
                is_pinned = excluded.is_pinned,
                is_ignored = excluded.is_ignored;
            """;
        command.Parameters.AddWithValue("$path", folder.Path);
        command.Parameters.AddWithValue("$created_at", folder.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$last_accessed_at", folder.LastAccessedAt?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$access_count", folder.AccessCount);
        command.Parameters.AddWithValue("$is_pinned", folder.IsPinned ? 1 : 0);
        command.Parameters.AddWithValue("$is_ignored", folder.IsIgnored ? 1 : 0);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }

    private static FolderEntry ReadFolder(SqliteDataReader reader)
    {
        DateTimeOffset? lastAccessedAt = reader.IsDBNull(2)
            ? null
            : DateTimeOffset.Parse(reader.GetString(2));

        return new FolderEntry(
            reader.GetString(0),
            DateTimeOffset.Parse(reader.GetString(1)),
            lastAccessedAt,
            reader.GetInt32(3),
            reader.GetInt32(4) == 1,
            reader.GetInt32(5) == 1);
    }
}

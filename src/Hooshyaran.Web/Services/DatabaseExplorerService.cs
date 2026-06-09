using System.Diagnostics;
using System.Text.RegularExpressions;
using Hooshyaran.Web.ViewModels;
using Microsoft.Data.Sqlite;

namespace Hooshyaran.Web.Services;

public class DatabaseExplorerService(IConfiguration configuration, IWebHostEnvironment environment) : IDatabaseExplorerService
{
    private static readonly Regex UnsafeSqlPattern = new(
        @"\b(drop|delete|update|insert|alter|create|replace|truncate|attach|detach|vacuum|reindex|pragma|grant|revoke)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly string connectionString = configuration.GetConnectionString("HooshyaranDb") ?? "Data Source=App_Data/hooshyaran.db";

    public string DatabasePath => ResolveDatabasePath();

    public async Task<DatabaseOverview> GetOverviewAsync()
    {
        var tables = await GetTablesAsync();
        var file = new FileInfo(DatabasePath);

        return new DatabaseOverview(
            Path.GetFileName(DatabasePath),
            DatabasePath,
            FormatBytes(file.Exists ? file.Length : 0),
            tables.Count,
            tables.Where(table => !table.IsSystemTable).Sum(table => table.RowCount));
    }

    public async Task<IReadOnlyList<DatabaseTableInfo>> GetTablesAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var names = new List<string>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT name
                FROM sqlite_master
                WHERE type = 'table'
                ORDER BY name;
                """;

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                names.Add(reader.GetString(0));
            }
        }

        var tables = new List<DatabaseTableInfo>();
        foreach (var name in names)
        {
            tables.Add(new DatabaseTableInfo(name, await CountRowsAsync(connection, name), IsSystemTable(name)));
        }

        return tables;
    }

    public async Task<DatabaseGridResult> GetTableDataAsync(string tableName, int page, int pageSize, string? searchTerm)
    {
        page = Math.Max(page, 1);
        pageSize = NormalizePageSize(pageSize);
        searchTerm = searchTerm?.Trim() ?? string.Empty;

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        if (!await TableExistsAsync(connection, tableName))
        {
            return new DatabaseGridResult([], [], 0, page, pageSize, searchTerm);
        }

        var columns = await GetColumnsAsync(connection, tableName);
        var where = BuildSearchWhere(columns, searchTerm);
        var totalRows = await CountRowsAsync(connection, tableName, where.Sql, where.Parameter);
        var rows = new List<IReadOnlyDictionary<string, string>>();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT *
            FROM {QuoteIdentifier(tableName)}
            {where.Sql}
            LIMIT @limit OFFSET @offset;
            """;
        command.Parameters.AddWithValue("@limit", pageSize);
        command.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
        if (where.Parameter is not null)
        {
            command.Parameters.AddWithValue("@search", where.Parameter);
        }

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(ReadRow(reader));
        }

        return new DatabaseGridResult(columns, rows, totalRows, page, pageSize, searchTerm);
    }

    public async Task<DatabaseQueryResult> ExecuteReadOnlyQueryAsync(string query)
    {
        query = query.Trim();
        if (!IsAllowedReadOnlyQuery(query))
        {
            return new DatabaseQueryResult([], [], 0, 0, "فقط queryهای SELECT یا WITH مجاز هستند. دستورهای تغییر دیتابیس مسدود شده‌اند.");
        }

        var rows = new List<IReadOnlyDictionary<string, string>>();
        var columns = new List<string>();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandTimeout = 20;

            await using var reader = await command.ExecuteReaderAsync();
            columns.AddRange(Enumerable.Range(0, reader.FieldCount).Select(reader.GetName));

            while (await reader.ReadAsync() && rows.Count < 500)
            {
                rows.Add(ReadRow(reader));
            }

            stopwatch.Stop();
            return new DatabaseQueryResult(columns, rows, rows.Count, stopwatch.ElapsedMilliseconds, string.Empty);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new DatabaseQueryResult([], [], 0, stopwatch.ElapsedMilliseconds, ex.Message);
        }
    }

    public async Task<string> CreateBackupAsync()
    {
        var backupDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "backups");
        Directory.CreateDirectory(backupDirectory);

        var backupPath = Path.Combine(backupDirectory, $"hooshyaran-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db");
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"VACUUM INTO {QuoteSqlLiteral(backupPath)};";
        await command.ExecuteNonQueryAsync();

        return backupPath;
    }

    public async Task<DatabaseQueryResult> RestoreAsync(IFormFile backupFile)
    {
        var extension = Path.GetExtension(backupFile.FileName);
        var isSupportedFile = extension.Equals(".db", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".sqlite", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".sqlite3", StringComparison.OrdinalIgnoreCase);

        if (backupFile.Length == 0 || !isSupportedFile)
        {
            return new DatabaseQueryResult([], [], 0, 0, "فایل ریستور باید یک فایل SQLite با پسوند db، sqlite یا sqlite3 باشد.");
        }

        var tempPath = Path.Combine(Path.GetTempPath(), $"hooshyaran-restore-{Guid.NewGuid():N}.db");
        await using (var stream = File.Create(tempPath))
        {
            await backupFile.CopyToAsync(stream);
        }

        try
        {
            await ValidateSqliteFileAsync(tempPath);

            var currentPath = DatabasePath;
            var backupBeforeRestore = await CreateBackupAsync();

            DeleteSqliteSidecarFiles(currentPath);
            File.Copy(tempPath, currentPath, overwrite: true);
            DeleteSqliteSidecarFiles(currentPath);

            return new DatabaseQueryResult([], [], 0, 0, $"ریستور انجام شد. بکاپ قبل از ریستور: {backupBeforeRestore}");
        }
        catch (Exception ex)
        {
            return new DatabaseQueryResult([], [], 0, 0, $"ریستور انجام نشد: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private SqliteConnection CreateConnection()
    {
        var builder = new SqliteConnectionStringBuilder(connectionString)
        {
            DataSource = DatabasePath
        };

        return new SqliteConnection(builder.ToString());
    }

    private string ResolveDatabasePath()
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;

        return Path.GetFullPath(Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.Combine(environment.ContentRootPath, dataSource));
    }

    private static async Task<bool> TableExistsAsync(SqliteConnection connection, string tableName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name;";
        command.Parameters.AddWithValue("@name", tableName);

        return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
    }

    private static async Task<IReadOnlyList<DatabaseColumnInfo>> GetColumnsAsync(SqliteConnection connection, string tableName)
    {
        var columns = new List<DatabaseColumnInfo>();
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({QuoteIdentifier(tableName)});";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(new DatabaseColumnInfo(
                reader.GetString(1),
                reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                reader.GetInt32(5) > 0));
        }

        return columns;
    }

    private static async Task<long> CountRowsAsync(
        SqliteConnection connection,
        string tableName,
        string whereSql = "",
        string? searchParameter = null)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {QuoteIdentifier(tableName)} {whereSql};";
        if (searchParameter is not null)
        {
            command.Parameters.AddWithValue("@search", searchParameter);
        }

        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private static (string Sql, string? Parameter) BuildSearchWhere(IReadOnlyList<DatabaseColumnInfo> columns, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || columns.Count == 0)
        {
            return (string.Empty, null);
        }

        var conditions = columns
            .Select(column => $"CAST({QuoteIdentifier(column.Name)} AS TEXT) LIKE @search")
            .ToList();

        return ($"WHERE {string.Join(" OR ", conditions)}", $"%{searchTerm}%");
    }

    private static IReadOnlyDictionary<string, string> ReadRow(SqliteDataReader reader)
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < reader.FieldCount; index++)
        {
            row[reader.GetName(index)] = reader.IsDBNull(index) ? "NULL" : Convert.ToString(reader.GetValue(index)) ?? string.Empty;
        }

        return row;
    }

    private static bool IsAllowedReadOnlyQuery(string query)
    {
        var normalized = query.Trim().TrimEnd(';').TrimStart();

        return !string.IsNullOrWhiteSpace(normalized) &&
            !normalized.Contains(';') &&
            (normalized.StartsWith("select", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("with", StringComparison.OrdinalIgnoreCase)) &&
            !UnsafeSqlPattern.IsMatch(normalized);
    }

    private static async Task ValidateSqliteFileAsync(string path)
    {
        await using var connection = new SqliteConnection($"Data Source={path};Mode=ReadOnly");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table';";
        _ = await command.ExecuteScalarAsync();
    }

    private static void DeleteSqliteSidecarFiles(string databasePath)
    {
        foreach (var suffix in new[] { "-wal", "-shm" })
        {
            var path = databasePath + suffix;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static bool IsSystemTable(string name) =>
        name.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase) ||
        name.StartsWith("__EF", StringComparison.OrdinalIgnoreCase);

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        50 => 50,
        100 => 100,
        _ => 25
    };

    private static string QuoteIdentifier(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    private static string QuoteSqlLiteral(string value) => "'" + value.Replace("'", "''") + "'";

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.#} {units[unit]}";
    }
}

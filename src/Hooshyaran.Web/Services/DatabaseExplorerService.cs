using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Hooshyaran.Web.ViewModels;
using Microsoft.Data.SqlClient;

namespace Hooshyaran.Web.Services;

public class DatabaseExplorerService(IConfiguration configuration, IWebHostEnvironment environment) : IDatabaseExplorerService
{
    private static readonly Regex UnsafeSqlPattern = new(
        @"\b(drop|delete|update|insert|alter|create|replace|truncate|backup|restore|attach|detach|grant|revoke|merge|execute|exec)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly string connectionString = configuration.GetConnectionString("HooshyaranDb")
        ?? throw new InvalidOperationException("ConnectionStrings:HooshyaranDb must be configured.");

    public bool CanRestoreDatabase =>
        environment.IsDevelopment() || configuration.GetValue<bool>("Database:AllowAdminRestore");

    public string DatabasePath => GetDatabaseDisplayName();

    public async Task<DatabaseOverview> GetOverviewAsync()
    {
        var tables = await GetTablesAsync();
        var sizeInBytes = await GetDatabaseSizeInBytesAsync();

        return new DatabaseOverview(
            GetDatabaseName(),
            GetDatabaseDisplayName(),
            FormatBytes(sizeInBytes),
            tables.Count,
            tables.Where(table => !table.IsSystemTable).Sum(table => table.RowCount));
    }

    public async Task<IReadOnlyList<DatabaseTableInfo>> GetTablesAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                QUOTENAME(s.name) + '.' + QUOTENAME(t.name) AS FullName,
                COALESCE(SUM(p.rows), 0) AS RowCount
            FROM sys.tables t
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            LEFT JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
            GROUP BY s.name, t.name
            ORDER BY s.name, t.name;
            """;

        var tables = new List<DatabaseTableInfo>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var name = NormalizeBracketedName(reader.GetString(0));
            tables.Add(new DatabaseTableInfo(name, reader.GetInt64(1), IsSystemTable(name)));
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

        var table = await ResolveTableAsync(connection, tableName);
        if (table is null)
        {
            return new DatabaseGridResult([], [], 0, page, pageSize, searchTerm);
        }

        var columns = await GetColumnsAsync(connection, table.Value.Schema, table.Value.Name);
        var where = BuildSearchWhere(columns, searchTerm);
        var quotedTable = QuoteMultipartIdentifier(table.Value.Schema, table.Value.Name);
        var totalRows = await CountRowsAsync(connection, quotedTable, where.Sql, where.Parameter);
        var rows = new List<IReadOnlyDictionary<string, string>>();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT *
            FROM {quotedTable}
            {where.Sql}
            ORDER BY (SELECT 1)
            OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;
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
            command.CommandTimeout = 30;

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
        var backupDirectory = await GetServerBackupDirectoryAsync();
        Directory.CreateDirectory(backupDirectory);

        var backupPath = Path.Combine(backupDirectory, $"hooshyaran-{GetDatabaseName()}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.bak");
        var databaseName = GetDatabaseName();

        await using var connection = CreateMasterConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 180;
        command.CommandText = $"""
            BACKUP DATABASE {QuoteIdentifier(databaseName)}
            TO DISK = @backupPath
            WITH INIT, COPY_ONLY, CHECKSUM, STATS = 10;
            """;
        command.Parameters.AddWithValue("@backupPath", backupPath);
        await command.ExecuteNonQueryAsync();

        return backupPath;
    }

    public async Task<DatabaseQueryResult> RestoreAsync(IFormFile backupFile)
    {
        if (!CanRestoreDatabase)
        {
            return new DatabaseQueryResult([], [], 0, 0, "ریستور دیتابیس در محیط production غیرفعال است.");
        }

        if (backupFile.Length == 0 ||
            !Path.GetExtension(backupFile.FileName).Equals(".bak", StringComparison.OrdinalIgnoreCase))
        {
            return new DatabaseQueryResult([], [], 0, 0, "فایل ریستور باید یک فایل bak معتبر SQL Server باشد.");
        }

        var restoreDirectory = await GetServerBackupDirectoryAsync();
        Directory.CreateDirectory(restoreDirectory);

        var restorePath = Path.Combine(restoreDirectory, $"restore-{Guid.NewGuid():N}.bak");
        await using (var stream = File.Create(restorePath))
        {
            await backupFile.CopyToAsync(stream);
        }

        try
        {
            var backupBeforeRestore = await CreateBackupAsync();
            await RestoreDatabaseAsync(restorePath);

            return new DatabaseQueryResult([], [], 0, 0, $"ریستور انجام شد. بکاپ قبل از ریستور: {backupBeforeRestore}");
        }
        catch (Exception ex)
        {
            return new DatabaseQueryResult([], [], 0, 0, $"ریستور انجام نشد: {ex.Message}");
        }
        finally
        {
            if (File.Exists(restorePath))
            {
                File.Delete(restorePath);
            }
        }
    }

    private async Task RestoreDatabaseAsync(string backupPath)
    {
        var databaseName = GetDatabaseName();
        SqlConnection.ClearAllPools();

        await using var connection = CreateMasterConnection();
        await connection.OpenAsync();

        await ExecuteMasterCommandAsync(connection, $"""
            ALTER DATABASE {QuoteIdentifier(databaseName)}
            SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            """);

        try
        {
            await using var restoreCommand = connection.CreateCommand();
            restoreCommand.CommandTimeout = 300;
            restoreCommand.CommandText = $"""
                RESTORE DATABASE {QuoteIdentifier(databaseName)}
                FROM DISK = @backupPath
                WITH REPLACE, CHECKSUM, STATS = 10;
                """;
            restoreCommand.Parameters.AddWithValue("@backupPath", backupPath);
            await restoreCommand.ExecuteNonQueryAsync();
        }
        finally
        {
            await ExecuteMasterCommandAsync(connection, $"""
                ALTER DATABASE {QuoteIdentifier(databaseName)}
                SET MULTI_USER;
                """);
        }
    }

    private async Task<string> GetServerBackupDirectoryAsync()
    {
        try
        {
            await using var connection = CreateMasterConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000));";
            var value = Convert.ToString(await command.ExecuteScalarAsync());

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        catch
        {
            // The fallback keeps the admin panel usable on older SQL Server versions.
        }

        return Path.Combine(environment.ContentRootPath, "App_Data", "backups");
    }

    private static async Task ExecuteMasterCommandAsync(SqlConnection connection, string commandText)
    {
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 120;
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync();
    }

    private SqlConnection CreateConnection() => new(connectionString);

    private SqlConnection CreateMasterConnection()
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master",
            MultipleActiveResultSets = false
        };

        return new SqlConnection(builder.ToString());
    }

    private string GetDatabaseName()
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        return string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "HooshyaranWebSite" : builder.InitialCatalog;
    }

    private string GetDatabaseDisplayName()
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        return $"{builder.DataSource} / {GetDatabaseName()}";
    }

    private async Task<long> GetDatabaseSizeInBytesAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(SUM(size), 0) * 8 * 1024 FROM sys.database_files;";

        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private static async Task<(string Schema, string Name)?> ResolveTableAsync(SqlConnection connection, string tableName)
    {
        var (schema, name) = SplitTableName(tableName);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.name, t.name
            FROM sys.tables t
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = @schema AND t.name = @name;
            """;
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@name", name);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return (reader.GetString(0), reader.GetString(1));
    }

    private static async Task<IReadOnlyList<DatabaseColumnInfo>> GetColumnsAsync(SqlConnection connection, string schema, string tableName)
    {
        var columns = new List<DatabaseColumnInfo>();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                c.name,
                CASE
                    WHEN ty.name IN ('varchar', 'char', 'nvarchar', 'nchar')
                        THEN ty.name + '(' + CASE WHEN c.max_length = -1 THEN 'max' ELSE CONVERT(varchar(12), CASE WHEN ty.name LIKE 'n%' THEN c.max_length / 2 ELSE c.max_length END) END + ')'
                    WHEN ty.name IN ('decimal', 'numeric')
                        THEN ty.name + '(' + CONVERT(varchar(12), c.precision) + ',' + CONVERT(varchar(12), c.scale) + ')'
                    ELSE ty.name
                END AS TypeName,
                CASE WHEN pk.column_id IS NULL THEN CONVERT(bit, 0) ELSE CONVERT(bit, 1) END AS IsPrimaryKey
            FROM sys.columns c
            JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            LEFT JOIN (
                SELECT ic.object_id, ic.column_id
                FROM sys.index_columns ic
                JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                WHERE i.is_primary_key = 1
            ) pk ON pk.object_id = c.object_id AND pk.column_id = c.column_id
            WHERE s.name = @schema AND t.name = @table
            ORDER BY c.column_id;
            """;
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@table", tableName);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(new DatabaseColumnInfo(reader.GetString(0), reader.GetString(1), reader.GetBoolean(2)));
        }

        return columns;
    }

    private static async Task<long> CountRowsAsync(
        SqlConnection connection,
        string quotedTableName,
        string whereSql = "",
        string? searchParameter = null)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT_BIG(*) FROM {quotedTableName} {whereSql};";
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
            .Select(column => $"CAST({QuoteIdentifier(column.Name)} AS nvarchar(max)) LIKE @search")
            .ToList();

        return ($"WHERE {string.Join(" OR ", conditions)}", $"%{searchTerm}%");
    }

    private static IReadOnlyDictionary<string, string> ReadRow(DbDataReader reader)
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

    private static (string Schema, string Name) SplitTableName(string tableName)
    {
        var cleaned = tableName
            .Replace("[", string.Empty, StringComparison.Ordinal)
            .Replace("]", string.Empty, StringComparison.Ordinal)
            .Trim();
        var parts = cleaned.Split('.', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length == 2 ? (parts[0], parts[1]) : ("dbo", cleaned);
    }

    private static string NormalizeBracketedName(string name) =>
        name.Replace("[", string.Empty, StringComparison.Ordinal).Replace("]", string.Empty, StringComparison.Ordinal);

    private static bool IsSystemTable(string name) =>
        name.Contains("__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase) ||
        name.StartsWith("sys.", StringComparison.OrdinalIgnoreCase);

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        50 => 50,
        100 => 100,
        _ => 25
    };

    private static string QuoteMultipartIdentifier(string schema, string name) => $"{QuoteIdentifier(schema)}.{QuoteIdentifier(name)}";

    private static string QuoteIdentifier(string value) => "[" + value.Replace("]", "]]", StringComparison.Ordinal) + "]";

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

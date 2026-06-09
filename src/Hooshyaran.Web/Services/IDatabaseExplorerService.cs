using Hooshyaran.Web.ViewModels;

namespace Hooshyaran.Web.Services;

public interface IDatabaseExplorerService
{
    string DatabasePath { get; }

    Task<DatabaseOverview> GetOverviewAsync();

    Task<IReadOnlyList<DatabaseTableInfo>> GetTablesAsync();

    Task<DatabaseGridResult> GetTableDataAsync(string tableName, int page, int pageSize, string? searchTerm);

    Task<DatabaseQueryResult> ExecuteReadOnlyQueryAsync(string query);

    Task<string> CreateBackupAsync();

    Task<DatabaseQueryResult> RestoreAsync(IFormFile backupFile);
}

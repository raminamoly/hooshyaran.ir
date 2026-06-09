using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Hooshyaran.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hooshyaran.Web.Pages.Admin.Database;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class IndexModel(IDatabaseExplorerService databaseExplorer) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string TableName { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 25;

    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    [BindProperty]
    public string QueryText { get; set; } = "SELECT name, type FROM sqlite_master WHERE type = 'table' ORDER BY name;";

    [BindProperty]
    public IFormFile? RestoreFile { get; set; }

    public DatabaseOverview Overview { get; private set; } = new(string.Empty, string.Empty, string.Empty, 0, 0);

    public IReadOnlyList<DatabaseTableInfo> Tables { get; private set; } = [];

    public DatabaseGridResult TableData { get; private set; } = new([], [], 0, 1, 25, string.Empty);

    public DatabaseQueryResult? QueryResult { get; private set; }

    public string? StatusMessage { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadPageAsync();
    }

    public async Task<IActionResult> OnPostQueryAsync()
    {
        await LoadPageAsync();
        QueryResult = await databaseExplorer.ExecuteReadOnlyQueryAsync(QueryText);

        return Page();
    }

    public async Task<IActionResult> OnPostBackupAsync()
    {
        var backupPath = await databaseExplorer.CreateBackupAsync();

        return PhysicalFile(backupPath, "application/vnd.sqlite3", Path.GetFileName(backupPath));
    }

    public async Task<IActionResult> OnPostRestoreAsync()
    {
        if (RestoreFile is null)
        {
            StatusMessage = "فایل بکاپ انتخاب نشده است.";
        }
        else
        {
            QueryResult = await databaseExplorer.RestoreAsync(RestoreFile);
            StatusMessage = QueryResult.ErrorMessage;
        }

        await LoadPageAsync();

        return Page();
    }

    private async Task LoadPageAsync()
    {
        Overview = await databaseExplorer.GetOverviewAsync();
        Tables = await databaseExplorer.GetTablesAsync();

        if (string.IsNullOrWhiteSpace(TableName))
        {
            TableName = Tables.FirstOrDefault(table => !table.IsSystemTable)?.Name ?? Tables.FirstOrDefault()?.Name ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(TableName))
        {
            TableData = await databaseExplorer.GetTableDataAsync(TableName, PageNumber, PageSize, SearchTerm);
        }
    }
}

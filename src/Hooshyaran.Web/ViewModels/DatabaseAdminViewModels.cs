namespace Hooshyaran.Web.ViewModels;

public record DatabaseTableInfo(string Name, long RowCount, bool IsSystemTable);

public record DatabaseColumnInfo(string Name, string Type, bool IsPrimaryKey);

public record DatabaseGridResult(
    IReadOnlyList<DatabaseColumnInfo> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, string>> Rows,
    long TotalRows,
    int Page,
    int PageSize,
    string SearchTerm);

public record DatabaseQueryResult(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, string>> Rows,
    int RowCount,
    long ElapsedMilliseconds,
    string ErrorMessage);

public record DatabaseOverview(
    string DatabaseName,
    string DatabasePath,
    string DatabaseSize,
    int TableCount,
    long ApproximateRecordCount);

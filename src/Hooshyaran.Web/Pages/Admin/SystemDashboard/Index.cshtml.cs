using System.Text.Json;
using System.Text.Json.Serialization;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.SystemDashboard;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    private static readonly TimeSpan TehranOffset = TimeSpan.FromHours(3.5);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [BindProperty(SupportsGet = true)]
    public string Range { get; set; } = "7d";

    [BindProperty(SupportsGet = true)]
    public int? ArticleId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string EventType { get; set; } = "all";

    public DashboardMetric TodayVisitors { get; private set; } = new("کاربران امروز", "0", "ورودهای یکتای امروز");

    public DashboardMetric OnlineVisitors { get; private set; } = new("آنلاین الان", "0", "فعال در ۵ دقیقه اخیر");

    public DashboardMetric TotalViews { get; private set; } = new("کل بازدیدها", "0", "بازدیدهای ثبت‌شده");

    public DashboardMetric DemoSubmits { get; private set; } = new("فرم‌های دمو", "0", "درخواست‌های ثبت‌شده");

    public DashboardMetric DemoConversionRate { get; private set; } = new("نرخ تبدیل دمو", "0٪", "نسبت دمو به بازدید");

    public DashboardMetric ArticleClicks { get; private set; } = new("کلیک مقالات", "0", "باز شدن صفحات مقاله");

    public List<ArticleMetricRow> ArticleRows { get; private set; } = [];

    public List<DemoRequestRow> LatestDemoRequests { get; private set; } = [];

    public List<ActivityRow> LatestActivities { get; private set; } = [];

    public List<ArticleFilterItem> ArticleFilters { get; private set; } = [];

    public string VisitsChartJson { get; private set; } = "{}";

    public string OnlineChartJson { get; private set; } = "{}";

    public string ArticleClicksChartJson { get; private set; } = "{}";

    public string DemoSubmitsChartJson { get; private set; } = "{}";

    public async Task OnGetAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var todayStart = StartOfTehranDay(now);
        var rangeDays = Range switch
        {
            "today" => 1,
            "30d" => 30,
            _ => 7
        };
        var rangeStart = StartOfTehranDay(now.AddDays(-(rangeDays - 1)));
        var previousRangeStart = rangeStart.AddDays(-rangeDays);

        var allLogs = await dbContext.SiteVisitLogs
            .AsNoTracking()
            .ToListAsync();
        var allDemoRequests = await dbContext.DemoRequests
            .AsNoTracking()
            .ToListAsync();
        var scopedLogs = allLogs.Where(log => log.CreatedAt >= rangeStart);

        if (ArticleId.HasValue)
        {
            scopedLogs = scopedLogs.Where(log => log.BlogArticleId == ArticleId);
        }

        if (!string.Equals(EventType, "all", StringComparison.OrdinalIgnoreCase))
        {
            scopedLogs = scopedLogs.Where(log => log.EventType == EventType);
        }

        var totalPageViews = allLogs.Count(log => log.EventType == SiteVisitEventTypes.PageView);
        var todayVisitors = allLogs
            .Where(log => log.CreatedAt >= todayStart)
            .Select(log => log.VisitorKey)
            .Distinct()
            .Count();
        var onlineVisitors = allLogs
            .Where(log => log.CreatedAt >= now.AddMinutes(-5))
            .Select(log => log.VisitorKey)
            .Distinct()
            .Count();
        var demoSubmitCount = allDemoRequests.Count;
        var articleClickCount = allLogs.Count(log => log.EventType == SiteVisitEventTypes.ArticleClick);
        var conversion = totalPageViews == 0 ? 0 : decimal.Round(demoSubmitCount * 100m / totalPageViews, 1);

        TodayVisitors = new("کاربران امروز", ToFa(todayVisitors), "ورودهای یکتای امروز");
        OnlineVisitors = new("آنلاین الان", ToFa(onlineVisitors), "فعال در ۵ دقیقه اخیر");
        TotalViews = new("کل بازدیدها", ToFa(totalPageViews), "بازدیدهای ثبت‌شده");
        DemoSubmits = new("فرم‌های دمو", ToFa(demoSubmitCount), "درخواست‌های ثبت‌شده");
        DemoConversionRate = new("نرخ تبدیل دمو", $"{ToFa(conversion)}٪", "نسبت دمو به بازدید");
        ArticleClicks = new("کلیک مقالات", ToFa(articleClickCount), "باز شدن صفحات مقاله");

        ArticleFilters = await dbContext.BlogArticles
            .AsNoTracking()
            .OrderBy(article => article.Title)
            .Select(article => new ArticleFilterItem(article.Id, article.Title))
            .ToListAsync();

        await LoadArticleRowsAsync(allLogs, rangeStart, previousRangeStart);
        LoadDemoRows(allDemoRequests);
        LoadActivityRows(scopedLogs);
        LoadCharts(allLogs, allDemoRequests, rangeStart, rangeDays);
    }

    private async Task LoadArticleRowsAsync(
        IReadOnlyCollection<SiteVisitLog> allLogs,
        DateTimeOffset rangeStart,
        DateTimeOffset previousRangeStart)
    {
        var articles = await dbContext.BlogArticles
            .AsNoTracking()
            .OrderBy(article => article.Title)
            .Select(article => new { article.Id, article.Title, article.Slug })
            .ToListAsync();

        var currentClicks = allLogs
            .Where(log => log.EventType == SiteVisitEventTypes.ArticleClick &&
                log.CreatedAt >= rangeStart &&
                log.BlogArticleId.HasValue)
            .GroupBy(log => log.BlogArticleId)
            .ToDictionary(group => group.Key!.Value, group => group.Count());

        var previousClicks = allLogs
            .Where(log => log.EventType == SiteVisitEventTypes.ArticleClick &&
                log.CreatedAt >= previousRangeStart &&
                log.CreatedAt < rangeStart &&
                log.BlogArticleId.HasValue)
            .GroupBy(log => log.BlogArticleId)
            .ToDictionary(group => group.Key!.Value, group => group.Count());

        ArticleRows = articles
            .Select(article =>
            {
                var current = currentClicks.GetValueOrDefault(article.Id);
                var previous = previousClicks.GetValueOrDefault(article.Id);
                var growth = previous == 0 ? (current > 0 ? 100 : 0) : decimal.Round((current - previous) * 100m / previous, 1);

                return new ArticleMetricRow(article.Id, article.Title, article.Slug, current, growth);
            })
            .OrderByDescending(row => row.Clicks)
            .ThenBy(row => row.Title)
            .ToList();
    }

    private void LoadDemoRows(IEnumerable<DemoRequest> demoRequests)
    {
        LatestDemoRequests = demoRequests
            .OrderByDescending(request => request.CreatedAt)
            .Take(8)
            .Select(request => new DemoRequestRow(
                request.FullName,
                request.OrganizationName,
                DemoRequestStatuses.ToPersian(request.Status),
                PersianDateFormatter.ToPersianDateTime(request.CreatedAt)))
            .ToList();
    }

    private void LoadActivityRows(IEnumerable<SiteVisitLog> scopedLogs)
    {
        LatestActivities = scopedLogs
            .OrderByDescending(log => log.CreatedAt)
            .Take(12)
            .Select(log => new ActivityRow(
                SiteVisitEventTypes.ToPersian(log.EventType),
                log.PageTitle,
                log.Path,
                log.Device,
                log.Browser,
                log.IpAddress,
                PersianDateFormatter.ToPersianDateTime(log.CreatedAt)))
            .ToList();
    }

    private void LoadCharts(
        IReadOnlyCollection<SiteVisitLog> allLogs,
        IReadOnlyCollection<DemoRequest> allDemoRequests,
        DateTimeOffset rangeStart,
        int rangeDays)
    {
        var labels = Enumerable.Range(0, rangeDays)
            .Select(day => rangeStart.AddDays(day))
            .ToList();

        var pageViews = CountByDay(allLogs, SiteVisitEventTypes.PageView, rangeStart);
        var articleClicks = CountByDay(allLogs, SiteVisitEventTypes.ArticleClick, rangeStart);
        var demoSubmits = allDemoRequests
            .Where(request => request.CreatedAt >= rangeStart)
            .Select(request => request.CreatedAt)
            .GroupBy(createdAt => createdAt.ToOffset(TehranOffset).Date)
            .ToDictionary(group => group.Key, group => group.Count());

        VisitsChartJson = Serialize(new
        {
            labels = labels.Select(PersianDateFormatter.ToPersianDate).ToList(),
            values = labels.Select(day => pageViews.GetValueOrDefault(day.ToOffset(TehranOffset).Date)).ToList()
        });
        OnlineChartJson = Serialize(new
        {
            labels = new[] { "آنلاین", "آفلاین امروز" },
            values = new[] { ParseFaSafe(OnlineVisitors.Value), Math.Max(ParseFaSafe(TodayVisitors.Value) - ParseFaSafe(OnlineVisitors.Value), 0) }
        });
        ArticleClicksChartJson = Serialize(new
        {
            labels = ArticleRows.Take(7).Select(row => row.Title).ToList(),
            values = ArticleRows.Take(7).Select(row => row.Clicks).ToList()
        });
        DemoSubmitsChartJson = Serialize(new
        {
            labels = labels.Select(PersianDateFormatter.ToPersianDate).ToList(),
            values = labels.Select(day => demoSubmits.GetValueOrDefault(day.ToOffset(TehranOffset).Date)).ToList()
        });
    }

    private static Dictionary<DateTime, int> CountByDay(
        IEnumerable<SiteVisitLog> allLogs,
        string eventType,
        DateTimeOffset rangeStart) =>
        allLogs
            .Where(log => log.EventType == eventType && log.CreatedAt >= rangeStart)
            .Select(log => log.CreatedAt)
            .GroupBy(createdAt => createdAt.ToOffset(TehranOffset).Date)
            .ToDictionary(group => group.Key, group => group.Count());

    private static DateTimeOffset StartOfTehranDay(DateTimeOffset value)
    {
        var tehran = value.ToOffset(TehranOffset);

        return new DateTimeOffset(tehran.Year, tehran.Month, tehran.Day, 0, 0, 0, TehranOffset).ToUniversalTime();
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    private static string ToFa(int value) => PersianDateFormatter.ToPersianDigits(value.ToString("N0"));

    private static string ToFa(decimal value) => PersianDateFormatter.ToPersianDigits(value.ToString("0.#"));

    private static int ParseFaSafe(string value)
    {
        var english = value
            .Replace('۰', '0')
            .Replace('۱', '1')
            .Replace('۲', '2')
            .Replace('۳', '3')
            .Replace('۴', '4')
            .Replace('۵', '5')
            .Replace('۶', '6')
            .Replace('۷', '7')
            .Replace('۸', '8')
            .Replace('۹', '9')
            .Replace(",", string.Empty);

        return int.TryParse(english, out var result) ? result : 0;
    }

    public record DashboardMetric(string Label, string Value, string Hint);

    public record ArticleMetricRow(int Id, string Title, string Slug, int Clicks, decimal GrowthPercent);

    public record DemoRequestRow(string FullName, string OrganizationName, string Status, string CreatedAt);

    public record ActivityRow(string EventType, string PageTitle, string Path, string Device, string Browser, string IpAddress, string CreatedAt);

    public record ArticleFilterItem(int Id, string Title);
}

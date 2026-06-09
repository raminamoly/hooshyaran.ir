namespace Hooshyaran.Web.Models;

public class SiteVisitLog
{
    public int Id { get; set; }

    public string VisitorKey { get; set; } = string.Empty;

    public string EventType { get; set; } = SiteVisitEventTypes.PageView;

    public string Path { get; set; } = string.Empty;

    public string PageTitle { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public string Browser { get; set; } = string.Empty;

    public string Device { get; set; } = string.Empty;

    public string Referrer { get; set; } = string.Empty;

    public int? BlogArticleId { get; set; }

    public BlogArticle? BlogArticle { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

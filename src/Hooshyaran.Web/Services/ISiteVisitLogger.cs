namespace Hooshyaran.Web.Services;

public interface ISiteVisitLogger
{
    Task LogAsync(HttpContext httpContext, string eventType, string pageTitle, int? blogArticleId = null);
}

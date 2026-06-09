using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;

namespace Hooshyaran.Web.Services;

public class SiteVisitLogger(HooshyaranDbContext dbContext) : ISiteVisitLogger
{
    public async Task LogAsync(HttpContext httpContext, string eventType, string pageTitle, int? blogArticleId = null)
    {
        var request = httpContext.Request;
        var visitorKey = GetOrCreateVisitorKey(httpContext);

        dbContext.SiteVisitLogs.Add(new SiteVisitLog
        {
            VisitorKey = visitorKey,
            EventType = eventType,
            Path = $"{request.Path}{request.QueryString}",
            PageTitle = pageTitle,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = request.Headers.UserAgent.ToString(),
            Browser = DetectBrowser(request.Headers.UserAgent.ToString()),
            Device = DetectDevice(request.Headers.UserAgent.ToString()),
            Referrer = request.Headers.Referer.ToString(),
            BlogArticleId = blogArticleId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static string GetOrCreateVisitorKey(HttpContext httpContext)
    {
        const string cookieName = "Hooshyaran.Visitor";

        if (httpContext.Request.Cookies.TryGetValue(cookieName, out var existing) && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var visitorKey = Guid.NewGuid().ToString("N");
        httpContext.Response.Cookies.Append(cookieName, visitorKey, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = httpContext.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });

        return visitorKey;
    }

    private static string DetectBrowser(string userAgent)
    {
        if (userAgent.Contains("Edg", StringComparison.OrdinalIgnoreCase))
        {
            return "Microsoft Edge";
        }

        if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
        {
            return "Chrome";
        }

        if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
        {
            return "Firefox";
        }

        if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase))
        {
            return "Safari";
        }

        return "نامشخص";
    }

    private static string DetectDevice(string userAgent)
    {
        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
        {
            return "موبایل";
        }

        if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
        {
            return "تبلت";
        }

        return "دسکتاپ";
    }
}

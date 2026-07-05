using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;

namespace Hooshyaran.Web.Middleware;

public class SiteVisitLoggingMiddleware(RequestDelegate next)
{
    private static readonly string[] IgnoredPathPrefixes =
    [
        "/css",
        "/js",
        "/lib",
        "/assets",
        "/favicon",
        "/admin"
    ];

    public async Task InvokeAsync(HttpContext context, ISiteVisitLogger visitLogger)
    {
        await next(context);

        if (!ShouldLog(context))
        {
            return;
        }

        try
        {
            await visitLogger.LogAsync(context, SiteVisitEventTypes.PageView, ResolvePageTitle(context.Request.Path));
        }
        catch
        {
            // Analytics must not block the public website.
        }
    }

    private static bool ShouldLog(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) || context.Response.StatusCode >= 400)
        {
            return false;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        return !IgnoredPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolvePageTitle(PathString path) => path.Value?.Trim('/').ToLowerInvariant() switch
    {
        "" => "صفحه اصلی",
        "blog" => "فهرست مقالات",
        "products" => "محصولات",
        "solutions" => "راهکارها",
        "use-cases" => "کاربردها",
        "request-demo" => "درخواست دمو",
        "contact" => "تماس",
        "about" => "درباره ما",
        var value when value?.StartsWith("blog/", StringComparison.OrdinalIgnoreCase) == true => "مقاله",
        _ => path.Value ?? "/"
    };
}

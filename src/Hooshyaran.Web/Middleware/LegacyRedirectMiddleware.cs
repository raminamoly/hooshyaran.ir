using System.Text.Json;

namespace Hooshyaran.Web.Middleware;

public sealed class LegacyRedirectMiddleware
{
    private const string RedirectsFilePath = "Configuration/legacy-redirects.json";

    private readonly RequestDelegate next;
    private readonly IReadOnlyDictionary<string, string> redirects;
    private readonly ILogger<LegacyRedirectMiddleware> logger;

    public LegacyRedirectMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger<LegacyRedirectMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
        redirects = LoadRedirects(Path.Combine(environment.ContentRootPath, RedirectsFilePath));
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (IsRedirectableMethod(context.Request.Method)
            && TryGetDestination(context.Request.Path.Value, out var destination))
        {
            context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            context.Response.Headers.Location = destination;
            return Task.CompletedTask;
        }

        return next(context);
    }

    private static bool IsRedirectableMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method);

    private bool TryGetDestination(string? requestPath, out string destination)
    {
        var normalizedPath = NormalizePath(requestPath);

        if (redirects.TryGetValue(normalizedPath, out var configuredDestination)
            && !string.Equals(normalizedPath, NormalizePath(configuredDestination), StringComparison.OrdinalIgnoreCase))
        {
            destination = configuredDestination;
            return true;
        }

        destination = string.Empty;
        return false;
    }

    private IReadOnlyDictionary<string, string> LoadRedirects(string filePath)
    {
        if (!File.Exists(filePath))
        {
            logger.LogWarning("Legacy redirect file was not found at {FilePath}", filePath);
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var rules = JsonSerializer.Deserialize<List<LegacyRedirectRule>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? [];

            return rules
                .Where(rule => !string.IsNullOrWhiteSpace(rule.Source)
                    && !string.IsNullOrWhiteSpace(rule.Destination)
                    && IsLocalPath(rule.Destination))
                .GroupBy(rule => NormalizePath(rule.Source), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => NormalizeDestination(group.Last().Destination),
                    StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Legacy redirect file has invalid JSON: {FilePath}", filePath);
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static bool IsLocalPath(string value) =>
        value.StartsWith('/') && !value.StartsWith("//", StringComparison.Ordinal);

    private static string NormalizePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "/";
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized.Length > 1
            ? normalized.TrimEnd('/').ToLowerInvariant()
            : normalized;
    }

    private static string NormalizeDestination(string value)
    {
        var destination = value.Trim();
        return destination.StartsWith('/') ? destination : "/" + destination;
    }

    private sealed record LegacyRedirectRule
    {
        public string Source { get; init; } = string.Empty;

        public string Destination { get; init; } = string.Empty;

        public string Note { get; init; } = string.Empty;
    }
}

public static class LegacyRedirectMiddlewareExtensions
{
    public static IApplicationBuilder UseLegacyRedirects(this IApplicationBuilder app) =>
        app.UseMiddleware<LegacyRedirectMiddleware>();
}

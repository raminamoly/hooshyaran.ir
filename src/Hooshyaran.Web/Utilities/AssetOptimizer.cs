namespace Hooshyaran.Web.Utilities;

public static class AssetOptimizer
{
    private static readonly IReadOnlyDictionary<string, string> ThumbnailOverrides =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["/uploads/media/imported/content/llmops-private-ai-architecture-hero-v2.jpg"] = "/uploads/media/imported/content/llmops-private-ai-architecture-hero-v2-thumb.webp",
            ["/uploads/media/imported/content/employee-monitor-ai-dashboard.jpg"] = "/uploads/media/imported/content/employee-monitor-ai-dashboard-thumb.webp",
            ["/uploads/media/imported/content/complaint-management-ai-dashboard.jpg"] = "/uploads/media/imported/content/complaint-management-ai-dashboard-thumb.webp",
            ["/uploads/media/imported/content/ai-bi-natural-manager-hero-final.jpg"] = "/uploads/media/imported/content/ai-bi-natural-manager-hero-final-thumb.webp",
        };

    public static string Image(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        if (path.StartsWith("/assets/content/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat(path.AsSpan(0, path.Length - 4), ".jpg");
        }

        if (path.StartsWith("/uploads/media/imported/content/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat(path.AsSpan(0, path.Length - 4), ".webp");
        }

        return path;
    }

    public static string Thumbnail(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        if (ThumbnailOverrides.TryGetValue(path, out var thumbnailPath))
        {
            return thumbnailPath;
        }

        if (path.StartsWith("/uploads/media/imported/content/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat(path.AsSpan(0, path.Length - 4), "-thumb.webp");
        }

        if (path.StartsWith("/uploads/media/imported/content/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
            && !path.EndsWith("-thumb.webp", StringComparison.OrdinalIgnoreCase))
        {
            return string.Concat(path.AsSpan(0, path.Length - 5), "-thumb.webp");
        }

        return Image(path);
    }
}

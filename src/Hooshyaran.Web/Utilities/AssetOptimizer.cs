namespace Hooshyaran.Web.Utilities;

public static class AssetOptimizer
{
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

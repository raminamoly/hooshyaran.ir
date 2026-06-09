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

        return path;
    }
}

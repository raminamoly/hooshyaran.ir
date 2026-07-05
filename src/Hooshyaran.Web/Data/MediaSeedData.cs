using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Data;

public static class MediaSeedData
{
    private static readonly HashSet<string> ImportableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".svg",
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".zip"
    };

    public static async Task ImportExistingMediaAsync(HooshyaranDbContext dbContext, IWebHostEnvironment environment)
    {
        var referencedAssets = await GetReferencedAssetFileNamesAsync(dbContext);
        ImportFolder(environment, "assets/content", "content", referencedAssets.Content);
        ImportFolder(environment, "assets/brand", "brand", referencedAssets.Brand);
        await UpdateDatabaseMediaPathsAsync(dbContext);
    }

    private static void ImportFolder(
        IWebHostEnvironment environment,
        string sourceRelativePath,
        string targetFolder,
        IReadOnlySet<string> referencedFileNames)
    {
        var sourceRoot = Path.Combine(environment.WebRootPath, sourceRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(sourceRoot) || referencedFileNames.Count == 0)
        {
            return;
        }

        var targetRoot = Path.Combine(environment.WebRootPath, "uploads", "media", "imported", targetFolder);
        Directory.CreateDirectory(targetRoot);

        foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot))
        {
            if (!ImportableExtensions.Contains(Path.GetExtension(sourcePath)) ||
                !referencedFileNames.Contains(Path.GetFileName(sourcePath)))
            {
                continue;
            }

            var targetPath = Path.Combine(targetRoot, Path.GetFileName(sourcePath));
            if (!File.Exists(targetPath))
            {
                File.Copy(sourcePath, targetPath);
            }
        }
    }

    private static async Task<(IReadOnlySet<string> Content, IReadOnlySet<string> Brand)> GetReferencedAssetFileNamesAsync(HooshyaranDbContext dbContext)
    {
        var content = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var brand = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var articleAssets = await dbContext.BlogArticles
            .Select(article => article.ImagePath)
            .ToListAsync();
        foreach (var imagePath in articleAssets)
        {
            AddAssetReference(imagePath, content, brand);
        }

        var productAssets = await dbContext.Products
            .Select(product => new
            {
                product.HeroImagePath,
                product.LogoPath
            })
            .ToListAsync();
        foreach (var product in productAssets)
        {
            AddAssetReference(product.HeroImagePath, content, brand);
            AddAssetReference(product.LogoPath, content, brand);
        }

        return (content, brand);
    }

    private static void AddAssetReference(string? path, HashSet<string> content, HashSet<string> brand)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (path.StartsWith("/assets/content/", StringComparison.OrdinalIgnoreCase))
        {
            content.Add(Path.GetFileName(path));
        }
        else if (path.StartsWith("/assets/brand/", StringComparison.OrdinalIgnoreCase))
        {
            brand.Add(Path.GetFileName(path));
        }
    }

    private static async Task UpdateDatabaseMediaPathsAsync(HooshyaranDbContext dbContext)
    {
        var changed = false;
        var articles = await dbContext.BlogArticles
            .Where(article => article.ImagePath.StartsWith("/assets/content/"))
            .ToListAsync();

        foreach (var article in articles)
        {
            article.ImagePath = MapImportedUrl(article.ImagePath);
            changed = true;
        }

        var products = await dbContext.Products
            .Where(product =>
                product.HeroImagePath.StartsWith("/assets/content/") ||
                product.LogoPath.StartsWith("/assets/content/") ||
                product.LogoPath.StartsWith("/assets/brand/"))
            .ToListAsync();

        foreach (var product in products)
        {
            product.HeroImagePath = MapImportedUrl(product.HeroImagePath);
            product.LogoPath = MapImportedUrl(product.LogoPath);
            changed = true;
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync();
        }
    }

    private static string MapImportedUrl(string path)
    {
        if (path.StartsWith("/assets/content/", StringComparison.OrdinalIgnoreCase))
        {
            return "/uploads/media/imported/content/" + Path.GetFileName(path);
        }

        if (path.StartsWith("/assets/brand/", StringComparison.OrdinalIgnoreCase))
        {
            return "/uploads/media/imported/brand/" + Path.GetFileName(path);
        }

        return path;
    }
}

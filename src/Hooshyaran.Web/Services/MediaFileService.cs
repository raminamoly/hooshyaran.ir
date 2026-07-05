using System.Text.RegularExpressions;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Services;

public class MediaFileService(IWebHostEnvironment environment, HooshyaranDbContext dbContext) : IMediaFileService
{
    private const string UploadPath = "uploads/media";
    private const string UploadFolder = "uploads";

    private static readonly Regex UnsafeFileNameChars = new(@"[^\p{L}\p{N}\-_]+", RegexOptions.Compiled);

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".svg"
    };

    private static readonly Dictionary<string, string[]> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"],
        [".png"] = ["image/png"],
        [".webp"] = ["image/webp"],
        [".gif"] = ["image/gif"],
        [".svg"] = ["image/svg+xml", "text/xml", "application/xml"]
    };

    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".svg"] = "image/svg+xml",
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".zip"] = "application/zip"
    };

    private static readonly HashSet<string> UploadExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".zip"
    };

    public long MaxFileSize { get; } = 10 * 1024 * 1024;

    public IReadOnlyCollection<string> AllowedExtensions { get; } = UploadExtensions;

    public async Task<MediaLibraryResult> ListAsync(string? searchTerm = null, string? fileType = null, string? usageFilter = null)
    {
        var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
        var normalizedType = NormalizeFileType(fileType);
        var normalizedUsage = NormalizeUsageFilter(usageFilter);
        var root = EnsureUploadRoot();
        var metadata = await dbContext.MediaAssets
            .AsNoTracking()
            .ToDictionaryAsync(asset => asset.Url, StringComparer.OrdinalIgnoreCase);
        var usage = await BuildUsageMapAsync();
        var files = Directory
            .EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(file => ToMediaFileItem(file, metadata, usage))
            .ToList();
        var duplicateGroups = files
            .Where(file => !string.IsNullOrWhiteSpace(file.Hash))
            .GroupBy(file => file.Hash, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        files = files
            .Select(file => duplicateGroups.TryGetValue(file.Hash, out var count)
                ? file with { HasDuplicate = true, DuplicateCount = count }
                : file)
            .Where(file => string.IsNullOrWhiteSpace(normalizedSearch) ||
                file.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                file.Url.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                file.AltText.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                file.Title.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                file.Description.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                file.SeoDescription.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
            .Where(file => normalizedType switch
            {
                "images" => file.IsImage,
                "documents" => !file.IsImage,
                _ => true
            })
            .ToList();

        var totalCount = files.Count;
        var usedCount = files.Count(file => file.IsUsed);
        var unusedCount = files.Count(file => !file.IsUsed);
        var duplicateCount = files.Count(file => file.HasDuplicate);

        files = files
            .Where(file => normalizedUsage switch
            {
                "used" => file.IsUsed,
                "unused" => !file.IsUsed,
                "duplicates" => file.HasDuplicate,
                _ => true
            })
            .OrderByDescending(file => file.LastModified)
            .ThenBy(file => file.Name)
            .ToList();

        return new MediaLibraryResult(
            files,
            normalizedSearch,
            normalizedType,
            normalizedUsage,
            totalCount,
            usedCount,
            unusedCount,
            duplicateCount);
    }

    public async Task<MediaFileItem> UploadAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidateUpload(file);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var root = EnsureUploadRoot();
        var relativeFolder = Path.Combine(
            ImageExtensions.Contains(extension) ? "images" : "files",
            DateTime.UtcNow.ToString("yyyy"),
            DateTime.UtcNow.ToString("MM"));
        var targetDirectory = Path.Combine(root, relativeFolder);
        Directory.CreateDirectory(targetDirectory);

        var fileName = BuildSafeFileName(file.FileName, extension);
        var targetPath = Path.Combine(targetDirectory, fileName);

        while (File.Exists(targetPath))
        {
            fileName = BuildSafeFileName(file.FileName, extension);
            targetPath = Path.Combine(targetDirectory, fileName);
        }

        await using var output = File.Create(targetPath);
        await file.CopyToAsync(output, cancellationToken);

        var item = ToMediaFileItem(
            targetPath,
            new Dictionary<string, MediaAsset>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase));
        await SaveMetadataAsync(new MediaMetadataInput
        {
            Url = item.Url,
            AltText = Path.GetFileNameWithoutExtension(item.Name),
            Title = Path.GetFileNameWithoutExtension(item.Name)
        });

        return item;
    }

    public async Task SaveMetadataAsync(MediaMetadataInput input)
    {
        var url = NormalizeUrl(input.Url);
        if (!TryResolveManagedPath(url, out var fullPath) || !File.Exists(fullPath))
        {
            throw new InvalidOperationException("مسیر فایل معتبر نیست.");
        }

        var now = DateTimeOffset.UtcNow;
        var asset = await dbContext.MediaAssets.SingleOrDefaultAsync(item => item.Url == url);
        if (asset is null)
        {
            asset = new MediaAsset
            {
                Url = url,
                CreatedAt = now
            };
            dbContext.MediaAssets.Add(asset);
        }

        asset.Name = Path.GetFileName(fullPath);
        asset.AltText = NormalizeMetadataText(input.AltText, 220);
        asset.Title = NormalizeMetadataText(input.Title, 220);
        asset.Description = NormalizeMetadataText(input.Description, 700);
        asset.SeoDescription = NormalizeMetadataText(input.SeoDescription, 320);
        asset.UpdatedAt = now;

        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string url)
    {
        if (!TryResolveManagedPath(url, out var fullPath) || !File.Exists(fullPath))
        {
            return false;
        }

        File.Delete(fullPath);
        var normalizedUrl = NormalizeUrl(url);
        await dbContext.MediaAssets
            .Where(asset => asset.Url == normalizedUrl)
            .ExecuteDeleteAsync();

        return true;
    }

    public async Task<int> DeleteUnusedAsync(CancellationToken cancellationToken = default)
    {
        var library = await ListAsync(usageFilter: "unused");
        var deletedCount = 0;

        foreach (var file in library.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (file.IsUsed || !IsManagedMediaUrl(file.Url))
            {
                continue;
            }

            if (await DeleteAsync(file.Url))
            {
                deletedCount++;
            }
        }

        return deletedCount;
    }

    public bool IsManagedMediaUrl(string? url) => TryResolveManagedPath(url ?? string.Empty, out _);

    private void ValidateUpload(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("فایل انتخاب نشده است.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new InvalidOperationException("حداکثر حجم فایل ۱۰ مگابایت است.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !UploadExtensions.Contains(extension))
        {
            throw new InvalidOperationException("پسوند فایل مجاز نیست.");
        }

        if (ImageExtensions.Contains(extension) &&
            (!ImageContentTypes.TryGetValue(extension, out var allowedContentTypes) ||
                !allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("نوع فایل تصویر با پسوند آن هم‌خوانی ندارد.");
        }
    }

    private string EnsureUploadRoot()
    {
        var root = Path.Combine(environment.WebRootPath, UploadFolder, "media");
        Directory.CreateDirectory(root);

        return root;
    }

    private MediaFileItem ToMediaFileItem(
        string fullPath,
        IReadOnlyDictionary<string, MediaAsset> metadata,
        IReadOnlyDictionary<string, IReadOnlyList<string>> usage)
    {
        var fileInfo = new FileInfo(fullPath);
        var extension = fileInfo.Extension.ToLowerInvariant();
        var relativePath = Path.GetRelativePath(environment.WebRootPath, fullPath)
            .Replace(Path.DirectorySeparatorChar, '/');
        var url = "/" + relativePath.TrimStart('/');
        metadata.TryGetValue(url, out var asset);
        usage.TryGetValue(url, out var usedIn);
        usedIn ??= [];

        return new MediaFileItem(
            fileInfo.Name,
            url,
            extension.TrimStart('.').ToUpperInvariant(),
            ContentTypes.GetValueOrDefault(extension, "application/octet-stream"),
            FormatBytes(fileInfo.Length),
            fileInfo.Length,
            fileInfo.LastWriteTimeUtc,
            ImageExtensions.Contains(extension),
            asset?.AltText ?? string.Empty,
            asset?.Title ?? string.Empty,
            asset?.Description ?? string.Empty,
            asset?.SeoDescription ?? string.Empty,
            ComputeSha256(fullPath),
            usedIn.Count > 0,
            usedIn.Count,
            usedIn,
            false,
            0);
    }

    private bool TryResolveManagedPath(string url, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var pathOnly = url.Split('?', '#')[0];
        if (!pathOnly.StartsWith("/" + UploadPath + "/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        pathOnly = Uri.UnescapeDataString(pathOnly).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var candidatePath = Path.GetFullPath(Path.Combine(environment.WebRootPath, pathOnly));
        var root = Path.GetFullPath(EnsureUploadRoot());
        if (!candidatePath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        fullPath = candidatePath;

        return true;
    }

    private async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> BuildUsageMapAsync()
    {
        var files = Directory
            .EnumerateFiles(EnsureUploadRoot(), "*", SearchOption.AllDirectories)
            .Select(file => "/" + Path.GetRelativePath(environment.WebRootPath, file).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var usage = files.ToDictionary(
            url => url,
            _ => new List<string>(),
            StringComparer.OrdinalIgnoreCase);

        var articles = await dbContext.BlogArticles
            .AsNoTracking()
            .Select(article => new { article.Id, article.Title, article.ImagePath, article.Body })
            .ToListAsync();
        foreach (var article in articles)
        {
            AddUsage(usage, article.ImagePath, $"مقاله: {article.Title}");
            AddTextUsage(usage, article.Body, $"متن مقاله: {article.Title}");
        }

        var products = await dbContext.Products
            .AsNoTracking()
            .Select(product => new
            {
                product.Id,
                product.PersianTitle,
                product.HeroImagePath,
                product.LogoPath,
                product.ShortDescription,
                product.LongDescription,
                product.ProblemsSolved,
                product.Benefits,
                product.PublicFeatures,
                product.TargetAudience,
                product.UseCases
            })
            .ToListAsync();
        foreach (var product in products)
        {
            AddUsage(usage, product.HeroImagePath, $"تصویر محصول: {product.PersianTitle}");
            AddUsage(usage, product.LogoPath, $"نشان محصول: {product.PersianTitle}");
            AddTextUsage(usage, product.ShortDescription, $"متن محصول: {product.PersianTitle}");
            AddTextUsage(usage, product.LongDescription, $"متن محصول: {product.PersianTitle}");
            AddTextUsage(usage, product.ProblemsSolved, $"متن محصول: {product.PersianTitle}");
            AddTextUsage(usage, product.Benefits, $"متن محصول: {product.PersianTitle}");
            AddTextUsage(usage, product.PublicFeatures, $"متن محصول: {product.PersianTitle}");
            AddTextUsage(usage, product.TargetAudience, $"متن محصول: {product.PersianTitle}");
            AddTextUsage(usage, product.UseCases, $"متن محصول: {product.PersianTitle}");
        }

        var pages = await dbContext.StaticPages
            .AsNoTracking()
            .Select(page => new { page.Id, page.Title, page.Summary, page.Body })
            .ToListAsync();
        foreach (var page in pages)
        {
            AddTextUsage(usage, page.Summary, $"خلاصه صفحه: {page.Title}");
            AddTextUsage(usage, page.Body, $"متن صفحه: {page.Title}");
        }

        return usage
            .Where(item => item.Value.Count > 0)
            .ToDictionary(
                item => item.Key,
                item => (IReadOnlyList<string>)item.Value.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                StringComparer.OrdinalIgnoreCase);
    }

    private static void AddTextUsage(Dictionary<string, List<string>> usage, string? text, string label)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        foreach (var url in usage.Keys)
        {
            if (text.Contains(url, StringComparison.OrdinalIgnoreCase))
            {
                AddUsage(usage, url, label);
            }
        }
    }

    private static void AddUsage(Dictionary<string, List<string>> usage, string? url, string label)
    {
        var normalizedUrl = NormalizeUrl(url ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedUrl) || !usage.TryGetValue(normalizedUrl, out var usedIn))
        {
            return;
        }

        if (!usedIn.Contains(label, StringComparer.OrdinalIgnoreCase))
        {
            usedIn.Add(label);
        }
    }

    private static string BuildSafeFileName(string originalName, string extension)
    {
        var baseName = Path.GetFileNameWithoutExtension(originalName).Trim();
        baseName = UnsafeFileNameChars.Replace(baseName, "-").Trim('-', '_');
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "file";
        }

        if (baseName.Length > 60)
        {
            baseName = baseName[..60].Trim('-', '_');
        }

        return $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}-{baseName}{extension}";
    }

    private static string NormalizeFileType(string? fileType) =>
        fileType?.Trim().ToLowerInvariant() switch
        {
            "images" => "images",
            "documents" => "documents",
            _ => "all"
        };

    private static string NormalizeUsageFilter(string? usageFilter) =>
        usageFilter?.Trim().ToLowerInvariant() switch
        {
            "used" => "used",
            "unused" => "unused",
            "duplicates" => "duplicates",
            _ => "all"
        };

    private static string NormalizeUrl(string url)
    {
        var value = url.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Split('?', '#')[0].Replace('\\', '/');
    }

    private static string NormalizeMetadataText(string value, int maxLength)
    {
        value = Regex.Replace(value.Trim(), @"\s+", " ");

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string ComputeSha256(string fullPath)
    {
        using var stream = File.OpenRead(fullPath);
        var hash = System.Security.Cryptography.SHA256.HashData(stream);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.#} {units[unit]}";
    }
}

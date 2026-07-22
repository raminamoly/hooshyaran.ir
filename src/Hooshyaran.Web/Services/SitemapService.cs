using System.Text;
using System.Xml;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Services;

public class SitemapService(HooshyaranDbContext dbContext) : ISitemapService
{
    private static readonly IReadOnlyList<SitemapUrl> StaticUrls =
    [
        new("/", "daily", "1.0"),
        new("/products", "weekly", "0.9"),
        new("/blog", "weekly", "0.8"),
        new("/solutions", "monthly", "0.7"),
        new("/use-cases", "monthly", "0.7"),
        new("/ai-integrator", "monthly", "0.8"),
        new("/enterprise-ai-implementation", "monthly", "0.8"),
        new("/enterprise-ai-chatbot", "monthly", "0.8"),
        new("/ai-knowledge-base", "monthly", "0.8"),
        new("/private-enterprise-ai", "monthly", "0.8"),
        new("/request-demo", "monthly", "0.6"),
        new("/about", "monthly", "0.5"),
        new("/contact", "monthly", "0.5"),
        new("/privacy", "yearly", "0.2")
    ];

    private static readonly string[] PublicStaticPageSlugs = ["about", "contact", "solutions"];

    public async Task<SitemapBuildResult> BuildAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = await GetBaseUrlAsync(cancellationToken);
        var urls = new List<SitemapUrl>(StaticUrls);

        urls.AddRange(await dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderBy(product => product.SortOrder)
            .ThenBy(product => product.Slug)
            .Select(product => new SitemapUrl($"/products/{product.Slug}", "weekly", "0.8"))
            .ToListAsync(cancellationToken));

        urls.AddRange(await dbContext.BlogArticles
            .AsNoTracking()
            .Where(article => article.IsPublished)
            .OrderByDescending(article => article.PublishedAt)
            .Select(article => new SitemapUrl($"/blog/{article.Slug}", "monthly", "0.7", article.PublishedAt.UtcDateTime))
            .ToListAsync(cancellationToken));

        urls.AddRange(await dbContext.Tags
            .AsNoTracking()
            .Where(tag =>
                tag.BlogArticleTags.Any(item => item.BlogArticle != null && item.BlogArticle.IsPublished) ||
                tag.ProductTags.Any(item => item.Product != null && item.Product.IsActive) ||
                tag.StaticPageTags.Any(item =>
                    item.StaticPage != null &&
                    item.StaticPage.IsPublished &&
                    PublicStaticPageSlugs.Contains(item.StaticPage.Slug)))
            .OrderBy(tag => tag.Slug)
            .Select(tag => new SitemapUrl($"/tags/{tag.Slug}", "monthly", "0.4", tag.UpdatedAt.UtcDateTime))
            .ToListAsync(cancellationToken));

        var distinctUrls = urls
            .Where(item => !string.IsNullOrWhiteSpace(item.Path))
            .DistinctBy(item => item.Path.TrimEnd('/').ToLowerInvariant())
            .ToList();

        return new SitemapBuildResult(BuildXml(baseUrl, distinctUrls), distinctUrls.Count, baseUrl);
    }

    public Task<SitemapSnapshot?> GetLatestSnapshotAsync(CancellationToken cancellationToken = default) =>
        dbContext.SitemapSnapshots
            .AsNoTracking()
            .OrderByDescending(snapshot => snapshot.GeneratedAt)
            .ThenByDescending(snapshot => snapshot.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<SitemapSnapshot> RegenerateAsync(CancellationToken cancellationToken = default)
    {
        var result = await BuildAsync(cancellationToken);
        var snapshot = new SitemapSnapshot
        {
            Xml = result.Xml,
            UrlCount = result.UrlCount,
            BaseUrl = result.BaseUrl,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        dbContext.SitemapSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot;
    }

    private async Task<string> GetBaseUrlAsync(CancellationToken cancellationToken)
    {
        var configuredUrl = await dbContext.SiteSettings
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => item.WebsiteUrl)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(configuredUrl)
            ? "https://hooshyaran.ir"
            : configuredUrl.Trim().TrimEnd('/');
    }

    private static string BuildXml(string baseUrl, IEnumerable<SitemapUrl> urls)
    {
        using var stream = new MemoryStream();
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Async = false,
            Encoding = new UTF8Encoding(false),
            Indent = true,
            OmitXmlDeclaration = false
        });

        writer.WriteStartDocument();
        writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

        foreach (var url in urls)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", $"{baseUrl}/{url.Path.TrimStart('/')}");
            if (url.LastModified is not null)
            {
                writer.WriteElementString("lastmod", url.LastModified.Value.ToString("yyyy-MM-dd"));
            }
            writer.WriteElementString("changefreq", url.ChangeFrequency);
            writer.WriteElementString("priority", url.Priority);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private sealed record SitemapUrl(string Path, string ChangeFrequency, string Priority, DateTime? LastModified = null);
}

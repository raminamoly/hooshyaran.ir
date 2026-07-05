using System.Text;
using System.Xml;
using Hooshyaran.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages;

public class SitemapModel(HooshyaranDbContext dbContext) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var baseUrl = await GetBaseUrlAsync();
        var urls = new List<SitemapUrl>
        {
            new("/", "daily", "1.0"),
            new("/products", "weekly", "0.9"),
            new("/blog", "weekly", "0.8"),
            new("/solutions", "monthly", "0.7"),
            new("/use-cases", "monthly", "0.7"),
            new("/ai-integrator", "monthly", "0.8"),
            new("/request-demo", "monthly", "0.6"),
            new("/about", "monthly", "0.5"),
            new("/contact", "monthly", "0.5")
        };

        urls.AddRange(await dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .OrderBy(product => product.SortOrder)
            .Select(product => new SitemapUrl($"/products/{product.Slug}", "weekly", "0.8"))
            .ToListAsync());

        urls.AddRange(await dbContext.BlogArticles
            .AsNoTracking()
            .Where(article => article.IsPublished)
            .OrderByDescending(article => article.PublishedAt)
            .Select(article => new SitemapUrl($"/blog/{article.Slug}", "monthly", "0.7", article.PublishedAt.UtcDateTime))
            .ToListAsync());

        urls.AddRange(await dbContext.Tags
            .AsNoTracking()
            .Where(tag => tag.BlogArticleTags.Any() || tag.ProductTags.Any() || tag.StaticPageTags.Any())
            .OrderBy(tag => tag.Slug)
            .Select(tag => new SitemapUrl($"/tags/{tag.Slug}", "monthly", "0.4", tag.UpdatedAt.UtcDateTime))
            .ToListAsync());

        return Content(BuildXml(baseUrl, urls.DistinctBy(item => item.Path)), "application/xml; charset=utf-8");
    }

    private async Task<string> GetBaseUrlAsync()
    {
        var configuredUrl = await dbContext.SiteSettings
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => item.WebsiteUrl)
            .FirstOrDefaultAsync();

        return string.IsNullOrWhiteSpace(configuredUrl)
            ? "https://hooshyaran.ir"
            : configuredUrl.Trim().TrimEnd('/');
    }

    private static string BuildXml(string baseUrl, IEnumerable<SitemapUrl> urls)
    {
        var builder = new StringBuilder();
        using var writer = XmlWriter.Create(builder, new XmlWriterSettings
        {
            Async = false,
            Encoding = Encoding.UTF8,
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

        return builder.ToString();
    }

    private sealed record SitemapUrl(string Path, string ChangeFrequency, string Priority, DateTime? LastModified = null);
}

using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages;

public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public SeoMetadata? Seo { get; private set; }

    public List<Product> FeaturedProducts { get; private set; } = [];

    public List<BlogArticle> LatestArticles { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Seo = await dbContext.SeoMetadata.SingleOrDefaultAsync(seo => seo.PageKey == "home");
        FeaturedProducts = await dbContext.Products
            .Include(product => product.ProductCategory)
            .Where(product => product.IsActive && product.IsFeatured)
            .OrderBy(product => product.SortOrder)
            .ToListAsync();

        LatestArticles = await dbContext.BlogArticles
            .Where(article => article.IsPublished)
            .ToListAsync();

        LatestArticles = LatestArticles
            .OrderByDescending(article => article.PublishedAt)
            .Take(3)
            .ToList();
    }
}

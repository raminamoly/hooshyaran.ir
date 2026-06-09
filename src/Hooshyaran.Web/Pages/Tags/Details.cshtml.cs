using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Tags;

public class DetailsModel(HooshyaranDbContext dbContext) : PageModel
{
    public Tag Tag { get; private set; } = new();

    public List<BlogArticle> Articles { get; private set; } = [];

    public List<Product> Products { get; private set; } = [];

    public List<StaticPage> Pages { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var tag = await dbContext.Tags.SingleOrDefaultAsync(item => item.Slug == slug);
        if (tag is null)
        {
            return NotFound();
        }

        Tag = tag;
        Articles = await dbContext.BlogArticles
            .Where(article => article.IsPublished && article.BlogArticleTags.Any(item => item.TagId == tag.Id))
            .ToListAsync();
        Articles = Articles
            .OrderByDescending(article => article.PublishedAt)
            .ToList();
        Products = await dbContext.Products
            .Include(product => product.ProductCategory)
            .Where(product => product.IsActive && product.ProductTags.Any(item => item.TagId == tag.Id))
            .OrderBy(product => product.SortOrder)
            .ToListAsync();
        Pages = await dbContext.StaticPages
            .Where(page => page.IsPublished && page.StaticPageTags.Any(item => item.TagId == tag.Id))
            .OrderBy(page => page.Title)
            .ToListAsync();

        return Page();
    }
}

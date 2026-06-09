using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Blog;

public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public SeoMetadata? Seo { get; private set; }

    public List<BlogArticle> Articles { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Seo = await dbContext.SeoMetadata.SingleOrDefaultAsync(seo => seo.PageKey == "blog");
        Articles = await dbContext.BlogArticles
            .Where(article => article.IsPublished)
            .ToListAsync();

        Articles = Articles
            .OrderByDescending(article => article.PublishedAt)
            .ToList();
    }
}

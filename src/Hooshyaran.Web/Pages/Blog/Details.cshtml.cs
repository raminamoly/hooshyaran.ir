using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Blog;

public class DetailsModel(HooshyaranDbContext dbContext, ISiteVisitLogger visitLogger) : PageModel
{
    public BlogArticle Article { get; private set; } = new();

    public List<BlogArticle> RelatedArticles { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var article = await dbContext.BlogArticles
            .Include(item => item.BlogArticleTags)
                .ThenInclude(item => item.Tag)
            .SingleOrDefaultAsync(item => item.Slug == slug && item.IsPublished);

        if (article is null)
        {
            return NotFound();
        }

        Article = article;
        await visitLogger.LogAsync(HttpContext, SiteVisitEventTypes.ArticleClick, article.Title, article.Id);

        RelatedArticles = await dbContext.BlogArticles
            .Where(item => item.IsPublished && item.Id != article.Id)
            .ToListAsync();

        RelatedArticles = RelatedArticles
            .OrderByDescending(item => item.PublishedAt)
            .Take(2)
            .ToList();

        return Page();
    }

    public static IReadOnlyList<string> Paragraphs(string value) =>
        value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

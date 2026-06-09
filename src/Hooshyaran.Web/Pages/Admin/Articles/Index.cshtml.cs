using System.Security.Claims;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Articles;

public class IndexModel(HooshyaranDbContext dbContext, IPublicUrlBuilder publicUrlBuilder) : PageModel
{
    public List<ArticleRow> Articles { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; private set; } = 10;

    public int TotalPages { get; private set; }

    public int TotalItems { get; private set; }

    public async Task OnGetAsync()
    {
        var query = dbContext.BlogArticles.AsNoTracking();
        if (!User.IsInRole(AdminUserRoles.SuperAdmin))
        {
            query = query.Where(article => article.AdminUserId == CurrentUserId);
        }

        var allArticles = await query.ToListAsync();
        allArticles = allArticles
            .OrderByDescending(article => article.PublishedAt)
            .ToList();

        TotalItems = allArticles.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        var articleItems = allArticles
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Articles = [];
        foreach (var article in articleItems)
        {
            Articles.Add(new ArticleRow(article, await publicUrlBuilder.BuildAsync(Request, $"/blog/{article.Slug}")));
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var article = await dbContext.BlogArticles.FindAsync(id);
        if (article is not null)
        {
            if (!User.IsInRole(AdminUserRoles.SuperAdmin) && article.AdminUserId != CurrentUserId)
            {
                return Forbid();
            }

            dbContext.BlogArticles.Remove(article);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public record ArticleRow(BlogArticle Article, string PublicUrl);
}

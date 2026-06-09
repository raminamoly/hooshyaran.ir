using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages;

public class AboutModel(HooshyaranDbContext dbContext) : PageModel
{
    public StaticPage? PageContent { get; private set; }

    public SeoMetadata? Seo { get; private set; }

    public async Task OnGetAsync()
    {
        PageContent = await dbContext.StaticPages
            .Include(page => page.StaticPageTags)
                .ThenInclude(item => item.Tag)
            .SingleOrDefaultAsync(page => page.Key == "about" && page.IsPublished);
        Seo = await dbContext.SeoMetadata.SingleOrDefaultAsync(seo => seo.PageKey == "about");
    }

    public IReadOnlyList<string> Paragraphs =>
        (PageContent?.Body ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

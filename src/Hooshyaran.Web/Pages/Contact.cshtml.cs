using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages;

public class ContactModel(HooshyaranDbContext dbContext) : PageModel
{
    public StaticPage? PageContent { get; private set; }

    public SeoMetadata? Seo { get; private set; }

    public async Task OnGetAsync()
    {
        PageContent = await dbContext.StaticPages.SingleOrDefaultAsync(page => page.Key == "contact" && page.IsPublished);
        Seo = await dbContext.SeoMetadata.SingleOrDefaultAsync(seo => seo.PageKey == "contact");
    }

    public IReadOnlyList<string> Lines =>
        (PageContent?.Body ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

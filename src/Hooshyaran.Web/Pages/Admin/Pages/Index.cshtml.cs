using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Pages;

public class IndexModel(HooshyaranDbContext dbContext, IPublicUrlBuilder publicUrlBuilder) : PageModel
{
    public List<PageRow> Pages { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var pages = await dbContext.StaticPages.OrderBy(page => page.Key).ToListAsync();
        Pages = [];
        foreach (var page in pages)
        {
            Pages.Add(new PageRow(page, await publicUrlBuilder.BuildAsync(Request, $"/{page.Slug}")));
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var page = await dbContext.StaticPages.FindAsync(id);
        if (page is not null)
        {
            dbContext.StaticPages.Remove(page);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public record PageRow(StaticPage Page, string PublicUrl);
}

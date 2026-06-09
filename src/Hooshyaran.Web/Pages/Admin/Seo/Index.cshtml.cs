using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Seo;

public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public List<SeoMetadata> Metadata { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Metadata = await dbContext.SeoMetadata.OrderBy(seo => seo.PageKey).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var seo = await dbContext.SeoMetadata.FindAsync(id);
        if (seo is not null)
        {
            dbContext.SeoMetadata.Remove(seo);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}

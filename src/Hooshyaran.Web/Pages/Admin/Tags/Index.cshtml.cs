using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Tags;

public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public List<Tag> Tags { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Tags = await dbContext.Tags
            .Include(tag => tag.BlogArticleTags)
            .Include(tag => tag.ProductTags)
            .Include(tag => tag.StaticPageTags)
            .OrderBy(tag => tag.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var tag = await dbContext.Tags.FindAsync(id);
        if (tag is not null)
        {
            dbContext.Tags.Remove(tag);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}

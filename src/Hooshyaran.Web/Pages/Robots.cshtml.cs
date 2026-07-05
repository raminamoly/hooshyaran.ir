using Hooshyaran.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages;

public class RobotsModel(HooshyaranDbContext dbContext) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var baseUrl = await GetBaseUrlAsync();
        var body = string.Join('\n',
            "User-agent: *",
            "Allow: /",
            "Disallow: /admin",
            $"Sitemap: {baseUrl}/sitemap.xml",
            string.Empty);

        return Content(body, "text/plain; charset=utf-8");
    }

    private async Task<string> GetBaseUrlAsync()
    {
        var configuredUrl = await dbContext.SiteSettings
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => item.WebsiteUrl)
            .FirstOrDefaultAsync();

        return string.IsNullOrWhiteSpace(configuredUrl)
            ? "https://hooshyaran.ir"
            : configuredUrl.Trim().TrimEnd('/');
    }
}

using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hooshyaran.Web.Pages;

public class SitemapModel(ISitemapService sitemapService) : PageModel
{
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var snapshot = await sitemapService.GetLatestSnapshotAsync(cancellationToken);
        if (snapshot is not null)
        {
            return Content(snapshot.Xml, "application/xml; charset=utf-8");
        }

        var result = await sitemapService.BuildAsync(cancellationToken);
        return Content(result.Xml, "application/xml; charset=utf-8");
    }
}

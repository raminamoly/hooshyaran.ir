using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin;

public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public int ArticleCount { get; private set; }

    public int ProductCount { get; private set; }

    public int PageCount { get; private set; }

    public int SeoCount { get; private set; }

    public int CategoryCount { get; private set; }

    public int DemoRequestCount { get; private set; }

    public int UserCount { get; private set; }

    public bool IsSuperAdmin => User.IsInRole(AdminUserRoles.SuperAdmin);

    public async Task OnGetAsync()
    {
        ArticleCount = IsSuperAdmin
            ? await dbContext.BlogArticles.CountAsync()
            : await dbContext.BlogArticles.CountAsync(article => article.AdminUserId == CurrentUserId);
        if (IsSuperAdmin)
        {
            ProductCount = await dbContext.Products.CountAsync();
            PageCount = await dbContext.StaticPages.CountAsync();
            SeoCount = await dbContext.SeoMetadata.CountAsync();
            CategoryCount = await dbContext.ProductCategories.CountAsync();
            DemoRequestCount = await dbContext.DemoRequests.CountAsync();
            UserCount = await dbContext.AdminUsers.CountAsync();
        }
    }

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
}

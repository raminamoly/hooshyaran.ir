using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Products;

public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public SeoMetadata? Seo { get; private set; }

    public List<ProductCategory> Categories { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Seo = await dbContext.SeoMetadata.SingleOrDefaultAsync(seo => seo.PageKey == "products");
        Categories = await dbContext.ProductCategories
            .Include(category => category.Products.Where(product => product.IsActive))
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ToListAsync();
    }
}

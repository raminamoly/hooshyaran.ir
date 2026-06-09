using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Products;

public class IndexModel(HooshyaranDbContext dbContext, IPublicUrlBuilder publicUrlBuilder) : PageModel
{
    public List<ProductRow> Products { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var products = await dbContext.Products
            .Include(product => product.ProductCategory)
            .OrderBy(product => product.SortOrder)
            .ToListAsync();

        Products = [];
        foreach (var product in products)
        {
            Products.Add(new ProductRow(product, await publicUrlBuilder.BuildAsync(Request, $"/products/{product.Slug}")));
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var product = await dbContext.Products.FindAsync(id);
        if (product is not null)
        {
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public record ProductRow(Product Product, string PublicUrl);
}

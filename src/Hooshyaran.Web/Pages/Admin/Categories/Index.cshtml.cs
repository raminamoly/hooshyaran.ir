using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Categories;

public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    public List<ProductCategory> Categories { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Categories = await dbContext.ProductCategories.OrderBy(category => category.SortOrder).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var hasProducts = await dbContext.Products.AnyAsync(product => product.ProductCategoryId == id);
        var category = await dbContext.ProductCategories.FindAsync(id);
        if (category is not null && !hasProducts)
        {
            dbContext.ProductCategories.Remove(category);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}

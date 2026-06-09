using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Products;

public class DetailsModel(HooshyaranDbContext dbContext) : PageModel
{
    public Product Product { get; private set; } = new();

    public List<FaqItem> Faqs { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var product = await dbContext.Products
            .Include(item => item.ProductCategory)
            .Include(item => item.ProductTags)
                .ThenInclude(item => item.Tag)
            .SingleOrDefaultAsync(item => item.Slug == slug && item.IsActive);

        if (product is null)
        {
            return NotFound();
        }

        Product = product;
        Faqs = await dbContext.FaqItems
            .Where(faq => faq.IsActive && faq.PageKey == $"product-{slug}")
            .OrderBy(faq => faq.SortOrder)
            .ToListAsync();

        return Page();
    }

    public static IReadOnlyList<string> Lines(string value) =>
        value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

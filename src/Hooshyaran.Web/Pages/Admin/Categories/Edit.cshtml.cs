using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Categories;

public class EditModel(HooshyaranDbContext dbContext) : PageModel
{
    [BindProperty]
    public CategoryInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null) return Page();
        var category = await dbContext.ProductCategories.FindAsync(id.Value);
        if (category is null) return NotFound();
        Input = new CategoryInput
        {
            Id = category.Id,
            Title = category.Title,
            Slug = category.Slug,
            Description = category.Description,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (await dbContext.ProductCategories.AnyAsync(category => category.Slug == Input.Slug && category.Id != Input.Id))
        {
            ModelState.AddModelError("Input.Slug", "این Slug قبلا استفاده شده است.");
            return Page();
        }

        var categoryEntity = Input.Id == 0 ? new ProductCategory() : await dbContext.ProductCategories.FindAsync(Input.Id);
        if (categoryEntity is null) return NotFound();
        categoryEntity.Title = Input.Title;
        categoryEntity.Slug = Input.Slug;
        categoryEntity.Description = Input.Description;
        categoryEntity.SortOrder = Input.SortOrder;
        categoryEntity.IsActive = Input.IsActive;
        if (Input.Id == 0) dbContext.ProductCategories.Add(categoryEntity);
        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Admin/Categories/Index");
    }

    public class CategoryInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug الزامی است.")]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug فقط می‌تواند شامل حروف انگلیسی کوچک، عدد و خط تیره باشد.")]
        [Display(Name = "Slug")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "ترتیب نمایش")]
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

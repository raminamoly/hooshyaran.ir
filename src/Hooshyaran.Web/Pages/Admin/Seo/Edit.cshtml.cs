using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Seo;

public class EditModel(HooshyaranDbContext dbContext) : PageModel
{
    [BindProperty]
    public SeoInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null) return Page();
        var seo = await dbContext.SeoMetadata.FindAsync(id.Value);
        if (seo is null) return NotFound();
        Input = new SeoInput
        {
            Id = seo.Id,
            PageKey = seo.PageKey,
            Title = seo.Title,
            Description = seo.Description,
            Keywords = seo.Keywords,
            CanonicalPath = seo.CanonicalPath
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (await dbContext.SeoMetadata.AnyAsync(seo => seo.PageKey == Input.PageKey && seo.Id != Input.Id))
        {
            ModelState.AddModelError("Input.PageKey", "این PageKey قبلا استفاده شده است.");
            return Page();
        }

        var entity = Input.Id == 0 ? new SeoMetadata() : await dbContext.SeoMetadata.FindAsync(Input.Id);
        if (entity is null) return NotFound();
        entity.PageKey = Input.PageKey;
        entity.Title = Input.Title;
        entity.Description = Input.Description;
        entity.Keywords = Input.Keywords;
        entity.CanonicalPath = Input.CanonicalPath;
        if (Input.Id == 0) dbContext.SeoMetadata.Add(entity);
        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Admin/Seo/Index");
    }

    public class SeoInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "PageKey الزامی است.")]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "PageKey فقط می‌تواند شامل حروف انگلیسی کوچک، عدد و خط تیره باشد.")]
        [Display(Name = "PageKey")]
        public string PageKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "توضیحات")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "کلمات کلیدی")]
        public string Keywords { get; set; } = string.Empty;

        [Display(Name = "Canonical Path")]
        public string CanonicalPath { get; set; } = string.Empty;
    }
}

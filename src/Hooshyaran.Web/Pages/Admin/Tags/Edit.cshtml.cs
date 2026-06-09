using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Tags;

public class EditModel(HooshyaranDbContext dbContext) : PageModel
{
    [BindProperty]
    public TagInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return Page();
        }

        var tag = await dbContext.Tags.FindAsync(id.Value);
        if (tag is null)
        {
            return NotFound();
        }

        Input = new TagInput
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug,
            Description = tag.Description
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await dbContext.Tags.AnyAsync(tag => tag.Name == Input.Name && tag.Id != Input.Id))
        {
            ModelState.AddModelError("Input.Name", "این نام تگ قبلا استفاده شده است.");
            return Page();
        }

        if (await dbContext.Tags.AnyAsync(tag => tag.Slug == Input.Slug && tag.Id != Input.Id))
        {
            ModelState.AddModelError("Input.Slug", "این Slug قبلا استفاده شده است.");
            return Page();
        }

        var now = DateTimeOffset.Now;
        var tagEntity = Input.Id == 0 ? new Tag { CreatedAt = now } : await dbContext.Tags.FindAsync(Input.Id);
        if (tagEntity is null)
        {
            return NotFound();
        }

        tagEntity.Name = Input.Name.Trim();
        tagEntity.Slug = Input.Slug.Trim();
        tagEntity.Description = Input.Description.Trim();
        tagEntity.UpdatedAt = now;

        if (Input.Id == 0)
        {
            dbContext.Tags.Add(tagEntity);
        }

        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Admin/Tags/Index");
    }

    public class TagInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام تگ الزامی است.")]
        [StringLength(120, ErrorMessage = "نام تگ نمی‌تواند بیشتر از ۱۲۰ کاراکتر باشد.")]
        [Display(Name = "نام تگ")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug الزامی است.")]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug فقط می‌تواند شامل حروف انگلیسی کوچک، عدد و خط تیره باشد.")]
        [StringLength(120, ErrorMessage = "Slug نمی‌تواند بیشتر از ۱۲۰ کاراکتر باشد.")]
        [Display(Name = "Slug")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(600, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۶۰۰ کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; } = string.Empty;
    }
}

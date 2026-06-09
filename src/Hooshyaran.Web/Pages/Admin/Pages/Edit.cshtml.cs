using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Pages;

public class EditModel(
    HooshyaranDbContext dbContext,
    IPublicUrlBuilder publicUrlBuilder,
    ICmsHtmlService cmsHtmlService) : PageModel
{
    [BindProperty]
    public PageInput Input { get; set; } = new();

    public List<SelectListItem> TagOptions { get; private set; } = [];

    public string? PublicUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadTagsAsync();

        if (id is null) return Page();
        var page = await dbContext.StaticPages
            .Include(item => item.StaticPageTags)
            .SingleOrDefaultAsync(item => item.Id == id.Value);
        if (page is null) return NotFound();
        Input = new PageInput
        {
            Id = page.Id,
            Key = page.Key,
            Title = page.Title,
            Slug = page.Slug,
            Summary = page.Summary,
            Body = page.Body,
            IsPublished = page.IsPublished,
            SeoTitle = page.SeoTitle,
            SeoDescription = page.SeoDescription,
            SeoKeywords = page.SeoKeywords,
            SelectedTagIds = page.StaticPageTags.Select(item => item.TagId).ToList()
        };
        PublicUrl = await publicUrlBuilder.BuildAsync(Request, $"/{page.Slug}");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadTagsAsync();

        if (!ModelState.IsValid) return Page();
        if (!await AreTagsValidAsync(Input.SelectedTagIds))
        {
            ModelState.AddModelError("Input.SelectedTagIds", "یک یا چند تگ انتخاب‌شده معتبر نیست.");
            return Page();
        }

        if (await dbContext.StaticPages.AnyAsync(page => page.Key == Input.Key && page.Id != Input.Id))
        {
            ModelState.AddModelError("Input.Key", "این کلید قبلا استفاده شده است.");
            return Page();
        }
        if (await dbContext.StaticPages.AnyAsync(page => page.Slug == Input.Slug && page.Id != Input.Id))
        {
            ModelState.AddModelError("Input.Slug", "این Slug قبلا استفاده شده است.");
            return Page();
        }

        var pageEntity = Input.Id == 0
            ? new StaticPage()
            : await dbContext.StaticPages
                .Include(item => item.StaticPageTags)
                .SingleOrDefaultAsync(item => item.Id == Input.Id);
        if (pageEntity is null) return NotFound();
        pageEntity.Key = Input.Key;
        pageEntity.Title = Input.Title;
        pageEntity.Slug = Input.Slug;
        pageEntity.Summary = Input.Summary;
        pageEntity.Body = cmsHtmlService.ToSafeHtml(Input.Body);
        pageEntity.IsPublished = Input.IsPublished;
        pageEntity.SeoTitle = Input.SeoTitle;
        pageEntity.SeoDescription = Input.SeoDescription;
        pageEntity.SeoKeywords = Input.SeoKeywords;
        if (Input.Id == 0) dbContext.StaticPages.Add(pageEntity);
        await dbContext.SaveChangesAsync();
        SyncStaticPageTags(pageEntity);
        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Admin/Pages/Index");
    }

    private async Task LoadTagsAsync()
    {
        TagOptions = await dbContext.Tags
            .OrderBy(tag => tag.Name)
            .Select(tag => new SelectListItem(tag.Name, tag.Id.ToString()))
            .ToListAsync();
    }

    private async Task<bool> AreTagsValidAsync(List<int> selectedTagIds)
    {
        var tagIds = selectedTagIds.Distinct().ToList();
        var existingCount = await dbContext.Tags.CountAsync(tag => tagIds.Contains(tag.Id));
        return existingCount == tagIds.Count;
    }

    private void SyncStaticPageTags(StaticPage page)
    {
        var selectedTagIds = Input.SelectedTagIds.Distinct().ToHashSet();
        var currentTagIds = page.StaticPageTags.Select(item => item.TagId).ToHashSet();

        foreach (var relation in page.StaticPageTags.Where(item => !selectedTagIds.Contains(item.TagId)).ToList())
        {
            dbContext.StaticPageTags.Remove(relation);
        }

        foreach (var tagId in selectedTagIds.Except(currentTagIds))
        {
            page.StaticPageTags.Add(new StaticPageTag { StaticPageId = page.Id, TagId = tagId });
        }
    }

    public class PageInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "کلید الزامی است.")]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "کلید فقط می‌تواند شامل حروف انگلیسی کوچک، عدد و خط تیره باشد.")]
        [Display(Name = "کلید")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug الزامی است.")]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug فقط می‌تواند شامل حروف انگلیسی کوچک، عدد و خط تیره باشد.")]
        [Display(Name = "Slug")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "خلاصه")]
        public string Summary { get; set; } = string.Empty;

        [Display(Name = "متن صفحه")]
        public string Body { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = true;

        [Display(Name = "عنوان SEO")]
        public string SeoTitle { get; set; } = string.Empty;

        [Display(Name = "توضیحات SEO")]
        public string SeoDescription { get; set; } = string.Empty;

        [Display(Name = "کلمات کلیدی SEO")]
        public string SeoKeywords { get; set; } = string.Empty;

        [Display(Name = "تگ‌ها")]
        public List<int> SelectedTagIds { get; set; } = [];
    }
}

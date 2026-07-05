using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Products;

public class EditModel(
    HooshyaranDbContext dbContext,
    IPublicUrlBuilder publicUrlBuilder,
    ICmsHtmlService cmsHtmlService) : PageModel
{
    [BindProperty]
    public ProductInput Input { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; private set; } = [];

    public List<SelectListItem> TagOptions { get; private set; } = [];

    public string? PublicUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadCategoriesAsync();
        await LoadTagsAsync();

        if (id is null)
        {
            Input.SortOrder = 100;
            Input.LogoPath = "/uploads/media/imported/brand/hooshyaran-logo-small.png";
            Input.HeroImagePath = "/uploads/media/imported/content/home-enterprise-ai-hero.jpg";
            Input.CtaText = "درخواست جلسه معرفی";
            return Page();
        }

        var product = await dbContext.Products
            .Include(item => item.ProductTags)
            .SingleOrDefaultAsync(item => item.Id == id.Value);
        if (product is null)
        {
            return NotFound();
        }

        Input = new ProductInput
        {
            Id = product.Id,
            ProductCategoryId = product.ProductCategoryId,
            Name = product.Name,
            PersianTitle = product.PersianTitle,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            LongDescription = product.LongDescription,
            ProblemsSolved = product.ProblemsSolved,
            Benefits = product.Benefits,
            PublicFeatures = product.PublicFeatures,
            TargetAudience = product.TargetAudience,
            UseCases = product.UseCases,
            HeroImagePath = product.HeroImagePath,
            LogoPath = product.LogoPath,
            CtaText = product.CtaText,
            IsFeatured = product.IsFeatured,
            SortOrder = product.SortOrder,
            IsActive = product.IsActive,
            SeoTitle = product.SeoTitle,
            SeoDescription = product.SeoDescription,
            SeoKeywords = product.SeoKeywords,
            SelectedTagIds = product.ProductTags.Select(item => item.TagId).ToList()
        };

        PublicUrl = await publicUrlBuilder.BuildAsync(Request, $"/products/{product.Slug}");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();
        await LoadTagsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!await AreTagsValidAsync(Input.SelectedTagIds))
        {
            ModelState.AddModelError("Input.SelectedTagIds", "یک یا چند تگ انتخاب‌شده معتبر نیست.");
            return Page();
        }

        if (!await dbContext.ProductCategories.AnyAsync(category => category.Id == Input.ProductCategoryId))
        {
            ModelState.AddModelError("Input.ProductCategoryId", "دسته محصول معتبر نیست.");
            return Page();
        }

        if (await dbContext.Products.AnyAsync(product => product.Slug == Input.Slug && product.Id != Input.Id))
        {
            ModelState.AddModelError("Input.Slug", "این Slug قبلا استفاده شده است.");
            return Page();
        }

        var productEntity = Input.Id == 0
            ? new Product()
            : await dbContext.Products
                .Include(item => item.ProductTags)
                .SingleOrDefaultAsync(item => item.Id == Input.Id);

        if (productEntity is null)
        {
            return NotFound();
        }

        productEntity.ProductCategoryId = Input.ProductCategoryId;
        productEntity.Name = Input.Name;
        productEntity.PersianTitle = Input.PersianTitle;
        productEntity.Slug = Input.Slug;
        productEntity.ShortDescription = Input.ShortDescription;
        productEntity.LongDescription = cmsHtmlService.ToSafeHtml(Input.LongDescription);
        productEntity.ProblemsSolved = Input.ProblemsSolved;
        productEntity.Benefits = Input.Benefits;
        productEntity.PublicFeatures = Input.PublicFeatures;
        productEntity.TargetAudience = Input.TargetAudience;
        productEntity.UseCases = Input.UseCases;
        productEntity.HeroImagePath = Input.HeroImagePath;
        productEntity.LogoPath = Input.LogoPath;
        productEntity.CtaText = Input.CtaText;
        productEntity.IsFeatured = Input.IsFeatured;
        productEntity.SortOrder = Input.SortOrder;
        productEntity.IsActive = Input.IsActive;
        productEntity.SeoTitle = Input.SeoTitle;
        productEntity.SeoDescription = Input.SeoDescription;
        productEntity.SeoKeywords = Input.SeoKeywords;

        if (Input.Id == 0)
        {
            dbContext.Products.Add(productEntity);
        }

        await dbContext.SaveChangesAsync();
        SyncProductTags(productEntity);
        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Admin/Products/Index");
    }

    private async Task LoadCategoriesAsync()
    {
        CategoryOptions = await dbContext.ProductCategories
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .Select(category => new SelectListItem(category.Title, category.Id.ToString()))
            .ToListAsync();
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

    private void SyncProductTags(Product product)
    {
        var selectedTagIds = Input.SelectedTagIds.Distinct().ToHashSet();
        var currentTagIds = product.ProductTags.Select(item => item.TagId).ToHashSet();

        foreach (var relation in product.ProductTags.Where(item => !selectedTagIds.Contains(item.TagId)).ToList())
        {
            dbContext.ProductTags.Remove(relation);
        }

        foreach (var tagId in selectedTagIds.Except(currentTagIds))
        {
            product.ProductTags.Add(new ProductTag { ProductId = product.Id, TagId = tagId });
        }
    }

    public class ProductInput
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "انتخاب دسته الزامی است.")]
        [Display(Name = "دسته محصول")]
        public int ProductCategoryId { get; set; }

        [Required(ErrorMessage = "نام محصول الزامی است.")]
        [Display(Name = "نام لاتین/کوتاه")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان فارسی الزامی است.")]
        [Display(Name = "عنوان فارسی")]
        public string PersianTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug الزامی است.")]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug فقط می‌تواند شامل حروف انگلیسی کوچک، عدد و خط تیره باشد.")]
        [Display(Name = "Slug")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "توضیح کوتاه")]
        public string ShortDescription { get; set; } = string.Empty;

        [Display(Name = "معرفی کامل")]
        public string LongDescription { get; set; } = string.Empty;

        [Display(Name = "مسئله‌هایی که حل می‌کند")]
        public string ProblemsSolved { get; set; } = string.Empty;

        [Display(Name = "مزیت‌ها")]
        public string Benefits { get; set; } = string.Empty;

        [Display(Name = "قابلیت‌های عمومی")]
        public string PublicFeatures { get; set; } = string.Empty;

        [Display(Name = "مخاطبان هدف")]
        public string TargetAudience { get; set; } = string.Empty;

        [Display(Name = "کاربردها")]
        public string UseCases { get; set; } = string.Empty;

        [Display(Name = "مسیر تصویر Hero")]
        public string HeroImagePath { get; set; } = string.Empty;

        [Display(Name = "مسیر لوگو")]
        public string LogoPath { get; set; } = string.Empty;

        [Display(Name = "متن CTA")]
        public string CtaText { get; set; } = string.Empty;

        public bool IsFeatured { get; set; }

        [Display(Name = "ترتیب نمایش")]
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

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

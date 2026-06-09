using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Articles;

public class EditModel(
    HooshyaranDbContext dbContext,
    IPublicUrlBuilder publicUrlBuilder,
    ICmsHtmlService cmsHtmlService) : PageModel
{
    [BindProperty]
    public ArticleInput Input { get; set; } = new();

    public List<SelectListItem> TagOptions { get; private set; } = [];

    public List<SelectListItem> AuthorOptions { get; private set; } = [];

    public string? PublicUrl { get; private set; }

    public bool IsSuperAdmin => User.IsInRole(AdminUserRoles.SuperAdmin);

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadTagsAsync();
        await LoadAuthorsAsync();

        if (id is null)
        {
            Input.PublishedAt = DateTimeOffset.Now;
            Input.AdminUserId = CurrentUserId;
            Input.AuthorName = User.FindFirstValue("DisplayName") ?? "هوش‌یاران";
            return Page();
        }

        var article = await dbContext.BlogArticles
            .Include(item => item.BlogArticleTags)
            .SingleOrDefaultAsync(item => item.Id == id.Value);
        if (article is null)
        {
            return NotFound();
        }

        if (!CanManageArticle(article))
        {
            return Forbid();
        }

        Input = new ArticleInput
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            Body = article.Body,
            ImagePath = article.ImagePath,
            AuthorName = article.AuthorName,
            AdminUserId = article.AdminUserId,
            PublishedAt = article.PublishedAt,
            IsPublished = article.IsPublished,
            SeoTitle = article.SeoTitle,
            SeoDescription = article.SeoDescription,
            SeoKeywords = article.SeoKeywords,
            SelectedTagIds = article.BlogArticleTags.Select(item => item.TagId).ToList()
        };
        PublicUrl = await publicUrlBuilder.BuildAsync(Request, $"/blog/{article.Slug}");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadTagsAsync();
        await LoadAuthorsAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!await AreTagsValidAsync(Input.SelectedTagIds))
        {
            ModelState.AddModelError("Input.SelectedTagIds", "یک یا چند تگ انتخاب‌شده معتبر نیست.");
            return Page();
        }

        var slugExists = await dbContext.BlogArticles.AnyAsync(article =>
            article.Slug == Input.Slug && article.Id != Input.Id);
        if (slugExists)
        {
            ModelState.AddModelError("Input.Slug", "این Slug قبلا استفاده شده است.");
            return Page();
        }

        var article = Input.Id == 0
            ? new BlogArticle()
            : await dbContext.BlogArticles
                .Include(item => item.BlogArticleTags)
                .SingleOrDefaultAsync(item => item.Id == Input.Id);

        if (article is null)
        {
            return NotFound();
        }

        if (!CanManageArticle(article))
        {
            return Forbid();
        }

        article.Title = Input.Title;
        article.Slug = Input.Slug;
        article.Summary = Input.Summary;
        article.Body = cmsHtmlService.ToSafeHtml(Input.Body);
        article.ImagePath = Input.ImagePath;
        if (IsSuperAdmin)
        {
            article.AdminUserId = Input.AdminUserId == 0 ? null : Input.AdminUserId;
            article.AuthorName = Input.AuthorName;
        }
        else
        {
            article.AdminUserId = CurrentUserId;
            article.AuthorName = User.FindFirstValue("DisplayName") ?? Input.AuthorName;
        }
        article.PublishedAt = Input.PublishedAt;
        article.IsPublished = Input.IsPublished;
        article.SeoTitle = Input.SeoTitle;
        article.SeoDescription = Input.SeoDescription;
        article.SeoKeywords = Input.SeoKeywords;

        if (Input.Id == 0)
        {
            dbContext.BlogArticles.Add(article);
        }

        await dbContext.SaveChangesAsync();
        SyncArticleTags(article);
        await dbContext.SaveChangesAsync();
        return RedirectToPage("/Admin/Articles/Index");
    }

    private async Task LoadTagsAsync()
    {
        TagOptions = await dbContext.Tags
            .OrderBy(tag => tag.Name)
            .Select(tag => new SelectListItem(tag.Name, tag.Id.ToString()))
            .ToListAsync();
    }

    private async Task LoadAuthorsAsync()
    {
        AuthorOptions = await dbContext.AdminUsers
            .Where(user => user.IsActive)
            .OrderByDescending(user => user.Role == AdminUserRoles.SuperAdmin)
            .ThenBy(user => user.DisplayName)
            .Select(user => new SelectListItem(user.DisplayName, user.Id.ToString()))
            .ToListAsync();
    }

    private async Task<bool> AreTagsValidAsync(List<int> selectedTagIds)
    {
        var tagIds = selectedTagIds.Distinct().ToList();
        var existingCount = await dbContext.Tags.CountAsync(tag => tagIds.Contains(tag.Id));
        return existingCount == tagIds.Count;
    }

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private bool CanManageArticle(BlogArticle article) =>
        IsSuperAdmin || article.AdminUserId == CurrentUserId || (article.Id == 0 && CurrentUserId > 0);

    private void SyncArticleTags(BlogArticle article)
    {
        var selectedTagIds = Input.SelectedTagIds.Distinct().ToHashSet();
        var currentTagIds = article.BlogArticleTags.Select(item => item.TagId).ToHashSet();

        foreach (var relation in article.BlogArticleTags.Where(item => !selectedTagIds.Contains(item.TagId)).ToList())
        {
            dbContext.BlogArticleTags.Remove(relation);
        }

        foreach (var tagId in selectedTagIds.Except(currentTagIds))
        {
            article.BlogArticleTags.Add(new BlogArticleTag { BlogArticleId = article.Id, TagId = tagId });
        }
    }

    public class ArticleInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug الزامی است.")]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug فقط می‌تواند شامل حروف انگلیسی کوچک، عدد و خط تیره باشد.")]
        [Display(Name = "Slug")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "خلاصه")]
        public string Summary { get; set; } = string.Empty;

        [Display(Name = "متن مقاله")]
        public string Body { get; set; } = string.Empty;

        [Display(Name = "مسیر تصویر")]
        public string ImagePath { get; set; } = string.Empty;

        [Display(Name = "نویسنده")]
        public string AuthorName { get; set; } = string.Empty;

        [Display(Name = "کاربر نویسنده")]
        public int? AdminUserId { get; set; }

        [Display(Name = "زمان انتشار")]
        public DateTimeOffset PublishedAt { get; set; }

        public bool IsPublished { get; set; }

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

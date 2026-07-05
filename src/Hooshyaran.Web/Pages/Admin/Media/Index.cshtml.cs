using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Hooshyaran.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hooshyaran.Web.Pages.Admin.Media;

[Authorize]
public class IndexModel(IMediaFileService mediaFileService) : PageModel
{
    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string FileType { get; set; } = "all";

    [BindProperty(SupportsGet = true)]
    public string UsageFilter { get; set; } = "all";

    [BindProperty]
    public MediaMetadataInput MetadataInput { get; set; } = new();

    public MediaLibraryResult Library { get; private set; } = new([], string.Empty, "all", "all", 0, 0, 0, 0);

    public string? StatusMessage { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string AllowedExtensionsText => string.Join(", ", mediaFileService.AllowedExtensions.Select(item => item.TrimStart('.')));

    public string MaxFileSizeText => $"{mediaFileService.MaxFileSize / 1024 / 1024}MB";

    public async Task OnGetAsync()
    {
        await LoadLibraryAsync();
    }

    public async Task<IActionResult> OnGetLibraryAsync(string? searchTerm, string? fileType, string? usageFilter)
    {
        var library = await mediaFileService.ListAsync(searchTerm, fileType, usageFilter);

        return new JsonResult(library.Files);
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (UploadFile is null)
        {
            ErrorMessage = "فایل انتخاب نشده است.";
            await LoadLibraryAsync();

            return Page();
        }

        try
        {
            await mediaFileService.UploadAsync(UploadFile, HttpContext.RequestAborted);
            StatusMessage = "فایل با موفقیت آپلود شد.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        await LoadLibraryAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostUploadJsonAsync(IFormFile? file)
    {
        if (file is null)
        {
            return BadRequest(new { error = "فایل انتخاب نشده است." });
        }

        try
        {
            var item = await mediaFileService.UploadAsync(file, HttpContext.RequestAborted);

            return new JsonResult(item);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string url)
    {
        if (!mediaFileService.IsManagedMediaUrl(url))
        {
            ErrorMessage = "مسیر فایل معتبر نیست.";
            await LoadLibraryAsync();

            return Page();
        }

        var currentLibrary = await mediaFileService.ListAsync();
        var file = currentLibrary.Files.SingleOrDefault(item => item.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
        if (file?.IsUsed == true)
        {
            ErrorMessage = "این فایل در مقاله یا محصول استفاده شده و حذف نشد.";
            await LoadLibraryAsync();

            return Page();
        }

        StatusMessage = await mediaFileService.DeleteAsync(url)
            ? "فایل حذف شد."
            : "فایل پیدا نشد.";
        await LoadLibraryAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostSaveMetadataAsync()
    {
        try
        {
            await mediaFileService.SaveMetadataAsync(MetadataInput);
            StatusMessage = "اطلاعات سئوی فایل ذخیره شد.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        await LoadLibraryAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteUnusedAsync()
    {
        if (!User.IsInRole(AdminUserRoles.SuperAdmin))
        {
            ErrorMessage = "فقط مدیر ارشد می‌تواند پاکسازی گروهی انجام دهد.";
            await LoadLibraryAsync();

            return Page();
        }

        try
        {
            var deletedCount = await mediaFileService.DeleteUnusedAsync(HttpContext.RequestAborted);
            StatusMessage = deletedCount == 0
                ? "فایل استفاده‌نشده‌ای برای حذف پیدا نشد."
                : $"{deletedCount} فایل استفاده‌نشده حذف شد.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        await LoadLibraryAsync();

        return Page();
    }

    private async Task LoadLibraryAsync()
    {
        Library = await mediaFileService.ListAsync(SearchTerm, FileType, UsageFilter);
        SearchTerm = Library.SearchTerm;
        FileType = Library.FileType;
        UsageFilter = Library.UsageFilter;
    }
}

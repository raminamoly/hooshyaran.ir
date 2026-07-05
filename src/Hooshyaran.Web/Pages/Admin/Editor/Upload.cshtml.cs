using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hooshyaran.Web.Pages.Admin.Editor;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class UploadModel(IMediaFileService mediaFileService) : PageModel
{
    public IActionResult OnGet() => NotFound();

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        if (file is null)
        {
            return BadRequest(new { error = "فایل انتخاب نشده است." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "فقط فایل تصویری مجاز است." });
        }

        try
        {
            var media = await mediaFileService.UploadAsync(file, HttpContext.RequestAborted);

            return new JsonResult(new { url = media.Url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

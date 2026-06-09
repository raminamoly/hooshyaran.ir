using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hooshyaran.Web.Pages.Admin.Editor;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class UploadModel(IWebHostEnvironment environment) : PageModel
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxFileSize = 3 * 1024 * 1024;

    public IActionResult OnGet() => NotFound();

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "فایل انتخاب نشده است." });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { error = "حداکثر حجم فایل ۳ مگابایت است." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { error = "فقط jpg، png و webp مجاز است." });
        }

        var uploadRoot = Path.Combine(environment.WebRootPath, "uploads", "editor");
        Directory.CreateDirectory(uploadRoot);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadRoot, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        return new JsonResult(new { url = $"/uploads/editor/{fileName}" });
    }
}

using System.ComponentModel.DataAnnotations;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Settings;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class IndexModel(HooshyaranDbContext dbContext) : PageModel
{
    [BindProperty]
    public SettingsInput Input { get; set; } = new();

    public bool Saved { get; private set; }

    public async Task OnGetAsync()
    {
        var settings = await GetOrCreateSettingsAsync();
        Input = ToInput(settings);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!IsValidWebsiteUrl(Input.WebsiteUrl))
        {
            ModelState.AddModelError("Input.WebsiteUrl", "آدرس سایت معتبر نیست. آدرس‌های localhost هم قابل قبول هستند.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var settings = await GetOrCreateSettingsAsync();
        settings.WebsiteUrl = NormalizeUrl(Input.WebsiteUrl);
        settings.SmtpHost = Input.SmtpHost.Trim();
        settings.SmtpPort = Input.SmtpPort;
        settings.SmtpUserName = Input.SmtpUserName.Trim();
        if (!string.IsNullOrWhiteSpace(Input.SmtpPassword))
        {
            settings.SmtpPassword = Input.SmtpPassword;
        }
        settings.FromEmail = Input.FromEmail.Trim();
        settings.FromName = Input.FromName.Trim();
        settings.EnableSsl = Input.EnableSsl;
        settings.AdminNotificationEmail = Input.AdminNotificationEmail.Trim();
        settings.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        Input = ToInput(settings);
        Saved = true;
        return Page();
    }

    private async Task<SiteSettings> GetOrCreateSettingsAsync()
    {
        var settings = await dbContext.SiteSettings.OrderBy(item => item.Id).FirstOrDefaultAsync();
        if (settings is not null)
        {
            return settings;
        }

        settings = new SiteSettings { Id = 1 };
        dbContext.SiteSettings.Add(settings);
        await dbContext.SaveChangesAsync();
        return settings;
    }

    private static SettingsInput ToInput(SiteSettings settings) => new()
    {
        WebsiteUrl = settings.WebsiteUrl,
        SmtpHost = settings.SmtpHost,
        SmtpPort = settings.SmtpPort,
        SmtpUserName = settings.SmtpUserName,
        FromEmail = settings.FromEmail,
        FromName = settings.FromName,
        EnableSsl = settings.EnableSsl,
        AdminNotificationEmail = settings.AdminNotificationEmail
    };

    private static bool IsValidWebsiteUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Uri.TryCreate(NormalizeUrl(value), UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string NormalizeUrl(string value)
    {
        var trimmed = value.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        return trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? trimmed
                : $"https://{trimmed}";
    }

    public class SettingsInput
    {
        [Display(Name = "آدرس وب‌سایت")]
        public string WebsiteUrl { get; set; } = "https://hooshyaran.ir";

        [Display(Name = "SMTP Host")]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "پورت معتبر نیست.")]
        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "SMTP Username")]
        public string SmtpUserName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "SMTP Password")]
        public string SmtpPassword { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "ایمیل فرستنده معتبر نیست.")]
        [Display(Name = "From Email")]
        public string FromEmail { get; set; } = string.Empty;

        [Display(Name = "From Name")]
        public string FromName { get; set; } = "هوش‌یاران";

        public bool EnableSsl { get; set; } = true;

        [EmailAddress(ErrorMessage = "ایمیل ادمین معتبر نیست.")]
        [Display(Name = "ایمیل دریافت اعلان ادمین")]
        public string AdminNotificationEmail { get; set; } = string.Empty;
    }
}

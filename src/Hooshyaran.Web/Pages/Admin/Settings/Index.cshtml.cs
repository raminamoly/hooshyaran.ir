using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Pages.Admin.Settings;

[Authorize(Roles = AdminUserRoles.SuperAdmin)]
public class IndexModel(
    HooshyaranDbContext dbContext,
    IDatabaseExplorerService databaseExplorer,
    ISitemapService sitemapService,
    ISmsSender smsSender) : PageModel
{
    [BindProperty]
    public SettingsInput Input { get; set; } = new();

    [BindProperty]
    public SmsTestInput SmsTest { get; set; } = new();

    public bool Saved { get; private set; }

    public string? BackupErrorMessage { get; private set; }

    public string? EmailTestMessage { get; private set; }

    public bool EmailTestSucceeded { get; private set; }

    public string? SmsTestMessage { get; private set; }

    public bool SmsTestSucceeded { get; private set; }

    public string DatabasePath => databaseExplorer.DatabasePath;

    public SitemapSnapshot? LatestSitemapSnapshot { get; private set; }

    public string SitemapUrl { get; private set; } = "https://hooshyaran.ir/sitemap.xml";

    public string? SitemapMessage { get; private set; }

    public bool SitemapRegenerated { get; private set; }

    public string ActiveSettingsTab { get; private set; } = "general";

    public async Task OnGetAsync()
    {
        var settings = await GetOrCreateSettingsAsync();
        Input = ToInput(settings);
        await LoadSitemapStatusAsync(settings);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!IsValidWebsiteUrl(Input.WebsiteUrl))
        {
            ModelState.AddModelError("Input.WebsiteUrl", "آدرس سایت معتبر نیست. آدرس‌های localhost هم قابل قبول هستند.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSitemapStatusAsync();
            return Page();
        }

        var settings = await GetOrCreateSettingsAsync();
        ApplyInput(settings);
        await dbContext.SaveChangesAsync();
        Input = ToInput(settings);
        await LoadSitemapStatusAsync(settings);
        Saved = true;
        return Page();
    }

    public async Task<IActionResult> OnPostTestEmailAsync(CancellationToken cancellationToken)
    {
        ActiveSettingsTab = "email";
        if (!IsValidWebsiteUrl(Input.WebsiteUrl))
        {
            ModelState.AddModelError("Input.WebsiteUrl", "آدرس سایت معتبر نیست. آدرس‌های localhost هم قابل قبول هستند.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSitemapStatusAsync();
            return Page();
        }

        var settings = await GetOrCreateSettingsAsync();
        ApplyInput(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
        Input = ToInput(settings);
        await LoadSitemapStatusAsync(settings, cancellationToken);

        var validationMessage = ValidateEmailSettings(settings);
        if (validationMessage is not null)
        {
            EmailTestMessage = validationMessage;
            EmailTestSucceeded = false;
            return Page();
        }

        try
        {
            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                EnableSsl = settings.EnableSsl
            };
            SmtpClientConfigurator.SetClientDomain(client, settings.FromEmail, settings.WebsiteUrl);

            if (!string.IsNullOrWhiteSpace(settings.SmtpUserName))
            {
                client.Credentials = new NetworkCredential(settings.SmtpUserName, settings.SmtpPassword);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.FromName),
                Subject = "تست ایمیل هوش‌یاران",
                Body = """
                    این ایمیل برای تست تنظیمات SMTP پنل هوش‌یاران ارسال شده است.

                    اگر این پیام را دریافت کرده‌اید، ارسال ایمیل از پورتال فعال است.
                    """,
                IsBodyHtml = false
            };
            message.To.Add(settings.AdminNotificationEmail);

            await SmtpDelivery.SendMailAsync(
                client,
                message,
                settings.IgnoreInvalidTlsCertificate,
                cancellationToken);
            EmailTestSucceeded = true;
            EmailTestMessage = $"ایمیل تست به {settings.AdminNotificationEmail} ارسال شد.";
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException or FormatException)
        {
            EmailTestSucceeded = false;
            EmailTestMessage = "ارسال ایمیل تست انجام نشد. تنظیمات SMTP، نام کاربری، رمز یا دسترسی سرور را بررسی کنید.";
        }

        await LoadSitemapStatusAsync(settings, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostTestSmsAsync(CancellationToken cancellationToken)
    {
        ActiveSettingsTab = "sms";
        if (!IsValidWebsiteUrl(Input.WebsiteUrl))
        {
            ModelState.AddModelError("Input.WebsiteUrl", "آدرس سایت معتبر نیست. آدرس‌های localhost هم قابل قبول هستند.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSitemapStatusAsync(cancellationToken: cancellationToken);
            return Page();
        }

        var settings = await GetOrCreateSettingsAsync();
        ApplyInput(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
        Input = ToInput(settings);
        await LoadSitemapStatusAsync(settings, cancellationToken);

        var validationMessage = ValidateSmsSettings(settings);
        if (validationMessage is not null)
        {
            SmsTestMessage = validationMessage;
            SmsTestSucceeded = false;
            return Page();
        }

        var testValidationMessage = ValidateSmsTestInput(SmsTest);
        if (testValidationMessage is not null)
        {
            SmsTestMessage = testValidationMessage;
            SmsTestSucceeded = false;
            return Page();
        }

        var result = string.Equals(SmsTest.TemplateType, SmsTemplateTypes.Message, StringComparison.OrdinalIgnoreCase)
            ? await smsSender.SendMessageAsync(SmsTest.Mobile ?? string.Empty, SmsTest.Value ?? string.Empty, cancellationToken)
            : await smsSender.SendOtpAsync(SmsTest.Mobile ?? string.Empty, SmsTest.Value ?? string.Empty, cancellationToken);

        SmsTestSucceeded = result.Succeeded;
        SmsTestMessage = result.Succeeded
            ? $"پیامک تست به {SmsTest.Mobile?.Trim()} ارسال شد."
            : result.Message;

        await LoadSitemapStatusAsync(settings, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostBackupAsync()
    {
        ActiveSettingsTab = "database";
        try
        {
            var backupPath = await databaseExplorer.CreateBackupAsync();

            return PhysicalFile(backupPath, "application/octet-stream", Path.GetFileName(backupPath));
        }
        catch (Exception ex)
        {
            var settings = await GetOrCreateSettingsAsync();
            Input = ToInput(settings);
            await LoadSitemapStatusAsync(settings);
            BackupErrorMessage = $"بکاپ ساخته نشد: {ex.Message}";

            return Page();
        }
    }

    public async Task<IActionResult> OnPostRegenerateSitemapAsync(CancellationToken cancellationToken)
    {
        ActiveSettingsTab = "sitemap";
        if (!IsValidWebsiteUrl(Input.WebsiteUrl))
        {
            ModelState.AddModelError("Input.WebsiteUrl", "آدرس سایت معتبر نیست. آدرس‌های localhost هم قابل قبول هستند.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSitemapStatusAsync(cancellationToken: cancellationToken);
            return Page();
        }

        var settings = await GetOrCreateSettingsAsync();
        ApplyInput(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
        Input = ToInput(settings);

        var snapshot = await sitemapService.RegenerateAsync(cancellationToken);
        SitemapRegenerated = true;
        SitemapMessage = $"سایت‌مپ با {snapshot.UrlCount} آدرس بازتولید شد.";
        await LoadSitemapStatusAsync(settings, cancellationToken);

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

    private void ApplyInput(SiteSettings settings)
    {
        settings.WebsiteUrl = NormalizeUrl(Input.WebsiteUrl);
        settings.SmtpHost = (Input.SmtpHost ?? string.Empty).Trim();
        settings.SmtpPort = Input.SmtpPort;
        settings.SmtpUserName = (Input.SmtpUserName ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(Input.SmtpPassword))
        {
            settings.SmtpPassword = Input.SmtpPassword;
        }
        settings.FromEmail = (Input.FromEmail ?? string.Empty).Trim();
        settings.FromName = (Input.FromName ?? string.Empty).Trim();
        settings.EnableSsl = Input.EnableSsl;
        settings.IgnoreInvalidTlsCertificate = Input.IgnoreInvalidTlsCertificate;
        settings.AdminNotificationEmail = (Input.AdminNotificationEmail ?? string.Empty).Trim();
        settings.SmsApiUrl = NormalizeUrl(Input.SmsApiUrl);
        if (!string.IsNullOrWhiteSpace(Input.SmsApiKey))
        {
            settings.SmsApiKey = Input.SmsApiKey;
        }
        settings.SmsOtpTemplateId = Input.SmsOtpTemplateId;
        settings.SmsMessageTemplateId = Input.SmsMessageTemplateId;
        settings.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static string? ValidateEmailSettings(SiteSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SmtpHost))
        {
            return "برای تست ایمیل، SMTP Host باید وارد شود.";
        }

        if (string.IsNullOrWhiteSpace(settings.FromEmail))
        {
            return "برای تست ایمیل، From Email باید وارد شود.";
        }

        if (string.IsNullOrWhiteSpace(settings.AdminNotificationEmail))
        {
            return "برای تست ایمیل، ایمیل دریافت اعلان ادمین باید وارد شود.";
        }

        return null;
    }

    private static string? ValidateSmsSettings(SiteSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SmsApiUrl))
        {
            return "برای تست پیامک، آدرس API باید وارد شود.";
        }

        if (!IsValidWebsiteUrl(settings.SmsApiUrl))
        {
            return "آدرس API پیامک معتبر نیست.";
        }

        if (string.IsNullOrWhiteSpace(settings.SmsApiKey))
        {
            return "برای تست پیامک، کلید API باید وارد شود.";
        }

        if (settings.SmsOtpTemplateId <= 0 || settings.SmsMessageTemplateId <= 0)
        {
            return "شناسه قالب‌های پیامک باید معتبر باشد.";
        }

        return null;
    }

    private static string? ValidateSmsTestInput(SmsTestInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Mobile))
        {
            return "شماره موبایل تست را وارد کنید.";
        }

        if (string.IsNullOrWhiteSpace(input.Value))
        {
            return "مقدار پارامتر تست را وارد کنید.";
        }

        if (!string.Equals(input.TemplateType, SmsTemplateTypes.Otp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(input.TemplateType, SmsTemplateTypes.Message, StringComparison.OrdinalIgnoreCase))
        {
            return "نوع قالب پیامک معتبر نیست.";
        }

        return null;
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
        IgnoreInvalidTlsCertificate = settings.IgnoreInvalidTlsCertificate,
        AdminNotificationEmail = settings.AdminNotificationEmail,
        SmsApiUrl = settings.SmsApiUrl,
        SmsOtpTemplateId = settings.SmsOtpTemplateId,
        SmsMessageTemplateId = settings.SmsMessageTemplateId
    };

    private static bool IsValidWebsiteUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Uri.TryCreate(NormalizeUrl(value), UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string NormalizeUrl(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        return trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? trimmed
                : $"https://{trimmed}";
    }

    private async Task LoadSitemapStatusAsync(
        SiteSettings? settings = null,
        CancellationToken cancellationToken = default)
    {
        LatestSitemapSnapshot = await sitemapService.GetLatestSnapshotAsync(cancellationToken);
        var baseUrl = NormalizeUrl(settings?.WebsiteUrl ?? Input.WebsiteUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://hooshyaran.ir";
        }

        SitemapUrl = $"{baseUrl.TrimEnd('/')}/sitemap.xml";
    }

    public class SettingsInput
    {
        [Display(Name = "آدرس وب‌سایت")]
        public string? WebsiteUrl { get; set; } = "https://hooshyaran.ir";

        [Display(Name = "SMTP Host")]
        public string? SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "پورت معتبر نیست.")]
        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "SMTP Username")]
        public string? SmtpUserName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "SMTP Password")]
        public string? SmtpPassword { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "ایمیل فرستنده معتبر نیست.")]
        [Display(Name = "From Email")]
        public string? FromEmail { get; set; } = string.Empty;

        [Display(Name = "From Name")]
        public string? FromName { get; set; } = "هوش‌یاران";

        public bool EnableSsl { get; set; } = true;

        [Display(Name = "نادیده گرفتن خطای گواهی TLS")]
        public bool IgnoreInvalidTlsCertificate { get; set; }

        [EmailAddress(ErrorMessage = "ایمیل ادمین معتبر نیست.")]
        [Display(Name = "ایمیل دریافت اعلان ادمین")]
        public string? AdminNotificationEmail { get; set; } = string.Empty;

        [Display(Name = "آدرس API پیامک")]
        public string? SmsApiUrl { get; set; } = "https://api.sms.ir/v1/send/verify";

        [DataType(DataType.Password)]
        [Display(Name = "کلید API پیامک")]
        public string? SmsApiKey { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "شناسه قالب کد یکبارمصرف معتبر نیست.")]
        [Display(Name = "شناسه قالب کد یکبارمصرف")]
        public int SmsOtpTemplateId { get; set; } = 160052;

        [Range(1, int.MaxValue, ErrorMessage = "شناسه قالب پیام ساده معتبر نیست.")]
        [Display(Name = "شناسه قالب پیام ساده")]
        public int SmsMessageTemplateId { get; set; } = 391212;
    }

    public class SmsTestInput
    {
        [Display(Name = "شماره موبایل تست")]
        public string? Mobile { get; set; } = string.Empty;

        [Display(Name = "نوع قالب")]
        public string? TemplateType { get; set; } = SmsTemplateTypes.Otp;

        [Display(Name = "مقدار پارامتر")]
        public string? Value { get; set; } = string.Empty;
    }

    public static class SmsTemplateTypes
    {
        public const string Otp = "otp";

        public const string Message = "message";
    }
}

using System.Net;
using System.Net.Mail;
using Hooshyaran.Web.Models;

namespace Hooshyaran.Web.Services;

public class DemoRequestEmailService(
    ISiteSettingsService siteSettingsService,
    ILogger<DemoRequestEmailService> logger) : IDemoRequestEmailService
{
    public async Task NotifyAsync(DemoRequest demoRequest, CancellationToken cancellationToken = default)
    {
        var settings = await siteSettingsService.GetAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(settings.SmtpHost) ||
            string.IsNullOrWhiteSpace(settings.FromEmail) ||
            string.IsNullOrWhiteSpace(settings.AdminNotificationEmail))
        {
            logger.LogInformation("Demo request email skipped because SMTP settings are incomplete.");
            return;
        }

        using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
        {
            EnableSsl = settings.EnableSsl
        };
        SmtpClientConfigurator.SetClientDomain(client, settings.FromEmail, settings.WebsiteUrl);

        if (!string.IsNullOrWhiteSpace(settings.SmtpUserName))
        {
            client.Credentials = new NetworkCredential(settings.SmtpUserName, settings.SmtpPassword);
        }

        try
        {
            using var adminMessage = BuildMessage(
                settings,
                settings.AdminNotificationEmail,
                $"درخواست جلسه دمو جدید از {demoRequest.OrganizationName}",
                BuildAdminBody(demoRequest));
            await client.SendMailAsync(adminMessage, cancellationToken);

            if (!string.IsNullOrWhiteSpace(demoRequest.Email))
            {
                using var userMessage = BuildMessage(
                    settings,
                    demoRequest.Email,
                    "درخواست جلسه معرفی شما دریافت شد",
                    BuildUserBody(demoRequest));
                await client.SendMailAsync(userMessage, cancellationToken);
            }
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException)
        {
            logger.LogWarning(exception, "Demo request email could not be sent.");
        }
    }

    private static MailMessage BuildMessage(SiteSettings settings, string to, string subject, string body)
    {
        var message = new MailMessage
        {
            From = new MailAddress(settings.FromEmail, settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(to);
        return message;
    }

    private static string BuildAdminBody(DemoRequest request) => $"""
        درخواست جلسه دمو جدید ثبت شد.

        نام: {request.FullName}
        سازمان: {request.OrganizationName}
        سمت: {request.JobTitle}
        تلفن: {request.PhoneNumber}
        ایمیل: {request.Email}
        حوزه نیاز: {request.NeedArea}
        زمان پیشنهادی: {request.PreferredTime}

        توضیحات:
        {request.Notes}
        """;

    private static string BuildUserBody(DemoRequest request) => $"""
        {request.FullName} عزیز،

        درخواست جلسه معرفی شما برای سازمان {request.OrganizationName} دریافت شد.
        تیم هوش‌یاران پس از بررسی، برای هماهنگی زمان جلسه با شما تماس می‌گیرد.

        با احترام
        هوش‌یاران
        """;
}

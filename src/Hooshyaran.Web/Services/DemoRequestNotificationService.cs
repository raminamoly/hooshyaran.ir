using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Services;

public class DemoRequestNotificationService(
    HooshyaranDbContext dbContext,
    ISiteSettingsService siteSettingsService,
    ISmsSender smsSender,
    ILogger<DemoRequestNotificationService> logger) : IDemoRequestNotificationService
{
    public async Task NotifyAsync(DemoRequest demoRequest, CancellationToken cancellationToken = default)
    {
        var recipients = await dbContext.AdminUsers
            .AsNoTracking()
            .Where(user => user.IsActive && user.ReceiveDemoRequestNotifications)
            .OrderBy(user => user.DisplayName)
            .ToListAsync(cancellationToken);

        if (recipients.Count == 0)
        {
            logger.LogInformation("Demo request notification skipped because no active admin user is subscribed.");
        }

        var settings = await siteSettingsService.GetAsync(cancellationToken);
        await SendInternalEmailsAsync(settings, demoRequest, recipients, cancellationToken);
        await SendRequesterConfirmationAsync(settings, demoRequest, cancellationToken);
        await SendInternalSmsAsync(demoRequest, recipients, cancellationToken);
    }

    private async Task SendInternalEmailsAsync(
        SiteSettings settings,
        DemoRequest demoRequest,
        List<AdminUser> recipients,
        CancellationToken cancellationToken)
    {
        var recipientEmails = recipients
            .Select(user => user.Email.Trim())
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (recipientEmails.Count == 0)
        {
            logger.LogInformation("Demo request internal email skipped because no subscribed user has an email address.");
            return;
        }

        if (!CanSendEmail(settings))
        {
            logger.LogInformation("Demo request internal email skipped because SMTP settings are incomplete.");
            return;
        }

        using var client = CreateSmtpClient(settings);
        foreach (var email in recipientEmails)
        {
            try
            {
                using var message = BuildMessage(
                    settings,
                    email,
                    $"درخواست جلسه دمو جدید از {demoRequest.OrganizationName}",
                    BuildInternalBody(demoRequest),
                    BuildInternalHtmlBody(demoRequest));
                await SmtpDelivery.SendMailAsync(
                    client,
                    message,
                    settings.IgnoreInvalidTlsCertificate,
                    cancellationToken);
            }
            catch (Exception exception) when (exception is SmtpException or InvalidOperationException or FormatException)
            {
                logger.LogWarning(exception, "Demo request internal email could not be sent to {Email}.", email);
            }
        }
    }

    private async Task SendRequesterConfirmationAsync(
        SiteSettings settings,
        DemoRequest demoRequest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(demoRequest.Email))
        {
            return;
        }

        if (!CanSendEmail(settings))
        {
            logger.LogInformation("Demo request confirmation email skipped because SMTP settings are incomplete.");
            return;
        }

        try
        {
            using var client = CreateSmtpClient(settings);
            using var message = BuildMessage(
                settings,
                demoRequest.Email,
                "درخواست جلسه معرفی شما دریافت شد",
                BuildRequesterBody(demoRequest),
                BuildRequesterHtmlBody(demoRequest));
            await SmtpDelivery.SendMailAsync(
                client,
                message,
                settings.IgnoreInvalidTlsCertificate,
                cancellationToken);
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException or FormatException)
        {
            logger.LogWarning(exception, "Demo request confirmation email could not be sent.");
        }
    }

    private async Task SendInternalSmsAsync(
        DemoRequest demoRequest,
        List<AdminUser> recipients,
        CancellationToken cancellationToken)
    {
        var mobileNumbers = recipients
            .Select(user => user.MobileNumber.Trim())
            .Where(mobile => !string.IsNullOrWhiteSpace(mobile))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (mobileNumbers.Count == 0)
        {
            logger.LogInformation("Demo request SMS notification skipped because no subscribed user has a mobile number.");
            return;
        }

        var smsMessage = $"یک درخواست دمو جدید توسط {demoRequest.OrganizationName} در وب‌سایت هوش‌یاران ثبت شد. لطفا ایمیل خود را بررسی کنید.";
        foreach (var mobile in mobileNumbers)
        {
            try
            {
                var result = await smsSender.SendMessageAsync(mobile, smsMessage, cancellationToken);
                if (!result.Succeeded)
                {
                    logger.LogWarning(
                        "Demo request SMS notification could not be sent to {Mobile}. Message: {Message}",
                        mobile,
                        result.Message);
                }
            }
            catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(exception, "Demo request SMS notification could not be sent to {Mobile}.", mobile);
            }
        }
    }

    private static bool CanSendEmail(SiteSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.SmtpHost) &&
        !string.IsNullOrWhiteSpace(settings.FromEmail);

    private static SmtpClient CreateSmtpClient(SiteSettings settings)
    {
        var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
        {
            EnableSsl = settings.EnableSsl
        };
        SmtpClientConfigurator.SetClientDomain(client, settings.FromEmail, settings.WebsiteUrl);

        if (!string.IsNullOrWhiteSpace(settings.SmtpUserName))
        {
            client.Credentials = new NetworkCredential(settings.SmtpUserName, settings.SmtpPassword);
        }

        return client;
    }

    private static MailMessage BuildMessage(
        SiteSettings settings,
        string to,
        string subject,
        string textBody,
        string htmlBody)
    {
        var message = new MailMessage
        {
            From = new MailAddress(settings.FromEmail, settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);
        message.AlternateViews.Add(
            AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));
        message.AlternateViews.Add(
            AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html"));
        return message;
    }

    private static string BuildInternalBody(DemoRequest request) => $"""
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

    private static string BuildRequesterBody(DemoRequest request) => $"""
        {request.FullName} عزیز،

        درخواست جلسه معرفی شما برای سازمان {request.OrganizationName} دریافت شد.
        تیم هوش‌یاران پس از بررسی، برای هماهنگی زمان جلسه با شما تماس می‌گیرد.

        با احترام
        هوش‌یاران
        """;

    private static string BuildInternalHtmlBody(DemoRequest request)
    {
        var fields = new (string Label, string Value)[]
        {
            ("نام و نام خانوادگی", request.FullName),
            ("نام سازمان یا شرکت", request.OrganizationName),
            ("سمت سازمانی", request.JobTitle),
            ("شماره تماس", request.PhoneNumber),
            ("ایمیل کاری", request.Email),
            ("موضوع مورد نظر برای جلسه", request.NeedArea),
            ("زمان پیشنهادی جلسه", request.PreferredTime)
        };

        var encodedRows = string.Join(string.Empty, fields.Select(field => $$"""
            <tr>
              <td style="padding:14px 16px;border-bottom:1px solid #d9e4ea;background:#f7fafc;color:#5c6b77;font-size:13px;font-weight:700;white-space:nowrap;">{{Encode(field.Label)}}</td>
              <td style="padding:14px 16px;border-bottom:1px solid #d9e4ea;color:#15324a;font-size:14px;line-height:1.9;">{{EncodeOrFallback(field.Value)}}</td>
            </tr>
            """));

        return $$"""
            <!DOCTYPE html>
            <html lang="fa" dir="rtl">
            <body style="margin:0;background:#eef4f7;font-family:Tahoma,Arial,sans-serif;color:#15324a;direction:rtl;text-align:right;">
              <div style="max-width:760px;margin:0 auto;padding:28px 16px;direction:rtl;text-align:right;">
                <div style="border-radius:16px;overflow:hidden;background:#ffffff;border:1px solid #d9e4ea;box-shadow:0 16px 44px rgba(21,50,74,0.08);direction:rtl;text-align:right;">
                  <div style="padding:24px 28px;background:linear-gradient(135deg,#0a6a74,#0eb4ac);color:#ffffff;direction:rtl;text-align:right;">
                    <div style="font-size:13px;opacity:0.92;">هوش‌یاران</div>
                    <h1 style="margin:10px 0 0;font-size:24px;line-height:1.8;">درخواست جلسه دمو جدید ثبت شد</h1>
                    <p style="margin:10px 0 0;font-size:14px;line-height:2;opacity:0.96;">یک درخواست جدید برای بررسی و پیگیری فروش در وب‌سایت ثبت شده است.</p>
                  </div>

                  <div style="padding:24px 28px;direction:rtl;text-align:right;">
                    <table dir="rtl" style="width:100%;border-collapse:collapse;border:1px solid #d9e4ea;border-radius:12px;overflow:hidden;direction:rtl;text-align:right;">
                      <tbody>
                        {{encodedRows}}
                      </tbody>
                    </table>

                    <div style="margin-top:20px;border:1px solid #d9e4ea;border-radius:12px;background:#f8fbfd;padding:18px 20px;direction:rtl;text-align:right;">
                      <div style="font-size:13px;font-weight:700;color:#5c6b77;margin-bottom:8px;">توضیح کوتاه</div>
                      <div style="font-size:14px;line-height:2;color:#15324a;white-space:pre-wrap;">{{EncodeOrFallback(request.Notes)}}</div>
                    </div>
                  </div>
                </div>
              </div>
            </body>
            </html>
            """;
    }

    private static string BuildRequesterHtmlBody(DemoRequest request) => $$"""
        <!DOCTYPE html>
        <html lang="fa" dir="rtl">
        <body style="margin:0;background:#eef4f7;font-family:Tahoma,Arial,sans-serif;color:#15324a;direction:rtl;text-align:right;">
          <div style="max-width:680px;margin:0 auto;padding:28px 16px;direction:rtl;text-align:right;">
            <div style="border-radius:16px;overflow:hidden;background:#ffffff;border:1px solid #d9e4ea;box-shadow:0 16px 44px rgba(21,50,74,0.08);direction:rtl;text-align:right;">
              <div style="padding:24px 28px;background:linear-gradient(135deg,#0a6a74,#0eb4ac);color:#ffffff;direction:rtl;text-align:right;">
                <div style="font-size:13px;opacity:0.92;">هوش‌یاران</div>
                <h1 style="margin:10px 0 0;font-size:24px;line-height:1.8;">درخواست جلسه معرفی شما دریافت شد</h1>
              </div>

              <div style="padding:24px 28px;direction:rtl;text-align:right;">
                <p style="margin:0 0 14px;font-size:15px;line-height:2;">{{Encode(request.FullName)}} عزیز،</p>
                <p style="margin:0 0 14px;font-size:14px;line-height:2.1;">
                  درخواست جلسه معرفی شما برای سازمان
                  <strong>{{Encode(request.OrganizationName)}}</strong>
                  دریافت شد.
                </p>
                <p style="margin:0 0 14px;font-size:14px;line-height:2.1;">تیم هوش‌یاران پس از بررسی، برای هماهنگی زمان جلسه با شما تماس می‌گیرد.</p>

                <div style="margin-top:18px;border:1px solid #d9e4ea;border-radius:12px;background:#f8fbfd;padding:16px 18px;direction:rtl;text-align:right;">
                  <div style="font-size:13px;font-weight:700;color:#5c6b77;margin-bottom:10px;">خلاصه درخواست</div>
                  <div style="font-size:14px;line-height:2;">
                    <div><strong>موضوع:</strong> {{EncodeOrFallback(request.NeedArea)}}</div>
                    <div><strong>زمان پیشنهادی:</strong> {{EncodeOrFallback(request.PreferredTime)}}</div>
                    <div><strong>شماره تماس:</strong> {{EncodeOrFallback(request.PhoneNumber)}}</div>
                  </div>
                </div>

                <p style="margin:18px 0 0;font-size:14px;line-height:2.1;">با احترام<br />هوش‌یاران</p>
              </div>
            </div>
          </div>
        </body>
        </html>
        """;

    private static string Encode(string? value) =>
        HtmlEncoder.Default.Encode(value ?? string.Empty);

    private static string EncodeOrFallback(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? "ثبت نشده"
            : Encode(value);
}

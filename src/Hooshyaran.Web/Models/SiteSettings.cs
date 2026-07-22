using System.ComponentModel.DataAnnotations;

namespace Hooshyaran.Web.Models;

public class SiteSettings
{
    public int Id { get; set; } = 1;

    [MaxLength(260)]
    public string WebsiteUrl { get; set; } = "https://hooshyaran.ir";

    [MaxLength(180)]
    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    [MaxLength(180)]
    public string SmtpUserName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string SmtpPassword { get; set; } = string.Empty;

    [MaxLength(180)]
    public string FromEmail { get; set; } = string.Empty;

    [MaxLength(180)]
    public string FromName { get; set; } = "هوش‌یاران";

    public bool EnableSsl { get; set; } = true;

    public bool IgnoreInvalidTlsCertificate { get; set; }

    [MaxLength(180)]
    public string AdminNotificationEmail { get; set; } = string.Empty;

    [MaxLength(260)]
    public string SmsApiUrl { get; set; } = "https://api.sms.ir/v1/send/verify";

    [MaxLength(500)]
    public string SmsApiKey { get; set; } = string.Empty;

    public int SmsOtpTemplateId { get; set; } = 160052;

    public int SmsMessageTemplateId { get; set; } = 391212;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

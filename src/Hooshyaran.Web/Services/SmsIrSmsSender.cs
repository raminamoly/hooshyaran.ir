using System.Text;
using System.Text.Json;

namespace Hooshyaran.Web.Services;

public class SmsIrSmsSender(
    HttpClient httpClient,
    ISiteSettingsService siteSettingsService,
    ILogger<SmsIrSmsSender> logger) : ISmsSender
{
    private const string ApiKeyHeaderName = "x-api-key";

    public async Task<SmsSendResult> SendTemplateAsync(
        string mobile,
        int templateId,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mobile))
        {
            return new SmsSendResult(false, null, "شماره موبایل وارد نشده است.");
        }

        if (templateId <= 0)
        {
            return new SmsSendResult(false, null, "شناسه قالب پیامک معتبر نیست.");
        }

        if (parameters.Count == 0)
        {
            return new SmsSendResult(false, null, "پارامترهای قالب پیامک وارد نشده‌اند.");
        }

        var settings = await siteSettingsService.GetAsync(cancellationToken);
        if (!TryGetEndpoint(settings.SmsApiUrl, out var endpoint))
        {
            return new SmsSendResult(false, null, "آدرس API پیامک معتبر نیست.");
        }

        if (string.IsNullOrWhiteSpace(settings.SmsApiKey))
        {
            return new SmsSendResult(false, null, "کلید API پیامک تنظیم نشده است.");
        }

        var model = new VerifySendModel
        {
            Mobile = mobile.Trim(),
            TemplateId = templateId,
            Parameters = parameters
                .Select(parameter => new VerifySendParameterModel
                {
                    Name = parameter.Key.Trim(),
                    Value = parameter.Value.Trim()
                })
                .Where(parameter => !string.IsNullOrWhiteSpace(parameter.Name))
                .ToArray()
        };

        if (model.Parameters.Length == 0)
        {
            return new SmsSendResult(false, null, "نام پارامترهای قالب پیامک معتبر نیست.");
        }

        var payload = JsonSerializer.Serialize(model);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation(ApiKeyHeaderName, settings.SmsApiKey.Trim());

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new SmsSendResult(true, response.StatusCode, "پیامک ارسال شد.", responseBody);
            }

            logger.LogWarning(
                "SMS provider rejected request with status code {StatusCode}: {ResponseBody}",
                (int)response.StatusCode,
                responseBody);

            return new SmsSendResult(
                false,
                response.StatusCode,
                "ارسال پیامک انجام نشد. پاسخ سرویس پیامک را بررسی کنید.",
                responseBody);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            logger.LogWarning(exception, "SMS request could not be sent.");
            return new SmsSendResult(false, null, "ارتباط با سرویس پیامک برقرار نشد.");
        }
    }

    public async Task<SmsSendResult> SendOtpAsync(
        string mobile,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var settings = await siteSettingsService.GetAsync(cancellationToken);
        return await SendTemplateAsync(
            mobile,
            settings.SmsOtpTemplateId,
            new Dictionary<string, string> { ["OTP"] = otp },
            cancellationToken);
    }

    public async Task<SmsSendResult> SendMessageAsync(
        string mobile,
        string message,
        CancellationToken cancellationToken = default)
    {
        var settings = await siteSettingsService.GetAsync(cancellationToken);
        return await SendTemplateAsync(
            mobile,
            settings.SmsMessageTemplateId,
            new Dictionary<string, string> { ["MESSAGE"] = message },
            cancellationToken);
    }

    private static bool TryGetEndpoint(string value, out Uri endpoint)
    {
        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out endpoint!) &&
            (endpoint.Scheme == Uri.UriSchemeHttp || endpoint.Scheme == Uri.UriSchemeHttps);
    }

    private sealed class VerifySendParameterModel
    {
        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    private sealed class VerifySendModel
    {
        public string Mobile { get; set; } = string.Empty;

        public int TemplateId { get; set; }

        public VerifySendParameterModel[] Parameters { get; set; } = [];
    }
}

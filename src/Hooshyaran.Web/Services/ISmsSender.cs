using System.Net;

namespace Hooshyaran.Web.Services;

public interface ISmsSender
{
    Task<SmsSendResult> SendTemplateAsync(
        string mobile,
        int templateId,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken = default);

    Task<SmsSendResult> SendOtpAsync(
        string mobile,
        string otp,
        CancellationToken cancellationToken = default);

    Task<SmsSendResult> SendMessageAsync(
        string mobile,
        string message,
        CancellationToken cancellationToken = default);
}

public sealed record SmsSendResult(
    bool Succeeded,
    HttpStatusCode? StatusCode,
    string Message,
    string? ResponseBody = null);

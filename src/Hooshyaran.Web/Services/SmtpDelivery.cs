using System.Net;
using System.Net.Mail;

namespace Hooshyaran.Web.Services;

public static class SmtpDelivery
{
    private static readonly SemaphoreSlim InvalidCertificateBypassLock = new(1, 1);

    public static async Task SendMailAsync(
        SmtpClient client,
        MailMessage message,
        bool ignoreInvalidTlsCertificate,
        CancellationToken cancellationToken = default)
    {
        if (!ignoreInvalidTlsCertificate)
        {
            await client.SendMailAsync(message, cancellationToken);
            return;
        }

        await InvalidCertificateBypassLock.WaitAsync(cancellationToken);
        var previousCallback = ServicePointManager.ServerCertificateValidationCallback;
        ServicePointManager.ServerCertificateValidationCallback = static (_, _, _, _) => true;

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        finally
        {
            ServicePointManager.ServerCertificateValidationCallback = previousCallback;
            InvalidCertificateBypassLock.Release();
        }
    }
}

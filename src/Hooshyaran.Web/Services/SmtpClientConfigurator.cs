using System.Net.Mail;
using System.Reflection;

namespace Hooshyaran.Web.Services;

public static class SmtpClientConfigurator
{
    private static readonly FieldInfo? ClientDomainField = typeof(SmtpClient)
        .GetField("_clientDomain", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void SetClientDomain(SmtpClient client, string fromEmail, string websiteUrl)
    {
        var domain = GetDomain(fromEmail);
        if (string.IsNullOrWhiteSpace(domain) &&
            Uri.TryCreate(websiteUrl, UriKind.Absolute, out var uri))
        {
            domain = uri.Host;
        }

        if (!string.IsNullOrWhiteSpace(domain))
        {
            ClientDomainField?.SetValue(client, domain);
        }
    }

    private static string GetDomain(string email)
    {
        var atIndex = email.LastIndexOf('@');
        return atIndex >= 0 && atIndex < email.Length - 1
            ? email[(atIndex + 1)..]
            : string.Empty;
    }
}

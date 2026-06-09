namespace Hooshyaran.Web.Services;

public class PublicUrlBuilder(ISiteSettingsService siteSettingsService) : IPublicUrlBuilder
{
    public async Task<string> BuildAsync(HttpRequest request, string path, CancellationToken cancellationToken = default)
    {
        var normalizedPath = string.IsNullOrWhiteSpace(path)
            ? "/"
            : path.StartsWith('/') ? path : $"/{path}";

        var requestHost = request.Host.Host;
        var isLocalRequest = request.Host.HasValue &&
            (requestHost.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
             requestHost.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
             requestHost.Equals("::1", StringComparison.OrdinalIgnoreCase));

        if (isLocalRequest || !request.Host.HasValue)
        {
            return $"{request.Scheme}://{request.Host}{normalizedPath}";
        }

        var settings = await siteSettingsService.GetAsync(cancellationToken);
        if (Uri.TryCreate(settings.WebsiteUrl, UriKind.Absolute, out var configuredUrl))
        {
            return new Uri(configuredUrl, normalizedPath).ToString();
        }

        return $"{request.Scheme}://{request.Host}{normalizedPath}";
    }
}

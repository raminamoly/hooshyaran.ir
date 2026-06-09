namespace Hooshyaran.Web.Services;

public interface IPublicUrlBuilder
{
    Task<string> BuildAsync(HttpRequest request, string path, CancellationToken cancellationToken = default);
}

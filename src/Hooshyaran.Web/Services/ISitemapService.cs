using Hooshyaran.Web.Models;

namespace Hooshyaran.Web.Services;

public interface ISitemapService
{
    Task<SitemapBuildResult> BuildAsync(CancellationToken cancellationToken = default);

    Task<SitemapSnapshot?> GetLatestSnapshotAsync(CancellationToken cancellationToken = default);

    Task<SitemapSnapshot> RegenerateAsync(CancellationToken cancellationToken = default);
}

public sealed record SitemapBuildResult(string Xml, int UrlCount, string BaseUrl);

using Hooshyaran.Web.Models;

namespace Hooshyaran.Web.Services;

public interface ISiteSettingsService
{
    Task<SiteSettings> GetAsync(CancellationToken cancellationToken = default);
}

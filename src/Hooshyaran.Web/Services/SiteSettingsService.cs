using Hooshyaran.Web.Data;
using Hooshyaran.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Services;

public class SiteSettingsService(HooshyaranDbContext dbContext) : ISiteSettingsService
{
    public async Task<SiteSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.SiteSettings
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return settings ?? new SiteSettings();
    }
}

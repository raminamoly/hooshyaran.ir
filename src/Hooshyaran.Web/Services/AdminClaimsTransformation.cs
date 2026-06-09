using System.Security.Claims;
using Hooshyaran.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Services;

public class AdminClaimsTransformation(HooshyaranDbContext dbContext) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        var identity = principal.Identity as ClaimsIdentity;
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identity is null || !int.TryParse(userIdValue, out var userId))
        {
            return principal;
        }

        var adminUser = await dbContext.AdminUsers
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId && user.IsActive);
        if (adminUser is null)
        {
            return principal;
        }

        AddOrReplace(identity, ClaimTypes.Role, adminUser.Role);
        AddOrReplace(identity, ClaimTypes.Email, adminUser.Email);
        AddOrReplace(identity, "DisplayName", adminUser.DisplayName);

        return principal;
    }

    private static void AddOrReplace(ClaimsIdentity identity, string type, string value)
    {
        foreach (var claim in identity.FindAll(type).ToList())
        {
            identity.RemoveClaim(claim);
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            identity.AddClaim(new Claim(type, value));
        }
    }
}

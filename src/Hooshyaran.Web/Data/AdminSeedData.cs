using Hooshyaran.Web.Models;
using Hooshyaran.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Data;

public static class AdminSeedData
{
    public static async Task SeedAsync(IServiceProvider services, HooshyaranDbContext dbContext)
    {
        if (await dbContext.AdminUsers.AnyAsync())
        {
            return;
        }

        var configuration = services.GetRequiredService<IConfiguration>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var userName = configuration["Admin:DefaultUserName"] ?? "admin";
        var password = configuration["Admin:DefaultPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Admin:DefaultPassword must be configured when admin seed is enabled.");
        }

        dbContext.AdminUsers.Add(new AdminUser
        {
            UserName = userName,
            DisplayName = "مدیر سایت",
            Email = configuration["Admin:DefaultEmail"] ?? "ramin.amoly@gmail.com",
            PasswordHash = passwordHasher.HashPassword(password),
            Role = AdminUserRoles.SuperAdmin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}

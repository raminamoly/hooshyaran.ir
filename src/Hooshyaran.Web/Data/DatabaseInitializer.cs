using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        Directory.CreateDirectory(Path.Combine(environment.ContentRootPath, "App_Data"));

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HooshyaranDbContext>();

        await dbContext.Database.MigrateAsync();
        await AdminSeedData.SeedAsync(scope.ServiceProvider, dbContext);
        await CmsSeedData.SeedAsync(dbContext);
    }
}

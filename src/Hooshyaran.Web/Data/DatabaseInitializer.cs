using Microsoft.EntityFrameworkCore;

namespace Hooshyaran.Web.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        Directory.CreateDirectory(Path.Combine(environment.ContentRootPath, "App_Data"));

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HooshyaranDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var autoMigrate = environment.IsDevelopment() || configuration.GetValue<bool>("Database:AutoMigrate");
        var seedOnStartup = environment.IsDevelopment() || configuration.GetValue<bool>("Database:SeedOnStartup");

        if (autoMigrate)
        {
            await dbContext.Database.MigrateAsync();
        }

        if (seedOnStartup)
        {
            await MediaSeedData.ImportExistingMediaAsync(dbContext, environment);
            await AdminSeedData.SeedAsync(scope.ServiceProvider, dbContext);
            await ProductSeedData.SeedAsync(dbContext);
            await ArticleSeedData.SeedAsync(dbContext);
        }
    }
}

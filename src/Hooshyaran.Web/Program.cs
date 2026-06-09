using Hooshyaran.Web.Data;
using Hooshyaran.Web.Middleware;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<HooshyaranDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("HooshyaranDb")));
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<IPublicUrlBuilder, PublicUrlBuilder>();
builder.Services.AddScoped<IDemoRequestEmailService, DemoRequestEmailService>();
builder.Services.AddScoped<ISiteVisitLogger, SiteVisitLogger>();
builder.Services.AddScoped<IDatabaseExplorerService, DatabaseExplorerService>();
builder.Services.AddSingleton<ICmsHtmlService, CmsHtmlService>();
builder.Services.AddScoped<IClaimsTransformation, AdminClaimsTransformation>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = "Hooshyaran.Admin";
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
    [
        "image/svg+xml",
        "font/woff2"
    ]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
        context.Context.Response.Headers.Expires = DateTimeOffset.UtcNow.AddYears(1).ToString("R");
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SiteVisitLoggingMiddleware>();

app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method)
        && !context.Request.Path.StartsWithSegments("/admin"))
    {
        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode == StatusCodes.Status200OK
                && context.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true
                && !context.Response.Headers.ContainsKey("Cache-Control"))
            {
                context.Response.Headers.CacheControl = "public,max-age=3600,stale-while-revalidate=86400";
            }

            return Task.CompletedTask;
        });
    }

    await next();
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await DatabaseInitializer.InitializeAsync(app.Services, app.Environment);

app.Run();

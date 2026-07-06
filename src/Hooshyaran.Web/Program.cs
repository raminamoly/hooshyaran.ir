using Hooshyaran.Web.Data;
using Hooshyaran.Web.Middleware;
using Hooshyaran.Web.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);
var databaseConnectionString = builder.Configuration.GetConnectionString("HooshyaranDb");
if (string.IsNullOrWhiteSpace(databaseConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:HooshyaranDb must be configured before the application starts.");
}

// Add services to the container.
builder.Services.AddDbContext<HooshyaranDbContext>(options =>
    options.UseSqlServer(databaseConnectionString));
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<IPublicUrlBuilder, PublicUrlBuilder>();
builder.Services.AddScoped<IDemoRequestEmailService, DemoRequestEmailService>();
builder.Services.AddScoped<ISiteVisitLogger, SiteVisitLogger>();
builder.Services.AddScoped<IDatabaseExplorerService, DatabaseExplorerService>();
builder.Services.AddScoped<IMediaFileService, MediaFileService>();
builder.Services.AddSingleton<ICmsHtmlService, CmsHtmlService>();
builder.Services.AddScoped<IClaimsTransformation, AdminClaimsTransformation>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = "Hooshyaran.Admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
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
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseResponseCompression();

app.Use(async (context, next) =>
{
    if (string.Equals(context.Request.Host.Host, "www.hooshyaran.ir", StringComparison.OrdinalIgnoreCase))
    {
        var target = string.Concat(
            "https://hooshyaran.ir",
            context.Request.PathBase.ToUriComponent(),
            context.Request.Path.ToUriComponent(),
            context.Request.QueryString.ToUriComponent());
        context.Response.Redirect(target, permanent: true);
        return;
    }

    await next();
});

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.Remove("X-Powered-By");
    headers.TryAdd("X-Content-Type-Options", "nosniff");
    headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
    headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

    var contentSecurityPolicy = app.Environment.IsDevelopment()
        ? "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'self'; frame-src https://www.openstreetmap.org; img-src 'self' data: https:; font-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self'; form-action 'self'"
        : "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'self'; frame-src https://www.openstreetmap.org; img-src 'self' data: https:; font-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self'; form-action 'self'; upgrade-insecure-requests";
    headers.TryAdd("Content-Security-Policy", contentSecurityPolicy);

    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("X-Powered-By");
        return Task.CompletedTask;
    });

    await next();
});

app.UseLegacyRedirects();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (context.Context.Request.Path.Equals("/service-worker.js", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.Headers.CacheControl = "no-cache,no-store,must-revalidate";
            context.Context.Response.Headers.Pragma = "no-cache";
            context.Context.Response.Headers.Expires = "0";
            return;
        }

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

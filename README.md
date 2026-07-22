# hooshyaran.ir

Production website and CMS-lite codebase for Hooshyaran, an enterprise AI product and services company.

Live site:

```text
https://hooshyaran.ir
```

## Overview

This repository contains a Persian RTL ASP.NET Core website for publishing Hooshyaran's enterprise AI products, service pages, articles, tags, media, demo requests, and SEO metadata.

The site is server-rendered with Razor Pages and backed by SQL Server through Entity Framework Core. It is designed for production deployment on Windows Server, IIS, ASP.NET Core/Kestrel, and SQL Server.

Main areas:

- Public marketing pages for enterprise AI services.
- Product catalog pages for AI platforms and tools.
- Blog/article pages with tag navigation.
- Admin CMS-lite screens for content, media, SEO, settings, users, and demo requests.
- Production sitemap and robots endpoints.
- Structured metadata for SEO and social sharing.
- Demo request notification flow through email and SMS settings.

## Stack

- .NET / ASP.NET Core Razor Pages
- Target framework: `net10.0`
- SQL Server
- Entity Framework Core SQL Server
- Cookie authentication for admin pages
- Razor Pages, local CSS, local JavaScript, local fonts, local media assets
- IIS + Kestrel in production

Main project:

```text
src/Hooshyaran.Web
```

Solution:

```text
Hooshyaran.sln
```

## Public Routes

Important public routes include:

```text
/
/products
/blog
/solutions
/use-cases
/ai-integrator
/enterprise-ai-implementation
/enterprise-ai-chatbot
/ai-knowledge-base
/private-enterprise-ai
/request-demo
/about
/contact
/privacy
/robots.txt
/sitemap.xml
```

Product and article detail pages use slug routes:

```text
/products/{slug}
/blog/{slug}
/tags/{slug}
```

## SEO

The app includes production SEO support:

- Self-referencing canonical links.
- Open Graph and Twitter card metadata.
- JSON-LD structured data in the shared layout.
- `Organization` and `WebSite` schema on public pages.
- `Service`, `BreadcrumbList`, and optional `FAQPage` schema on product pages.
- `Article` and `BreadcrumbList` schema on blog article pages.
- Dynamic `robots.txt`.
- Dynamic `sitemap.xml`.
- Sitemap snapshot support through `SitemapSnapshots`.
- Permanent canonical host redirect from `www.hooshyaran.ir` to `hooshyaran.ir`.
- Permanent HTTP to HTTPS redirect when enabled.

Current production sitemap target:

```text
https://hooshyaran.ir/sitemap.xml
```

## Configuration

The application requires a SQL Server connection string:

```text
ConnectionStrings:HooshyaranDb
```

Default `appsettings.json` intentionally contains an empty connection string. Do not publish local secrets.

Production-relevant settings:

```json
{
  "Database": {
    "AutoMigrate": false,
    "SeedOnStartup": false,
    "AllowAdminRestore": false
  },
  "Security": {
    "EnableHttpsRedirection": true,
    "EnableHsts": true
  },
  "CanonicalHost": {
    "Enabled": true,
    "SourceHost": "www.hooshyaran.ir",
    "TargetHost": "hooshyaran.ir",
    "Scheme": "https",
    "Permanent": true
  }
}
```

Admin seed credentials are read from development configuration:

```text
Admin:DefaultUserName
Admin:DefaultPassword
```

Never commit or publish production credentials.

## Local Development

Restore and build:

```powershell
dotnet restore Hooshyaran.sln
dotnet build Hooshyaran.sln
```

Run locally:

```powershell
dotnet run --project src/Hooshyaran.Web --urls http://127.0.0.1:5109
```

Use environment variables or development settings for the local database connection string.

Example:

```powershell
$env:ConnectionStrings__HooshyaranDb="Server=.\SQL2019;Database=HooshyaranWebSite;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet run --project src/Hooshyaran.Web --urls http://127.0.0.1:5109
```

## Validation

Run a Release build:

```powershell
dotnet build Hooshyaran.sln -c Release
```

Run the production publish smoke test:

```powershell
.\scripts\smoke-production-publish.ps1
```

The smoke test verifies:

- Release build.
- Publish output excludes `appsettings.Development.json`.
- Main public routes return `200`.
- `robots.txt` and `sitemap.xml` return `200`.
- Security headers are present.
- Social metadata is present.
- JSON-LD is present.
- Referenced assets return `200`.

## Production Deployment

Production changes are shipped as server patch packages. Local source changes alone are not enough.

Create a server patch:

```powershell
.\scripts\create-server-patch.ps1
```

With a specific SQL patch:

```powershell
.\scripts\create-server-patch.ps1 -PatchName "hooshyaran-server-patch-YYYYMMDD-HHmm" -SqlPatch "deploy\your-patch.sql"
```

Patch output convention:

```text
C:\publish\hooshyaran-server-patch-YYYYMMDD-HHmm.zip
C:\publish\hooshyaran-server-patch-YYYYMMDD-HHmm-sql.sql
```

Before deploying to the server:

- Back up the current IIS site folder.
- Back up the production database.
- Stop the IIS site or app pool.
- Preserve the production `appsettings.json` connection string and secrets.
- Copy the new publish files.
- Run the SQL patch only when one is provided.
- Start the app pool.

Post-deploy checks:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```

After SEO-related deployments, re-submit the sitemap in Google Search Console and Bing Webmaster Tools.

## Repository Hygiene

Commit source, migrations, deploy scripts, optimized assets, and deployment notes required for production patches.

Do not commit:

- publish output
- local logs
- temporary screenshots
- `.codex-run`
- local design experiments
- database backups
- local secrets

Temporary and generated files are ignored where possible through `.gitignore`.

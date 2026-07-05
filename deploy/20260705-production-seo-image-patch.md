# Production patch notes

Patch goal:

```text
SEO endpoints, social metadata, optimized article images, www redirect, IIS header cleanup
```

Package:

```text
C:\publish\hooshyaran-server-patch-YYYYMMDD-HHmm.zip
```

SQL patch:

```text
deploy\20260705-production-seo-image-patch.sql
```

## Before deployment

1. Back up the production SQL Server database.
2. Back up the current IIS site folder.
3. Stop the IIS app pool for `hooshyaran.ir`.
4. Keep a copy of the production `appsettings.json`.

## File deployment

1. Extract the zip package to a temporary folder.
2. Copy the publish files over the IIS site folder.
3. Re-apply the production `appsettings.json` if it was overwritten.
4. Confirm `web.config` exists and uses the ASP.NET Core module.

## Database patch

Run this script against the production database:

```text
20260705-production-seo-image-patch.sql
```

The script only replaces known PNG image URLs with their optimized WebP URLs.
It does not change the schema.

## Post-deploy checks

Open these URLs:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```

Expected:

- All routes return 200.
- `robots.txt` includes the sitemap URL.
- `sitemap.xml` returns XML.
- Blog cards load WebP thumbnails.
- Browser devtools does not show missing assets.
- `https://www.hooshyaran.ir/` redirects to `https://hooshyaran.ir/` if the `www` binding reaches this IIS site.

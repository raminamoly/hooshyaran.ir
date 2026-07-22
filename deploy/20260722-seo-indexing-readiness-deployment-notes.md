# SEO indexing readiness patch

Prepared:

```text
2026-07-22 20:27
```

## Package files

Publish package:

```text
C:\publish\hooshyaran-server-patch-20260722-2027.zip
```

SQL patch:

```text
C:\publish\hooshyaran-server-patch-20260722-2027-sql.sql
```

Source SQL file:

```text
deploy\20260722-seo-indexing-readiness.sql
```

## What changes

- HTTP to HTTPS redirection now uses `301 Moved Permanently`.
- `www.hooshyaran.ir` remains configured to redirect permanently to `https://hooshyaran.ir`.
- Shared layout emits JSON-LD structured data with literal `application/ld+json`.
- Product detail pages emit `Service`, `BreadcrumbList`, and `FAQPage` JSON-LD when FAQ rows exist.
- Blog detail pages emit `Article` and `BreadcrumbList` JSON-LD.
- Short tag descriptions are improved through an idempotent SQL patch when existing descriptions are under 50 characters.
- `robots.txt` behavior is unchanged.
- Sitemap behavior is unchanged; the smoke output still reports 54 sitemap URLs.

## Local validation

Production smoke test passed:

```powershell
.\scripts\smoke-production-publish.ps1
```

Additional checks against the published output:

```text
HTTP redirect: 301 Moved Permanently
Product JSON-LD blocks on /products/llmops: 5
Article JSON-LD blocks on /blog/what-is-llmops: 4
Sitemap URL count: 54
Sitemap lastmod count: 35
```

Build warnings observed:

```text
NU1902: AngleSharp 0.17.1 has a known moderate severity vulnerability.
SYSLIB0014: ServicePointManager usage in SmtpDelivery is obsolete.
```

These warnings existed outside this SEO patch and did not block publish.

## Server deployment steps

1. Back up the current IIS site folder.
2. Back up the production SQL Server database.
3. Stop the IIS site or app pool.
4. Extract `C:\publish\hooshyaran-server-patch-20260722-2027.zip`.
5. Copy the extracted publish files into the IIS site folder.
6. Preserve the production `appsettings.json` connection string and secrets. Do not blindly overwrite it with an empty/default local value.
7. Run `C:\publish\hooshyaran-server-patch-20260722-2027-sql.sql` against the production database.
8. Start the IIS app pool.

## Post-deploy smoke checklist

These URLs must return `200`:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```

These redirects must return `301`:

```text
http://hooshyaran.ir/
https://www.hooshyaran.ir/
```

These HTML checks should pass:

```text
https://hooshyaran.ir/ contains application/ld+json and schema.org
https://hooshyaran.ir/products/llmops contains Service JSON-LD
https://hooshyaran.ir/blog/what-is-llmops contains Article JSON-LD
```

## Search Console after deploy

1. Open Google Search Console for `sc-domain:hooshyaran.ir`.
2. Re-submit `https://hooshyaran.ir/sitemap.xml`.
3. Check that the sitemap status is successful and discovered URLs are still 54.
4. Inspect these URLs and request indexing only if Google has not crawled the deployed version:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/products/llmops
https://hooshyaran.ir/products/chatbot-builder
https://hooshyaran.ir/blog/what-is-llmops
https://hooshyaran.ir/request-demo
```

5. In Bing Webmaster Tools, submit the same sitemap.

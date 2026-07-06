# About page redesign deployment

## Package

```text
C:\publish\hooshyaran-server-patch-20260706-1241.zip
```

## SQL patch

```text
C:\publish\hooshyaran-server-patch-20260706-1241-sql.sql
```

Source script:

```text
deploy\20260706-about-page-redesign.sql
```

## Local smoke result

Executed successfully before packaging:

```powershell
.\scripts\smoke-production-publish.ps1 -BaseUrl http://127.0.0.1:5255 -PublishPath C:\publish\hooshyaran-smoke-about
```

Additional checks required:

- `/about` returns `200`.
- About page hero, main company image, and all license images load correctly.
- About page has no horizontal overflow on desktop or mobile.
- About page SEO title and description are present.

## Server deployment steps

1. Stop the IIS site or the application pool.
2. Back up the current site folder.
3. Back up the production database.
4. Extract the package:

```text
C:\publish\hooshyaran-server-patch-20260706-1241.zip
```

5. Copy the extracted publish files into the IIS site folder.
6. Preserve the production `appsettings.json` connection string if the copied package has an empty or default value.
7. Run the SQL patch once against the production database with UTF-8 input encoding:

```powershell
sqlcmd -S .\SQL2019 -d HooshyaranWebSite -E -f 65001 -i C:\publish\hooshyaran-server-patch-20260706-1241-sql.sql
```

8. Start the application pool.

## Post-deploy smoke checklist

Open these URLs and confirm `200` responses:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/about
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```

About page checks:

- The company photo from the reference page is visible.
- The company introduction text is visible and readable.
- The licenses section shows all seven cards.
- All license images load correctly.
- The page has no layout overlap on mobile.

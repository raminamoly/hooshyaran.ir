# Contact page polish deployment

## Package

```text
C:\publish\hooshyaran-server-patch-20260706-1124.zip
```

## SQL patch

```text
C:\publish\hooshyaran-server-patch-20260706-1124-sql.sql
```

Source script:

```text
deploy\20260706-contact-page-polish.sql
```

## Local smoke result

Passed:

```powershell
.\scripts\smoke-production-publish.ps1
```

Verified:

- Release build passed.
- Publish output did not include `appsettings.Development.json`.
- Main public routes returned 200.
- `robots.txt` and `sitemap.xml` returned 200.
- Security headers were present.
- Social metadata was present.
- Referenced assets returned 200.

Additional local checks:

- `/contact` returned 200.
- The contact page no longer renders the offline map notice.
- The contact page no longer renders a sales mobile number pattern.
- The map iframe points to OpenStreetMap with the company coordinates.
- The map column is left of the contact column on desktop.
- The mobile layout has no horizontal page overflow from the contact section.

## Server deployment steps

1. Stop the IIS site or the application pool.
2. Back up the current site folder.
3. Back up the production database.
4. Extract the package:

```text
C:\publish\hooshyaran-server-patch-20260706-1124.zip
```

5. Copy the extracted publish files into the IIS site folder.
6. Preserve the production `appsettings.json` connection string if the copied package has an empty or default value.
7. Run the SQL patch only once against the production database:

```text
C:\publish\hooshyaran-server-patch-20260706-1124-sql.sql
```

8. Start the application pool.

## Post-deploy smoke checklist

Open these URLs and confirm 200 responses:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/contact
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```

Contact page checks:

- The sales mobile number is not visible.
- The fixed phone numbers are visible.
- The company address is visible.
- The company logo is visible in the contact card.
- The OpenStreetMap iframe loads with the marker.
- The old offline-map explanation is not visible.
- The request demo link opens `/request-demo`.

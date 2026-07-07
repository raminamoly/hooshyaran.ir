# SMS settings deployment notes

## Artifacts

Package:

```text
C:\publish\hooshyaran-server-patch-20260707-1048.zip
```

SQL patch:

```text
C:\publish\hooshyaran-server-patch-20260707-1048-sql.sql
```

Source SQL:

```text
deploy\20260707-sms-settings.sql
```

## Local smoke test

Command:

```powershell
.\scripts\smoke-production-publish.ps1
```

Result:

```text
Passed
```

The first smoke run failed because the local database did not yet have the new SMS columns. The local database migration was applied, then the smoke test passed.

## Server deployment steps

1. Back up the current IIS site folder.
2. Back up the production SQL Server database.
3. Stop the IIS site or app pool.
4. Extract the package into the site folder.
5. Preserve the production `appsettings.json` if the package contains an empty or default value.
6. Run the SQL patch against the production database.
7. Start the IIS site or app pool.
8. Sign in to the admin panel and open:

```text
/admin/settings
```

9. Open the SMS tab, enter the API key, confirm the API URL and template IDs, then send a test SMS.

## Post-deploy smoke checklist

Check these URLs:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```

Admin check:

```text
https://hooshyaran.ir/admin/settings
```

Expected:

```text
The SMS settings tab is visible and test sending reports a clear success or provider error.
```

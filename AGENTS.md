# Hooshyaran production workflow

## Production state

The public site is live on:

```text
https://hooshyaran.ir
```

The production stack is:

```text
Windows Server
IIS
ASP.NET Core / Kestrel
SQL Server
```

From now on, local changes are not enough. Every production-facing change must produce a server patch that Ramin can copy to the server.

## Required delivery for production changes

For each production update, prepare:

- A clean publish package as a zip file.
- A SQL patch script when data changes are required.
- Deployment notes with exact copy/run steps.
- A local smoke test result before packaging.
- A post-deploy smoke checklist for the server.

Do not assume the local database is production. If a change needs production data updates, create a script under:

```text
deploy/
```

The script must be idempotent or narrowly scoped to known rows/URLs.

## Local validation before packaging

Run:

```powershell
.\scripts\smoke-production-publish.ps1
```

The smoke test must verify:

- Release build passes.
- Publish output does not include `appsettings.Development.json`.
- Main public routes return 200.
- `robots.txt` and `sitemap.xml` return 200.
- Security headers are present.
- Social metadata is present.
- Referenced assets return 200.

## Server patch convention

Use this package naming pattern:

```text
C:\publish\hooshyaran-server-patch-YYYYMMDD-HHmm.zip
```

If SQL is needed, place it beside the package:

```text
C:\publish\hooshyaran-server-patch-YYYYMMDD-HHmm-sql.sql
```

Ramin will copy the package to the server and replace the IIS site files. The production `appsettings.json` on the server contains the real connection string and must not be overwritten blindly without checking.

## Server deployment notes

Before replacing files on the server:

- Stop the IIS site or app pool.
- Back up the current site folder.
- Back up the production database.
- Preserve or re-apply production `appsettings.json` if the package contains an empty/default value.
- Copy the new publish files.
- Run the SQL patch only if one is provided.
- Start the app pool.

After deploy, check:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```

## Git hygiene

Do not commit local logs, temporary screenshots, `.codex-run`, design experiments, or publish output.

Commit only source, deploy scripts, optimized assets, and docs required for the server patch.

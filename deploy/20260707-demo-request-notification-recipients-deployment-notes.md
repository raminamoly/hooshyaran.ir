# Demo request notification recipients deployment notes

## Artifacts

Package:

```text
C:\publish\hooshyaran-server-patch-20260707-1313.zip
```

SQL patch:

```text
C:\publish\hooshyaran-server-patch-20260707-1313-sql.sql
```

Source SQL:

```text
deploy\20260707-demo-request-notification-recipients.sql
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

## Local manual test

Test user:

```text
admin
```

Notification mobile:

```text
09125177721
```

Submitted organization:

```text
شرکت تست اعلان دمو 20260707-131257
```

Result:

```text
The public request demo form returned the success message and the request appeared in the admin demo requests list.
```

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
/admin/users
```

9. Edit each notification recipient user, set the mobile number, and enable:

```text
دریافت کننده نوتیفیکیشن درخواست دمو
```

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

Admin checks:

```text
https://hooshyaran.ir/admin/users
https://hooshyaran.ir/admin/users/edit
```

Expected:

```text
Admin users can store a mobile number and the demo request notification flag.
New demo requests notify active flagged users by email and SMS.
```

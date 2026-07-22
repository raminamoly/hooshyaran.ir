# SMS test button style deployment notes

## Artifacts

Package:

```text
C:\publish\hooshyaran-server-patch-20260707-1135.zip
```

SQL patch:

```text
None
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

## Server deployment steps

1. Back up the current IIS site folder.
2. Stop the IIS site or app pool.
3. Extract the package into the site folder.
4. Preserve the production `appsettings.json` if the package contains an empty or default value.
5. Start the IIS site or app pool.

## Post-deploy check

Open:

```text
https://hooshyaran.ir/admin/settings
```

Expected:

```text
The SMS settings tab shows a visible, high-contrast "ارسال پیامک تست" button inside the test panel.
```

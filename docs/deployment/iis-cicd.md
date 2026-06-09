# IIS CI/CD Deployment Guide for Hooshyaran.ir

This guide explains how to deploy `hooshyaran.ir` to a Windows Server IIS website using GitHub Actions and a Windows self-hosted runner.

## Repository facts

- Solution: `Hooshyaran.sln`
- Web project: `src/Hooshyaran.Web/Hooshyaran.Web.csproj`
- Framework: `net10.0`
- App type: ASP.NET Core Razor Pages
- Database: SQLite via EF Core
- Default SQLite path: `App_Data/hooshyaran.db`
- Deployment target: Windows Server + IIS

## Why self-hosted runner?

The recommended flow is:

```text
Push to main
  -> GitHub Actions starts
  -> Windows self-hosted runner on the server runs the workflow
  -> dotnet restore/build/publish
  -> files are copied into the IIS physical path
  -> IIS app pool restarts
```

In this model, GitHub does not need to connect to the server IP. The server connects outward to GitHub as a runner. This avoids exposing FTP, WinRM, Web Deploy, or custom deployment ports to the internet.

## Files added for CI/CD

```text
.github/workflows/iis-deploy.yml
scripts/iis/prepare-iis-site.ps1
docs/deployment/iis-cicd.md
```

## Important SQLite rule

The deployment workflow preserves these folders/files:

```text
App_Data/
*.db
*.db-shm
*.db-wal
logs/
wwwroot/uploads/
appsettings.Production.json
appsettings.Local.json
```

Do not commit the production SQLite database to GitHub. The repository `.gitignore` already excludes the runtime database files under `src/Hooshyaran.Web/App_Data`.

## 1. Prepare the Windows Server

Run PowerShell as Administrator on the Windows Server.

From the repository folder or after copying the script to the server, run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\iis\prepare-iis-site.ps1 `
  -SiteName "hooshyaran.ir" `
  -AppPoolName "hooshyaran.ir" `
  -PhysicalPath "C:\inetpub\wwwroot\hooshyaran.ir" `
  -HostName "hooshyaran.ir" `
  -Port 80
```

This script will:

- Ensure IIS and IIS Management Tools are installed when `Install-WindowsFeature` is available.
- Create the IIS physical path.
- Create `App_Data`, `logs`, and `wwwroot/uploads`.
- Create the IIS App Pool.
- Set the App Pool to `No Managed Code`.
- Create the IIS website.
- Grant write permission to the App Pool identity for SQLite and runtime folders.
- Create a basic `appsettings.Production.json` if it does not already exist.

## 2. Install .NET Hosting Bundle

Install the .NET Hosting Bundle on the Windows Server. This installs the ASP.NET Core Module needed for ASP.NET Core apps to run behind IIS.

After installation, restart IIS:

```cmd
net stop was /y
net start w3svc
```

## 3. Configure production settings

Check this file on the server:

```text
C:\inetpub\wwwroot\hooshyaran.ir\appsettings.Production.json
```

Recommended minimal content:

```json
{
  "ConnectionStrings": {
    "HooshyaranDb": "Data Source=App_Data/hooshyaran.db"
  },
  "AllowedHosts": "hooshyaran.ir;www.hooshyaran.ir"
}
```

The workflow intentionally does not overwrite this file.

## 4. Install GitHub self-hosted runner

In GitHub:

```text
Repository -> Settings -> Actions -> Runners -> New self-hosted runner
```

Choose:

```text
Operating system: Windows
Architecture: x64
```

Run the commands that GitHub shows on your Windows Server. Prefer this folder:

```text
C:\actions-runner
```

Install the runner as a Windows service from an Administrator PowerShell window.

After setup, the runner must show as online in:

```text
Repository -> Settings -> Actions -> Runners
```

The workflow uses:

```yaml
runs-on: [self-hosted, windows, x64]
```

So the runner must have these default labels.

## 5. First manual deployment test

Go to:

```text
Repository -> Actions -> IIS CI/CD -> Run workflow
```

The workflow will:

1. Checkout repository.
2. Install/use .NET SDK `10.0.x`.
3. Run `dotnet restore`.
4. Run `dotnet build`.
5. Run tests only if test projects exist.
6. Run `dotnet publish` for `src/Hooshyaran.Web`.
7. Backup the current IIS folder.
8. Put the app temporarily offline.
9. Stop the IIS App Pool.
10. Copy published files to IIS.
11. Preserve SQLite database and runtime folders.
12. Start the IIS App Pool.

## 6. Automatic deployment

After the runner is online, every push to `main` triggers deployment automatically:

```bash
git add .
git commit -m "Update Hooshyaran website"
git push origin main
```

## 7. Backup and rollback

Before each deployment, the workflow creates a backup here:

```text
C:\inetpub\backups\hooshyaran.ir\yyyyMMdd_HHmmss
```

For rollback, copy the desired backup folder back to:

```text
C:\inetpub\wwwroot\hooshyaran.ir
```

Then restart the App Pool:

```powershell
Import-Module WebAdministration
Restart-WebAppPool -Name "hooshyaran.ir"
```

## 8. DNS and SSL

The CI/CD workflow does not need the server IP.

The IP is only needed for DNS and IIS binding:

```text
hooshyaran.ir      -> server public IP
www.hooshyaran.ir  -> server public IP
```

Configure HTTPS/SSL in IIS after the site is reachable over HTTP.

## 9. Security notes

This repository is public. Self-hosted runners on public repositories should be treated carefully.

The current workflow intentionally does not run on `pull_request`. Keep it that way unless you fully understand the security impact.

Recommended rules:

- Do not give unknown users write access to the repository.
- Do not enable deployment workflow on pull requests from forks.
- Keep the runner machine dedicated to deployment.
- Do not store production secrets in committed files.
- Keep `appsettings.Production.json` only on the server.

## 10. Common fixes

### App files are locked

The workflow uses `app_offline.htm` and stops the IIS App Pool before copying files.

### SQLite write error

Grant modify permission to the App Pool identity:

```powershell
icacls "C:\inetpub\wwwroot\hooshyaran.ir\App_Data" /grant "IIS AppPool\hooshyaran.ir:(OI)(CI)M" /T
```

### 500.30 or 500.31 on IIS

Install or repair the .NET Hosting Bundle, then restart IIS:

```cmd
net stop was /y
net start w3svc
```

### Workflow cannot stop App Pool

Run the GitHub runner service with a Windows account that has permission to manage IIS. For the first setup, Administrator is the simplest option. Later, use a dedicated deployment user with limited permissions.

<#
.SYNOPSIS
Prepares a Windows Server IIS website for the Hooshyaran.ir ASP.NET Core application.

.NOTES
Run this script on the Windows Server with Administrator privileges.
The .NET Hosting Bundle must still be installed manually after IIS is enabled.
#>

param(
    [string]$SiteName = "hooshyaran.ir",
    [string]$AppPoolName = "hooshyaran.ir",
    [string]$PhysicalPath = "C:\inetpub\wwwroot\hooshyaran.ir",
    [string]$HostName = "hooshyaran.ir",
    [int]$Port = 80
)

$ErrorActionPreference = "Stop"

function Assert-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run PowerShell as Administrator, then run this script again."
    }
}

Assert-Administrator

if (Get-Command Install-WindowsFeature -ErrorAction SilentlyContinue) {
    Write-Host "Ensuring IIS and IIS Management Tools are installed..."
    Install-WindowsFeature Web-Server, Web-Mgmt-Tools -IncludeManagementTools | Out-Null
}
else {
    Write-Warning "Install-WindowsFeature was not found. Enable IIS manually from Windows Features if needed."
}

Import-Module WebAdministration

Write-Host "Creating directories..."
New-Item -ItemType Directory -Force -Path $PhysicalPath | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $PhysicalPath "App_Data") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $PhysicalPath "logs") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $PhysicalPath "wwwroot\uploads") | Out-Null
New-Item -ItemType Directory -Force -Path "C:\inetpub\backups\hooshyaran.ir" | Out-Null

$appPoolPath = "IIS:\AppPools\$AppPoolName"
if (-not (Test-Path $appPoolPath)) {
    Write-Host "Creating app pool: $AppPoolName"
    New-WebAppPool -Name $AppPoolName | Out-Null
}
else {
    Write-Host "App pool already exists: $AppPoolName"
}

Set-ItemProperty $appPoolPath -Name managedRuntimeVersion -Value ""
Set-ItemProperty $appPoolPath -Name managedPipelineMode -Value "Integrated"
Set-ItemProperty $appPoolPath -Name startMode -Value "AlwaysRunning"
Set-ItemProperty $appPoolPath -Name processModel.idleTimeout -Value ([TimeSpan]::FromMinutes(0))

if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    Write-Host "Creating IIS website: $SiteName"
    New-Website -Name $SiteName -PhysicalPath $PhysicalPath -Port $Port -HostHeader $HostName -ApplicationPool $AppPoolName | Out-Null
}
else {
    Write-Host "IIS website already exists: $SiteName"
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $PhysicalPath
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
}

Write-Host "Granting write access for SQLite/App_Data and logs..."
& icacls (Join-Path $PhysicalPath "App_Data") /grant "IIS AppPool\${AppPoolName}:(OI)(CI)M" /T | Out-Host
& icacls (Join-Path $PhysicalPath "logs") /grant "IIS AppPool\${AppPoolName}:(OI)(CI)M" /T | Out-Host
& icacls (Join-Path $PhysicalPath "wwwroot\uploads") /grant "IIS AppPool\${AppPoolName}:(OI)(CI)M" /T | Out-Host

$productionSettingsPath = Join-Path $PhysicalPath "appsettings.Production.json"
if (-not (Test-Path $productionSettingsPath)) {
    Write-Host "Creating appsettings.Production.json template..."
    @"
{
  "ConnectionStrings": {
    "HooshyaranDb": "Data Source=App_Data/hooshyaran.db"
  },
  "AllowedHosts": "hooshyaran.ir;www.hooshyaran.ir"
}
"@ | Set-Content -Path $productionSettingsPath -Encoding UTF8 -Force
}
else {
    Write-Host "appsettings.Production.json already exists. Keeping current file."
}

Write-Host "Preparation completed. Install the .NET Hosting Bundle, then configure the GitHub self-hosted runner."
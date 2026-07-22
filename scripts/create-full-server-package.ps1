param(
    [string]$ProjectPath = "src\Hooshyaran.Web\Hooshyaran.Web.csproj",
    [string]$OutputRoot = "C:\publish",
    [string]$PackageName = "",
    [string]$DatabaseName = "HooshyaranWebSite",
    [string]$SqlInstance = ".\SQL2019",
    [string]$SmokeBaseUrl = "http://127.0.0.1:5217"
)

$ErrorActionPreference = "Stop"

function Invoke-SqlScalar {
    param(
        [string]$Query
    )

    $result = sqlcmd -S $SqlInstance -d master -E -C -Q $Query -h -1 -W
    if ($LASTEXITCODE -ne 0) {
        throw "sqlcmd failed while running query: $Query"
    }

    return ($result | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1).Trim()
}

if ([string]::IsNullOrWhiteSpace($PackageName)) {
    $PackageName = "hooshyaran-full-install-{0}" -f (Get-Date -Format "yyyyMMdd-HHmm")
}

$packageRoot = Join-Path $OutputRoot $PackageName
$publishFolder = Join-Path $packageRoot "publish"
$publishZipPath = Join-Path $packageRoot "hooshyaran-web-publish.zip"
$notesPath = Join-Path $packageRoot "deployment-notes.md"
$bundleZipPath = Join-Path $OutputRoot "$PackageName.zip"

if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}

if (Test-Path -LiteralPath $bundleZipPath) {
    Remove-Item -LiteralPath $bundleZipPath -Force
}

New-Item -ItemType Directory -Path $packageRoot | Out-Null

& .\scripts\smoke-production-publish.ps1 -ProjectPath $ProjectPath -BaseUrl $SmokeBaseUrl

dotnet publish $ProjectPath -c Release -o $publishFolder

if (Test-Path -LiteralPath (Join-Path $publishFolder "appsettings.Development.json")) {
    throw "appsettings.Development.json must not be present in the publish output."
}

Compress-Archive -Path (Join-Path $publishFolder "*") -DestinationPath $publishZipPath -Force
Remove-Item -LiteralPath $publishFolder -Recurse -Force

$defaultBackupPath = Invoke-SqlScalar "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000));"
if ([string]::IsNullOrWhiteSpace($defaultBackupPath)) {
    throw "Could not resolve SQL Server default backup path."
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmm"
$backupFileName = "$DatabaseName-full-$timestamp.bak"
$backupSourcePath = Join-Path $defaultBackupPath $backupFileName
$backupOutputPath = Join-Path $packageRoot $backupFileName

if (Test-Path -LiteralPath $backupSourcePath) {
    Remove-Item -LiteralPath $backupSourcePath -Force
}

$backupQuery = @"
BACKUP DATABASE [$DatabaseName]
TO DISK = N'$backupSourcePath'
WITH COPY_ONLY, INIT, CHECKSUM, COMPRESSION, STATS = 5;
"@

sqlcmd -S $SqlInstance -d master -E -C -Q $backupQuery
if ($LASTEXITCODE -ne 0) {
    throw "Database backup failed."
}

$verifyQuery = "RESTORE VERIFYONLY FROM DISK = N'$backupSourcePath';"
sqlcmd -S $SqlInstance -d master -E -C -Q $verifyQuery
if ($LASTEXITCODE -ne 0) {
    throw "Database backup verification failed."
}

Copy-Item -LiteralPath $backupSourcePath -Destination $backupOutputPath -Force

$logicalDataName = Invoke-SqlScalar "SET NOCOUNT ON; SELECT name FROM sys.master_files WHERE database_id = DB_ID(N'$DatabaseName') AND type_desc = 'ROWS';"
$logicalLogName = Invoke-SqlScalar "SET NOCOUNT ON; SELECT name FROM sys.master_files WHERE database_id = DB_ID(N'$DatabaseName') AND type_desc = 'LOG';"

$notesTemplate = @'
# بسته نصب کامل هوشیاران

تاریخ آماده‌سازی:

```text
__DATE__
```

## محتویات بسته

فایل برنامه:

```text
__PUBLISH_ZIP__
```

فایل بکاپ دیتابیس:

```text
__BACKUP_FILE__
```

فایل تجمیعی:

```text
__BUNDLE_ZIP__
```

## نتیجه بررسی محلی

اسکریپت smoke انتشار Production قبل از بسته‌سازی اجرا شد و موفق بود.

فایل `appsettings.Development.json` داخل publish وجود ندارد.

بکاپ دیتابیس با `COPY_ONLY` گرفته شد و `RESTORE VERIFYONLY` با موفقیت انجام شد.

## مراحل نصب روی سرور

1. سایت یا app pool در IIS را متوقف کنید.
2. از فولدر فعلی سایت و دیتابیس Production بکاپ بگیرید.
3. فایل `hooshyaran-web-publish.zip` را باز کنید و محتویات آن را روی فولدر سایت IIS کپی کنید.
4. فایل `appsettings.json` سرور را با مقادیر واقعی Production تنظیم کنید.
5. فایل بکاپ دیتابیس را روی سرور کپی کنید.
6. در SQL Server ابتدا نام فایل‌های منطقی را با دستور زیر بررسی کنید:

```sql
RESTORE FILELISTONLY
FROM DISK = N'C:\path\__BACKUP_FILENAME__';
```

7. سپس restore را با مسیرهای مناسب سرور انجام دهید:

```sql
RESTORE DATABASE [__DB_NAME__]
FROM DISK = N'C:\path\__BACKUP_FILENAME__'
WITH MOVE N'__LOGICAL_DATA__' TO N'D:\SQLData\__DB_NAME__.mdf',
     MOVE N'__LOGICAL_LOG__' TO N'D:\SQLLog\__DB_NAME___log.ldf',
     REPLACE,
     RECOVERY,
     STATS = 5;
```

8. اگر نام دیتابیس یا مسیر فایل‌ها روی سرور متفاوت است، connection string داخل `appsettings.json` را مطابق همان مقدار تنظیم کنید.
9. app pool را دوباره روشن کنید.

## چک بعد از نصب

این مسیرها باید پاسخ 200 بدهند:

```text
https://hooshyaran.ir/
https://hooshyaran.ir/products
https://hooshyaran.ir/blog
https://hooshyaran.ir/ai-integrator
https://hooshyaran.ir/request-demo
https://hooshyaran.ir/robots.txt
https://hooshyaran.ir/sitemap.xml
```
'@

$notes = (
    $notesTemplate.Replace('__DATE__', (Get-Date -Format "yyyy-MM-dd HH:mm")).
        Replace('__PUBLISH_ZIP__', $publishZipPath).
        Replace('__BACKUP_FILE__', $backupOutputPath).
        Replace('__BUNDLE_ZIP__', $bundleZipPath).
        Replace('__BACKUP_FILENAME__', $backupFileName).
        Replace('__DB_NAME__', $DatabaseName).
        Replace('__LOGICAL_DATA__', $logicalDataName).
        Replace('__LOGICAL_LOG__', $logicalLogName)
)

Set-Content -LiteralPath $notesPath -Value $notes -Encoding utf8

Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $bundleZipPath -Force

Write-Host "Full package folder: $packageRoot"
Write-Host "Publish zip: $publishZipPath"
Write-Host "Database backup: $backupOutputPath"
Write-Host "Bundle zip: $bundleZipPath"

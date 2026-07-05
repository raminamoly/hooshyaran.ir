param(
    [string]$ProjectPath = "src\Hooshyaran.Web\Hooshyaran.Web.csproj",
    [string]$OutputRoot = "C:\publish",
    [string]$PatchName = "",
    [string]$SqlPatch = "deploy\20260705-production-seo-image-patch.sql"
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($PatchName)) {
    $PatchName = "hooshyaran-server-patch-{0}" -f (Get-Date -Format "yyyyMMdd-HHmm")
}

$publishPath = Join-Path $OutputRoot $PatchName
$zipPath = Join-Path $OutputRoot "$PatchName.zip"
$sqlOutputPath = Join-Path $OutputRoot "$PatchName-sql.sql"

if (Test-Path -LiteralPath $publishPath) {
    Remove-Item -LiteralPath $publishPath -Recurse -Force
}

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

dotnet publish $ProjectPath -c Release -o $publishPath

if (Test-Path -LiteralPath (Join-Path $publishPath "appsettings.Development.json")) {
    throw "appsettings.Development.json must not be present in the server patch."
}

Compress-Archive -Path (Join-Path $publishPath "*") -DestinationPath $zipPath -Force

if (Test-Path -LiteralPath $SqlPatch) {
    Copy-Item -LiteralPath $SqlPatch -Destination $sqlOutputPath -Force
}

Write-Host "Patch package: $zipPath"
if (Test-Path -LiteralPath $sqlOutputPath) {
    Write-Host "SQL patch: $sqlOutputPath"
}

param(
    [string]$ProjectPath = "src\Hooshyaran.Web\Hooshyaran.Web.csproj",
    [string]$PublishPath = "C:\publish\hooshyaran-smoke",
    [string]$BaseUrl = "http://127.0.0.1:5145",
    [string]$ConnectionString = $env:ConnectionStrings__HooshyaranDb
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = "Server=.\SQL2019;Database=HooshyaranDev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw $Message
    }
}

dotnet build -c Release

if (Test-Path -LiteralPath $PublishPath) {
    Remove-Item -LiteralPath $PublishPath -Recurse -Force
}

dotnet publish $ProjectPath -c Release -o $PublishPath

Assert-True -Condition (-not (Test-Path -LiteralPath (Join-Path $PublishPath "appsettings.Development.json"))) `
    -Message "appsettings.Development.json must not be published."

$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__HooshyaranDb = $ConnectionString

$process = Start-Process -FilePath "dotnet" `
    -ArgumentList @("Hooshyaran.Web.dll", "--urls", $BaseUrl) `
    -WorkingDirectory $PublishPath `
    -NoNewWindow `
    -PassThru

try {
    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Milliseconds 500
        try {
            $response = Invoke-WebRequest -Uri "$BaseUrl/" -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                $ready = $true
                break
            }
        }
        catch {
            if ($process.HasExited) {
                throw "Application exited before smoke test could start."
            }
        }
    }

    Assert-True -Condition $ready -Message "Application did not become ready."

    $paths = @(
        "/",
        "/products",
        "/blog",
        "/ai-integrator",
        "/request-demo",
        "/robots.txt",
        "/sitemap.xml"
    )

    foreach ($path in $paths) {
        $response = Invoke-WebRequest -Uri "$BaseUrl$path" -UseBasicParsing -TimeoutSec 15
        Assert-True -Condition ($response.StatusCode -eq 200) -Message "$path returned $($response.StatusCode)."
    }

    $homeResponse = Invoke-WebRequest -Uri "$BaseUrl/" -UseBasicParsing -TimeoutSec 15
    Assert-True -Condition ($homeResponse.Headers["Content-Security-Policy"].Count -gt 0) -Message "CSP header is missing."
    Assert-True -Condition ($homeResponse.Headers["X-Content-Type-Options"] -contains "nosniff") -Message "X-Content-Type-Options header is missing."
    Assert-True -Condition ($homeResponse.Content -match 'property="og:image"') -Message "og:image is missing."
    Assert-True -Condition ($homeResponse.Content -match 'name="twitter:card"') -Message "twitter:card is missing."
    Assert-True -Condition ($homeResponse.Content -match 'application/ld' -and $homeResponse.Content -match 'schema.org') -Message "JSON-LD is missing."

    $assetMatches = [regex]::Matches($homeResponse.Content, '(?:src|href)="([^"]+\.(?:css|js|jpg|jpeg|png|webp|woff2|ico)(?:\?[^"]*)?)"', 'IgnoreCase')
    foreach ($match in $assetMatches) {
        $assetUrl = $match.Groups[1].Value.Replace("&amp;", "&")
        if ($assetUrl.StartsWith("http", [StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        $assetResponse = Invoke-WebRequest -Uri "$BaseUrl$assetUrl" -UseBasicParsing -TimeoutSec 15
        Assert-True -Condition ($assetResponse.StatusCode -eq 200) -Message "Asset failed: $assetUrl"
    }

    Write-Host "Smoke test passed for $PublishPath"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}

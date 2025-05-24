param (
    [string]$ProjectPath,
    [string]$env
)

if (-not $env) {
    $env = "Development"
}

$AppSettingsPath = "${ProjectPath}/appsettings.${env}.json"

if (-Not (Test-Path $AppSettingsPath)) {
    Write-Host "Missing ${env} appsettings file: ${AppSettingsPath}"
    exit 1
}

$JsonContent = Get-Content -Raw $AppSettingsPath | ConvertFrom-Json
$ConnectionString = $JsonContent.Database.ConnectionString

if (-not $ConnectionString) {
    Write-Host "Missing value 'Database:ConnectionString' in $AppSettingsPath"
    exit 1
}

return $ConnectionString

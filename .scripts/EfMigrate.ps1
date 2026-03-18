#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("digital", "cms")]
    [string]$Context,

    [Parameter(Mandatory = $true, Position = 1)]
    [string]$MigrationName
)

$ErrorActionPreference = "Stop"

$contextMap = @{
    "digital" = "Digital.Net.Core.Entities"
    "cms"     = "Digital.Net.Cms"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$appSettingsPath = Join-Path $rootDir "Digital.Net.Tests.Program" "appsettings.Development.json"
$entityProject = Join-Path $rootDir $contextMap[$Context]
$startupProject = Join-Path $rootDir "Digital.Net.Tests.Program"

if (-not (Test-Path $appSettingsPath)) {
    Write-Error "appsettings.Development.json not found at: $appSettingsPath"
    exit 1
}

$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
$connectionString = $appSettings.Database.ConnectionString

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    Write-Error "Database:ConnectionString is empty in appsettings.Development.json"
    exit 1
}

Write-Host "Creating migration '$MigrationName' for context '$Context'..." -ForegroundColor Cyan
Write-Host "Project: $($contextMap[$Context])" -ForegroundColor DarkGray
Write-Host "Connection: $connectionString" -ForegroundColor DarkGray

dotnet ef migrations add $MigrationName `
    --project $entityProject `
    --startup-project $startupProject `
    -- $connectionString

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration '$MigrationName' created successfully." -ForegroundColor Green
} else {
    Write-Error "Migration creation failed."
    exit $LASTEXITCODE
}

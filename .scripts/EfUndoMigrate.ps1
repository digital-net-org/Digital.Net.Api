#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$appSettingsPath = Join-Path $rootDir "Digital.Net.Tests.Program" "appsettings.Development.json"
$entitiesProject = Join-Path $rootDir "Digital.Net.Entities"
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

Write-Host "Reverting last migration..." -ForegroundColor Cyan
Write-Host "Connection: $connectionString" -ForegroundColor DarkGray

dotnet ef migrations remove `
    --project $entitiesProject `
    --startup-project $startupProject `
    -- $connectionString

if ($LASTEXITCODE -eq 0) {
    Write-Host "Last migration reverted successfully." -ForegroundColor Green
} else {
    Write-Error "Migration revert failed."
    exit $LASTEXITCODE
}

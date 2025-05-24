param (
    [string]$ConnectionString
)

$MigrationName = Read-Host "Enter migration name"
$Project = "$PSScriptRoot/../Digital.Net.Api.Entities"
dotnet ef migrations add $MigrationName --project $Project --context "DigitalContext" -- $ConnectionString

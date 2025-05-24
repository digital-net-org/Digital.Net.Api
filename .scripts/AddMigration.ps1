$DigitalScriptsPath = "${PSScriptRoot}/../Digital.Lib/.scripts"
$ConnectionString = & "${DigitalScriptsPath}/GetConnectionString.ps1" -ProjectPath "../Digital.Core.Api" -env "Development"
& "${DigitalScriptsPath}/AddMigration.ps1" $ConnectionString

## TODO

param (
[string]$ConnectionString
)

$MigrationName = Read-Host "Enter migration name"
$Project = "$PSScriptRoot/../Digital.Lib.Net.Entities"
dotnet ef migrations add $MigrationName --project $Project --context "DigitalContext" -- $ConnectionString

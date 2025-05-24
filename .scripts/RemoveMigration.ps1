$DigitalScriptsPath = "${PSScriptRoot}/../Digital.Lib/.scripts"
$ConnectionString = & "${DigitalScriptsPath}/GetConnectionString.ps1" -ProjectPath "../Digital.Core.Api" -env "Development"
& "${DigitalScriptsPath}/RemoveMigration.ps1" $ConnectionString

## TODO

param (
[string]$ConnectionString
)

$Project = "$PSScriptRoot/../Digital.Lib.Net.Entities"
dotnet ef migrations remove --project $Project -- $ConnectionString

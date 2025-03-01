$DigitalScriptsPath = "${PSScriptRoot}/../Digital.Lib/.scripts"
$ConnectionString = & "${DigitalScriptsPath}/GetConnectionString.ps1" -ProjectPath "../Digital.Core.Api" -env "Development"
& "${DigitalScriptsPath}/AddMigration.ps1" $ConnectionString

$ConnectionString = & "${PSScriptRoot}/GetConnectionString.ps1" -ProjectPath "../Digital.Net.Api.Rest" -env "Development"
& "${DigitalScriptsPath}/AddMigration.ps1" $ConnectionString

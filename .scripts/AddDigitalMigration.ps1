$ConnectionString = & "${PSScriptRoot}/GetConnectionString.ps1" -ProjectPath "../Digital.Net.Api.Rest" -env "Development"
& "${PSScriptRoot}/AddMigration.ps1" $ConnectionString

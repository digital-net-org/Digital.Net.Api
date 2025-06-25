param (
[string]$ConnectionString
)

$Project = "$PSScriptRoot/../Digital.Net.Api.Entities"
dotnet ef migrations remove --project $Project -- $ConnectionString

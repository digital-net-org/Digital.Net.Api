param (
[string]$ConnectionString
)

$Project = "$PSScriptRoot/../Digital.Lib.Net.Entities"
dotnet ef migrations remove --project $Project -- $ConnectionString

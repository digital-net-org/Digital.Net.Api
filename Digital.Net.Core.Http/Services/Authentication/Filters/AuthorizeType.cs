namespace Digital.Net.Core.Http.Services.Authentication.Filters;

[Flags]
public enum AuthorizeType
{
    ApiKey = 1,
    Jwt = 2,
    Application = 4
}
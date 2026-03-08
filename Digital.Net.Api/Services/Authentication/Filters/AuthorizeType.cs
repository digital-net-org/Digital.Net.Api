namespace Digital.Net.Api.Services.Authentication.Filters;

[Flags]
public enum AuthorizeType
{
    ApiKey = 1,
    Jwt = 2,
    Any = ApiKey | Jwt
}
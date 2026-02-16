namespace Digital.Net.Api.Authentication.Attributes;

[Flags]
public enum AuthorizeType
{
    ApiKey = 1,
    Jwt = 2,
    Any = ApiKey | Jwt
}
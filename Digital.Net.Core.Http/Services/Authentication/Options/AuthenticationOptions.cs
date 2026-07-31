using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Http.Services.Authentication.Options;

public class AuthenticationOptions
{
    public required SameSiteMode CookieSameSite { get; set; }
}

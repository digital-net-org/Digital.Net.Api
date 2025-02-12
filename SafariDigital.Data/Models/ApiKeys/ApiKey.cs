using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Authentication.Models.Authorizations;
using Digital.Lib.Net.Authentication.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace SafariDigital.Data.Models.ApiKeys;

[Table("ApiKey"), Index(nameof(Key), IsUnique = true)]
public class ApiKey : AuthorizationApiKey
{
    public ApiKey()
    {
    }

    public ApiKey(Guid userId)
    {
        ApiUserId = userId;
    }

    public ApiKey(Guid userId, string key)
    {
        ApiUserId = userId;
        SetKey(key);
    }

    public void SetKey(string key) => Key = HashService.HashApiKey(key);
}
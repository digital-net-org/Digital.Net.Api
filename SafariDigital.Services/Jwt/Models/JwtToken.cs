using System.Text.Json.Serialization;
using Digital.Net.Core.Messages;
using Microsoft.IdentityModel.Tokens;

namespace SafariDigital.Services.Jwt.Models;

public class JwtToken<T> : Result<T> where T : class
{
    [JsonConstructor]
    public JwtToken()
    {
    }

    public string? Token { get; set; }
    public SecurityToken? SecurityToken { get; set; }
    public JwtToken<T> AddError(Exception e)
    {
        Errors.Add(new ResultMessage(e));
        return this;
    }
}
using Microsoft.IdentityModel.Tokens;
using SafariDigital.Core.Validation;

namespace SafariDigital.Services.Jwt.Models;

public class JwtToken<T> : Result<T>
{
    public string? Token { get; set; }
    public SecurityToken? SecurityToken { get; set; }
    public T? Content { get; set; }

    public JwtToken<T> AddError(Exception e)
    {
        Errors.Add(new ResultMessage(e));
        return this;
    }
}
using Digital.Net.Api.Core.Messages;

namespace Digital.Net.Api.Services.Authentication.Models;

public class AuthorizationResult : Result
{
    public Guid UserId { get; set; } = Guid.Empty;
    public bool IsForbidden { get; set; }
    public bool IsAuthorized { get; set; }

    public void Forbid() => IsForbidden = true;

    public void Authorize(Guid userId)
    {
        IsAuthorized = true;
        UserId = userId;
        ClearErrors();
    }

    public new AuthorizationResult Merge(Result result)
    {
        base.Merge(result);
        if (result is AuthorizationResult authResult)
        {
            IsAuthorized = authResult.IsAuthorized;
            IsForbidden = authResult.IsForbidden;
            UserId = authResult.UserId != Guid.Empty ? authResult.UserId : UserId;
        }
        return this;
    }

    public AuthorizationResult AddError(Exception e)
    {
        base.AddError(e);
        return this;
    }
}

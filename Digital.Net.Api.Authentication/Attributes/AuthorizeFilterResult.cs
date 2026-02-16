using Digital.Net.Api.Core.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Digital.Net.Api.Authentication.Attributes;

public static class AuthorizeFilterResult
{
    public static Result RejectAuthorization(this AuthorizationFilterContext context, int statusCode)
    {
        var result = new Result();
        result.AddError(new UnauthorizedAccessException());
        context.Result = new JsonResult(result) { StatusCode = statusCode };
        return result;
    }
}
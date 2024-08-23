using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Repositories;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAuthorizationFilter
{
    public const string ApiKeyHeaderName = "X-API-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var result = new Result();
        var repository = context.HttpContext.RequestServices.GetRequiredService<IRepository<ApiKey>>();
        var headerKey = context.HttpContext.Request.Headers[ApiKeyHeaderName].ToString();
        var apiKey = repository.Get(k => k.Key == headerKey).FirstOrDefault();

        if (
            string.IsNullOrWhiteSpace(headerKey)
            || apiKey is null
            || (apiKey.ExpiredAt is not null && apiKey.ExpiredAt < DateTime.UtcNow)
        )
            result.AddError(new UnauthorizedAccessException());
        else return;

        context.Result = new JsonResult(result) { StatusCode = 401 };
    }
}
using Digital.Net.Core.Endpoints.Dto;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Core.Services.ApiKeys;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Core.Endpoints;

public static class ApiKeyEndpoints
{
    public static IEndpointRouteBuilder MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/user/self/api-key")
            .WithTags("User")
            .RequireRateLimiting(GlobalLimiter.Policy)
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        group
            .MapPost("", Create)
            .WithSummary("CreateApiKey")
            .WithDescription("Generates a new API key. The plaintext key is returned only once.");

        group
            .MapGet("", List)
            .WithSummary("ListApiKeys")
            .WithDescription("Lists all API keys for the authenticated user (metadata only).");

        group
            .MapDelete("/{id:guid}", Delete)
            .WithSummary("DeleteApiKey")
            .WithDescription("Deletes a specific API key.");

        return app;
    }

    public static async Task<Results<Ok<Result<string>>, BadRequest<Result<string>>>> Create(
        [FromBody] ApiKeyCreatePayload payload,
        ApiKeyService apiKeyService,
        IUserContextService userContextService
    )
    {
        var userId = userContextService.GetUserId();
        var result = await apiKeyService.CreateAsync(userId, payload.Name, payload.ExpiredAt);
        return result.HasError
            ? TypedResults.BadRequest(result)
            : TypedResults.Ok(result);
    }

    public static async Task<Ok<Result<List<ApiKeyDto>>>> List(
        ApiKeyService apiKeyService,
        IUserContextService userContextService
    )
    {
        var userId = userContextService.GetUserId();
        var result = await apiKeyService.GetByUserAsync(userId);
        var dtoResult = new Result<List<ApiKeyDto>>
        {
            Value = result.Value?.Select(k => new ApiKeyDto(k)).ToList() ?? []
        };
        return TypedResults.Ok(dtoResult);
    }

    public static async Task<Results<Ok<Result>, NotFound<Result>>> Delete(
        Guid id,
        ApiKeyService apiKeyService,
        IUserContextService userContextService
    )
    {
        var userId = userContextService.GetUserId();
        var result = await apiKeyService.DeleteAsync(userId, id);
        return result.HasErrorOfType<KeyNotFoundException>()
            ? TypedResults.NotFound(result)
            : TypedResults.Ok(result);
    }
}

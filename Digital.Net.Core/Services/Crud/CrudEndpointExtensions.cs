using System.Text.Json;
using Digital.Net.Core.Entities.Exceptions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Services.Crud;

public static class CrudEndpointExtensions
{
    public static RouteHandlerBuilder MapCrudSchema<TContext, T>(
        this IEndpointRouteBuilder app,
        string? route = null
    )
        where TContext : DbContext
        where T : class, IEntity
    {
        route = string.IsNullOrWhiteSpace(route) ? "" : $"{route}/";
        return app
            .MapGet($"{route}schema", () =>
            {
                var result = new Result<List<SchemaProperty<T>>>(SchemaProperty<T>.Get());
                return TypedResults.Ok(result);
            })
            .WithSummary($"GetSchema: {typeof(T).Name}")
            .WithDescription(
                $"Get the schema for the {typeof(T).Name}. " +
                $"The schema includes all properties, their types, accessibility, and validation rules."
            );
    }

    public static RouteHandlerBuilder MapCrudGet<TContext, T, TDto>(
        this IEndpointRouteBuilder app,
        string? route = null
    )
        where TContext : DbContext
        where T : Entity
        where TDto : class
    {
        route = string.IsNullOrWhiteSpace(route) ? "" : $"{route}/";
        return app
            .MapGet(
                $"{route}{{id:guid}}",
                async Task<Results<Ok<Result<TDto>>, NotFound<Result<TDto>>, InternalServerError<Result<TDto>>>> (
                    Guid id,
                    TContext context,
                    IEnumerable<IDtoEnricher<T, TDto>> enrichers,
                    CancellationToken ct
                ) =>
                {
                    var result = new Result<TDto>();
                    try
                    {
                        var entity = await context.Set<T>().FindAsync([id], ct) ??
                                     throw new ResourceNotFoundException();

                        var dto = Mapper.TryMap<T, TDto>(entity);
                        foreach (var enricher in enrichers)
                            await enricher.EnrichAsync(entity, dto, ct);

                        result.Value = dto;
                        return TypedResults.Ok(result);
                    }
                    catch (ResourceNotFoundException)
                    {
                        return TypedResults.NotFound(result);
                    }
                    catch (Exception ex)
                    {
                        return TypedResults.InternalServerError(result.AddError(ex));
                    }
                })
            .WithSummary($"GetById: {typeof(T).Name}")
            .WithDescription(
                $"Retrieves a single {typeof(T).Name} by its ID. Returns the entity as the specified DTO type."
            );
    }

    public static RouteHandlerBuilder MapCrudPost<TContext, T, TPayload>(
        this IEndpointRouteBuilder app,
        string? route = null,
        string? eventType = null
    )
        where TContext : DbContext
        where T : Entity
        where TPayload : class
    {
        route = string.IsNullOrWhiteSpace(route) ? "" : $"{route}";
        return app
            .MapPost(
                $"{route}",
                async Task<Results<Ok<Result<Guid>>, BadRequest<Result<Guid>>, InternalServerError<Result<Guid>>>> (
                    [FromBody]
                    TPayload payload,
                    CrudService<TContext, T> crudService,
                    IAuditService auditService,
                    UserContextService userContextService
                ) =>
                {
                    var result = await crudService.Create(Mapper.TryMap<TPayload, T>(payload));
                    if (result.HasError && !result.HasErrorOfType<EntityValidationException>())
                        return TypedResults.InternalServerError(result);

                    if (eventType is not null)
                        await auditService.RegisterEventAsync(
                            eventType,
                            result.HasError ? EventState.Failed : EventState.Success,
                            result,
                            userContextService.GetUserId()
                        );

                    return result.HasError
                        ? TypedResults.BadRequest(result)
                        : TypedResults.Ok(result);
                })
            .WithSummary($"Create: {typeof(T).Name}")
            .WithDescription(
                $"Creates a new {typeof(T).Name} with the provided payload. " +
                $"Returns the created {typeof(T).Name} as the specified DTO type."
            );
    }
    
    public static RouteHandlerBuilder MapCrudPatch<TContext, T>(
        this IEndpointRouteBuilder app,
        string? route = null,
        string? eventType = null
    )
        where TContext : DbContext
        where T : Entity
    {
    route = string.IsNullOrWhiteSpace(route) ? "" : $"{route}/";
    return app
            .MapPatch(
                $"{route}{{id:guid}}",
                async Task<Results<Ok<Result>, NotFound<Result>, BadRequest<Result>, InternalServerError<Result>>> (
                Guid id,
                [FromBody]
                JsonElement patch,
                CrudService<TContext, T> crudService,
                IAuditService auditService,
                UserContextService userContextService,
                CancellationToken ct
            ) =>
            {
                var result = await crudService.Patch(patch, id, ct);
                var isBadRequest = result.HasErrorOfType<EntityValidationException>() ||
                                   result.HasErrorOfType<InvalidOperationException>();
                
                if (result.HasErrorOfType<ResourceNotFoundException>())
                    return TypedResults.NotFound(result);
                if (result.HasError && !isBadRequest)
                    return TypedResults.InternalServerError(result);
                
                if (eventType is not null)
                    await auditService.RegisterEventAsync(
                        eventType,
                        result.HasError ? EventState.Failed : EventState.Success,
                        result,
                        userContextService.GetUserId()
                    );
                
                return result.HasError
                    ? TypedResults.BadRequest(result)
                    : TypedResults.Ok(result);
            })
            .WithSummary($"Patch: {typeof(T).Name}")
            .WithDescription(
                $"Applies a JSON Patch to a {typeof(T).Name} identified by its ID. " +
                $"Returns the patched {typeof(T).Name} as the specified DTO type. " +
                $"Use the *Schema* endpoint to get the available fields."
            );
    }
    
    public static RouteHandlerBuilder MapCrudDelete<TContext, T>(
        this IEndpointRouteBuilder app,
        string? route = null,
        string? eventType = null
    )
        where TContext : DbContext
        where T : Entity
    {
        route = string.IsNullOrWhiteSpace(route) ? "" : $"{route}/";
        return app
            .MapDelete(
                $"{route}{{id:guid}}",
                async Task<Results<Ok<Result>, NotFound<Result>, InternalServerError<Result>>> (
                    Guid id,
                    CrudService<TContext, T> crudService,
                    IAuditService auditService,
                    UserContextService userContextService
                ) =>
                {
                    var result = await crudService.Delete(id);
                    if (result.HasErrorOfType<ResourceNotFoundException>())
                        return TypedResults.NotFound(result);
                    if (result.HasError)
                        return TypedResults.InternalServerError(result);

                    if (eventType is not null)
                        await auditService.RegisterEventAsync(
                            eventType, // TODO: Handle authorization VS Server errors
                            result.HasError ? EventState.Failed : EventState.Success,
                            result,
                            userContextService.GetUserId()
                        );

                    return TypedResults.Ok(result);
                }
            )
            .WithSummary($"Delete: {typeof(T).Name}")
            .WithDescription(
                $"Removes an {typeof(T).Name} identified by its ID. " +
                $"Returns the deleted {typeof(T).Name} as the specified DTO type."
            );
    }
}
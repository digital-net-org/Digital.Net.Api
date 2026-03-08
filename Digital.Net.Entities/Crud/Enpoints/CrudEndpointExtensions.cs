using System.Text.Json;
using Digital.Net.Core.Exceptions.types;
using Digital.Net.Core.Formatters;
using Digital.Net.Core.Messages;
using Digital.Net.Core.Models;
using Digital.Net.Entities.Exceptions;
using Digital.Net.Entities.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Entities.Crud.Enpoints;

/// <summary>
///     Extension methods to map CRUD endpoints for entities to Minimal API routes.
///     Each method maps a single CRUD operation and returns a RouteHandlerBuilder for further configuration.
/// </summary>
public static class CrudEndpointExtensions
{
    /// <summary>
    ///     Maps a GET endpoint to retrieve the entity schema.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="route">The base route for the CRUD operations</param>
    /// <returns>A RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapCrudSchema<T>(
        this IEndpointRouteBuilder app,
        string route
    )
        where T : Entity =>
        app
            .MapGet($"{route}/schema", (
                ICrudValidationService crudValidationService
            ) =>
            {
                var result = new Result<List<SchemaProperty<T>>>(crudValidationService.GetSchema<T>());
                return TypedResults.Ok(result);
            })
            .WithSummary("GetSchema")
            .WithDescription("Get the schema for the entity. The schema includes all properties, their types, accessibility, and validation rules.");

    /// <summary>
    ///     Maps a GET endpoint to retrieve an entity by its ID.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TDto">The DTO type to return</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="route">The base route for the CRUD operations</param>
    /// <returns>A RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapCrudGet<T, TDto>(
        this IEndpointRouteBuilder app,
        string route
    )
        where T : Entity
        where TDto : class =>
        app
            .MapGet($"{route}/{{id:guid}}", (
                Guid id,
                ICrudService<T> crudService
            ) =>
            {
                var result = crudService.Get<TDto>(id);
                return result.HasError
                    ? (Results<Ok<Result<TDto>>, NotFound<Result<TDto>>>)TypedResults.NotFound(result)
                    : (Results<Ok<Result<TDto>>, NotFound<Result<TDto>>>)TypedResults.Ok(result);
            })
            .WithSummary("GetById")
            .WithDescription("Retrieves a single entity by its ID. Returns the entity as the specified DTO type.");

    /// <summary>
    ///     Maps a POST endpoint to create a new entity.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TPayload">The payload type for creation</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="route">The base route for the CRUD operations</param>
    /// <returns>A RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapCrudPost<T, TPayload>(
        this IEndpointRouteBuilder app,
        string route
    )
        where T : Entity
        where TPayload : class =>
        app
            .MapPost($"{route}", async (
                [FromBody]
                TPayload payload,
                ICrudService<T> crudService
            ) =>
            {
                var result = await crudService.Create(Mapper.Map<TPayload, T>(payload));
                return result.HasError
                    ? (Results<Ok<Result<Guid>>, BadRequest<Result<Guid>>>)TypedResults.BadRequest(result)
                    : (Results<Ok<Result<Guid>>, BadRequest<Result<Guid>>>)TypedResults.Ok(result);
            })
            .WithSummary("Create")
            .WithDescription("Creates a new entity with the provided payload. Returns the created entity as the specified DTO type.");

    /// <summary>
    ///     Maps a PATCH endpoint to partially update an entity.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="route">The base route for the CRUD operations</param>
    /// <returns>A RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapCrudPatch<T>(
        this IEndpointRouteBuilder app,
        string route
    )
        where T : Entity =>
        app
            .MapPatch($"{route}/{{id:guid}}", async (
                Guid id,
                [FromBody]
                JsonElement patch,
                ICrudService<T> crudService
            ) =>
            {
                var result = await crudService.Patch(patch.GetPatchDocument<T>(), id);

                if (result.HasError && result.HasErrorOfType<ResourceNotFoundException>())
                    return (Results<Ok<Result>, NotFound<Result>, BadRequest<Result>, InternalServerError<Result>>)
                        TypedResults.NotFound(result);

                if (result.HasError && (
                        result.HasErrorOfType<InvalidOperationException>()
                        || result.HasErrorOfType<EntityValidationException>()
                    ))
                    return (Results<Ok<Result>, NotFound<Result>, BadRequest<Result>, InternalServerError<Result>>)
                        TypedResults.BadRequest(result);

                if (result.HasError)
                    return (Results<Ok<Result>, NotFound<Result>, BadRequest<Result>, InternalServerError<Result>>)
                        TypedResults.InternalServerError(result);

                return (Results<Ok<Result>, NotFound<Result>, BadRequest<Result>, InternalServerError<Result>>)
                    TypedResults.Ok(result);
            })
            .WithSummary("Patch")
            .WithDescription("Applies a JSON Patch to an entity identified by its ID. Returns the patched entity as the specified DTO type. Use the *Schema* endpoint to get the available fields.");

    /// <summary>
    ///     Maps a DELETE endpoint to remove an entity.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="route">The base route for the CRUD operations</param>
    /// <returns>A RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapCrudDelete<T>(
        this IEndpointRouteBuilder app,
        string route
    )
        where T : Entity =>
        app
            .MapDelete($"{route}/{{id:guid}}", async (
                Guid id,
                ICrudService<T> crudService
            ) =>
            {
                var result = await crudService.Delete(id);
                return result.HasError
                    ? (Results<Ok<Result>, NotFound<Result>>)TypedResults.NotFound(result)
                    : (Results<Ok<Result>, NotFound<Result>>)TypedResults.Ok(result);
            })
            .WithSummary("Delete")
            .WithDescription("Removes an entity identified by its ID. Returns the deleted entity as the specified DTO type.");
}
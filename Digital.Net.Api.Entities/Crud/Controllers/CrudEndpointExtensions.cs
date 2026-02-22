using System.Text.Json;
using Digital.Net.Api.Core.Exceptions.types;
using Digital.Net.Api.Core.Formatters;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Core.OpenApi;
using Digital.Net.Api.Entities.Exceptions;
using Digital.Net.Api.Entities.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Entities.Crud.Controllers;

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
                return Results.Ok(result);
            })
            .WithDoc(d =>
            {
                d.Summary = "GetSchema";
                d.Description =
                    "Get the schema for the entity. The schema includes all properties, their types, accessibility, and validation rules.";
            });

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
                return result.HasError ? Results.NotFound(result) : Results.Ok(result);
            })
            .WithDoc(d =>
            {
                d.Summary = "GetById";
                d.Description =
                    "Retrieves a single entity by its ID. Returns the entity as the specified DTO type.";
            });

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
                return result.HasError ? Results.BadRequest(result) : Results.Ok(result);
            }).WithDoc(d =>
            {
                d.Summary = "Create";
                d.Description =
                    "Creates a new entity with the provided payload. Returns the created entity as the specified DTO type.";
            });

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
                var result = await crudService.Patch(JsonFormatter.GetPatchDocument<T>(patch), id);

                if (result.HasError && result.HasErrorOfType<ResourceNotFoundException>())
                    return Results.NotFound(result);

                if (result.HasError && (
                        result.HasErrorOfType<InvalidOperationException>()
                        || result.HasErrorOfType<EntityValidationException>()
                    ))
                    return Results.BadRequest(result);

                if (result.HasError)
                    return Results.InternalServerError(result);

                return Results.Ok(result);
            })
            .WithDoc(d =>
            {
                d.Summary = "Patch";
                d.Description =
                    "Applies a JSON Patch to an entity identified by its ID. Returns the patched entity as the specified DTO type. Use the *Schema* endpoint to get the available fields.";
            });

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
                return result.HasError ? Results.NotFound(result) : Results.Ok(result);
            })
            .WithDoc(d =>
            {
                d.Summary = "Delete";
                d.Description =
                    "Removes an entity identified by its ID. Returns the deleted entity as the specified DTO type.";
            });
}
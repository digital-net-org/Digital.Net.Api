using Digital.Net.Api.Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Services;

public interface IEntityValidator<TContext> where TContext : DbContext
{
    /// <summary>
    ///     Get a schema of the entity describing its properties.
    /// </summary>
    /// <typeparam name="T">The model of the entity</typeparam>
    /// <returns>Schema of the entity</returns>
    List<SchemaProperty<T>> GetSchema<T>() where T : Entity;

    /// <summary>
    ///     Validate a patch payload against the entity schema.
    /// </summary>
    /// <param name="patch">The patch document to validate</param>
    /// <typeparam name="T">The model of the entity</typeparam>
    /// <returns>Validation result</returns>
    public void ValidatePatchPayload<T>(JsonPatchDocument<T> patch) where T : Entity;

    /// <summary>
    ///     Validate a create payload against the entity schema.
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <typeparam name="T">The model of the entity</typeparam>
    /// <returns>Validation result</returns>
    public void ValidateCreatePayload<T>(T entity) where T : Entity;

    public void ValidateProperty<T>(object? value, string path, SchemaProperty<T>? property) where T : Entity;
}
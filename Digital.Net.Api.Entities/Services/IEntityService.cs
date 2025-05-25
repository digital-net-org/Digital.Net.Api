using System.Linq.Expressions;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Services;

public interface IEntityService<T, TContext>
    where T : Entity
    where TContext : DbContext
{
    /// <summary>
    ///     Get a schema of the entity describing its properties.
    /// </summary>
    /// <typeparam name="T">The model of the entity</typeparam>
    /// <returns>Schema of the entity</returns>
    List<SchemaProperty<T>> GetSchema();

    /// <summary>
    ///    Get an entity based on a predicate. Converts the entity to the provided model using constructor.
    /// </summary>
    /// <param name="expression">The predicate to filter entities</param>
    /// <typeparam name="TModel">The model to convert the entities to</typeparam>
    /// <returns>Result of the model</returns>
    Result<TModel> GetFirst<TModel>(Expression<Func<T, bool>> expression)
        where TModel : class;
    
    /// <summary>
    ///    Get an entity based on its primary key. Converts the entity to the provided model using constructor.
    /// </summary>
    /// <param name="id">The entity primary key</param>
    /// <typeparam name="TModel">The model to convert the entities to</typeparam>
    /// <returns>Result of the model</returns>
    Result<TModel> Get<TModel>(Guid? id)
        where TModel : class;

    /// <summary>
    ///    Get an entity based on its primary key. Converts the entity to the provided model using constructor.
    /// </summary>
    /// <param name="id">The entity primary key</param>
    /// <typeparam name="TModel">The model to convert the entities to</typeparam>
    /// <returns>Result of the model</returns>
    Result<TModel> Get<TModel>(int id)
        where TModel : class;

    /// <summary>
    ///     Patch an entity based on its primary key.
    /// </summary>
    /// <param name="patch">The patch body</param>
    /// <param name="id">The entity primary key</param>
    /// <returns>Result of the model</returns>
    /// <exception cref="KeyNotFoundException">
    ///     If the entity is not found, throws an exceptions.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If the patch is invalid, throws an exceptions.
    /// </exception>
    Task<Result> Patch(JsonPatchDocument<T> patch, Guid? id);

    /// <summary>
    ///     Patch an entity based on its primary key.
    /// </summary>
    /// <param name="patch">The patch body</param>
    /// <param name="id">The entity primary key</param>
    /// <returns>Result of the model</returns>
    /// <exception cref="KeyNotFoundException">
    ///     If the entity is not found, throws an exceptions.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If the patch is invalid, throws an exceptions.
    /// </exception>
    Task<Result> Patch(JsonPatchDocument<T> patch, int id);

    /// <summary>
    ///     Create a new entity. Converts the payload to the entity using fields and properties mapping.
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <returns>Result of the model</returns>
    /// <exception cref="InvalidOperationException">
    ///     If the payload is invalid, throws an exceptions.
    /// </exception>
    Task<Result> Create(T entity);

    /// <summary>
    ///     Delete an entity based on its primary key.
    /// </summary>
    /// <param name="id">The entity primary key</param>
    /// <returns>Result of the model</returns>
    /// <exception cref="KeyNotFoundException">
    ///     If the entity is not found, throws an exceptions.
    /// </exception>
    Task<Result> Delete(Guid? id);

    /// <summary>
    ///     Delete an entity based on its primary key.
    /// </summary>
    /// <param name="id">The entity primary key</param>
    /// <returns>Result of the model</returns>
    /// <exception cref="KeyNotFoundException">
    ///     If the entity is not found, throws an exceptions.
    /// </exception>
    Task<Result> Delete(int id);
}

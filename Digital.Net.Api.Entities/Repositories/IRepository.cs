using System.Linq.Expressions;
using Digital.Net.Api.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Repositories;

/// <summary>
///     Entity Repository. Use this class for database interactions.
/// </summary>
/// <typeparam name="T">The entity type. (Must inherit from EntityBase)</typeparam>
/// <typeparam name="TContext">The entity DbContext.</typeparam>
public interface IRepository<T, TContext>
    where T : Entity
    where TContext : DbContext
{
    /// <summary>
    ///     Reload the entity.
    /// </summary>
    /// <param name="entity"></param>
    void Reload(T entity);

    /// <summary>
    ///     Reload the entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task ReloadAsync(T entity);
    /// <summary>
    ///     Create a new entity
    /// </summary>
    /// <param name="entity">The entity to create</param>
    public void Create(T entity);

    /// <summary>
    ///     Create a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to create</param>
    public Task CreateAsync(T entity);

    /// <summary>
    ///     Create a new entity and save it directly.
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <returns>The created entity</returns>
    public T CreateAndSave(T entity);

    /// <summary>
    ///     Create a new entity asynchronously and save it directly.
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <returns>The created entity</returns>
    public Task<T> CreateAndSaveAsync(T entity);

    /// <summary>
    ///     Delete an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    public void Delete(T entity);

    /// <summary>
    ///     Delete an entity
    /// </summary>
    /// <param name="expression">The predicate to filter entities</param>
    public void Delete(Expression<Func<T, bool>> expression);

    /// <summary>
    ///     Update an entity.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public void Update(T entity);

    /// <summary>
    ///     Update an entity and save it directly.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public T UpdateAndSave(T entity);

    /// <summary>
    ///     Update an entity and save it directly asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public Task<T> UpdateAndSaveAsync(T entity);

    /// <summary>
    ///     Update a range of entities.
    /// </summary>
    /// <param name="entities">The entities to update</param>
    public void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    ///     Get all entities.
    /// </summary>
    /// <returns>Queryable of entities</returns>
    public IQueryable<T> Get();

    /// <summary>
    ///     Get entities based on a predicate.
    /// </summary>
    /// <param name="expression">The predicate to filter entities</param>
    /// <returns>Queryable of entities</returns>
    public IQueryable<T> Get(Expression<Func<T, bool>> expression);

    /// <summary>
    ///     Get entities based on a dynamic query.
    /// </summary>
    /// <param name="predicate">The dynamic query to filter entities. Use @0, @1, etc. for arguments</param>
    /// <param name="args">The arguments for the dynamic query</param>
    /// <returns>Queryable of entities</returns>
    IQueryable<T> DynamicQuery(string predicate, params object?[] args);

    /// <summary>
    ///     Save changes to the database.
    /// </summary>
    public void Save();

    /// <summary>
    ///     Save changes to the database asynchronously.
    /// </summary>
    public Task SaveAsync();

    /// <summary>
    ///     Get an entity by its id.
    /// </summary>
    /// <param name="id">The primary key of the entity.</param>
    /// <returns>The entity</returns>
    T? GetById(int? id);

    /// <summary>
    ///     Get an entity by its id asynchronously.
    /// </summary>
    /// <param name="id">The primary key of the entity.</param>
    /// <returns>The entity</returns>
    T? GetById(Guid? id);

    /// <summary>
    ///     Get an entity by its id asynchronously.
    /// </summary>
    /// <param name="id">The primary key of the entity.</param>
    /// <returns>The entity</returns>
    Task<T?> GetByIdAsync(int? id);

    /// <summary>
    ///     Get an entity by its id asynchronously.
    /// </summary>
    /// <param name="id">The primary key of the entity.</param>
    /// <returns>The entity</returns>
    Task<T?> GetByIdAsync(Guid? id);

    /// <summary>
    ///     Get the count of entities based on a predicate.
    /// </summary>
    /// <param name="expression">The predicate to filter entities</param>
    /// <returns>The count of entities</returns>
    int Count(Expression<Func<T, bool>> expression);

    /// <summary>
    ///     Get the count of entities based on a predicate asynchronously.
    /// </summary>
    /// <param name="expression">The predicate to filter entities</param>
    /// <returns>The count of entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> expression);
}

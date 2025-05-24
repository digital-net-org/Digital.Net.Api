using System.Collections;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Digital.Net.Api.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Repositories;

public class Repository<T, TContext>(TContext context) : IRepository<T, TContext>
    where T : Entity
    where TContext : DbContext
{
    public void Reload(T entity) => context.Entry(entity).Reload();

    public async Task ReloadAsync(T entity) => await context.Entry(entity).ReloadAsync();

    public void Create(T entity) => context.Set<T>().Add(entity);

    public async Task CreateAsync(T entity) => await context.Set<T>().AddAsync(entity);

    public T CreateAndSave(T entity)
    {
        context.Set<T>().Add(entity);
        Save();
        return entity;
    }

    public async Task<T> CreateAndSaveAsync(T entity)
    {
        context.Set<T>().Add(entity);
        await SaveAsync();
        return entity;
    }

    public void Update(T entity)
    {
        UpdateNestedEntities(entity);
        context.Set<T>().Update(entity);
    }

    public T UpdateAndSave(T entity)
    {
        UpdateNestedEntities(entity);
        context.Set<T>().Update(entity);
        Save();
        return entity;
    }

    public async Task<T> UpdateAndSaveAsync(T entity)
    {
        UpdateNestedEntities(entity);
        context.Set<T>().Update(entity);
        await SaveAsync();
        return entity;
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        var enumerable = entities as T[] ?? entities.ToArray();
        foreach (var entity in enumerable)
            UpdateNestedEntities(entity);
        context.Set<T>().UpdateRange(enumerable);
    }

    public void Delete(T entity) => context.Set<T>().Remove(entity);

    public void Delete(Expression<Func<T, bool>> expression)
    {
        var entities = context.Set<T>().Where(expression);
        foreach (var entity in entities)
            context.Set<T>().Remove(entity);
    }

    public IQueryable<T> Get() => context.Set<T>().AsQueryable();
    public IQueryable<T> Get(Expression<Func<T, bool>> expression) => context.Set<T>().Where(expression);

    public IQueryable<T> DynamicQuery(string predicate, params object?[] args) =>
        context.Set<T>().Where(predicate, args);

    public T? GetById(int? id) => context.Set<T>().Find(id);

    public T? GetById(Guid? id) => context.Set<T>().Find(id);

    public async Task<T?> GetByIdAsync(int? id) => await context.Set<T>().FindAsync(id);

    public async Task<T?> GetByIdAsync(Guid? id) => await context.Set<T>().FindAsync(id);

    public int Count(Expression<Func<T, bool>> expression) => context.Set<T>().Count(expression);

    public Task<int> CountAsync(Expression<Func<T, bool>> expression) =>
        context.Set<T>().CountAsync(expression);

    public async Task SaveAsync()
    {
        AddTimestamps();
        await context.SaveChangesAsync();
    }

    public void Save()
    {
        AddTimestamps();
        context.SaveChanges();
    }

    private void UpdateNestedEntities(T entity)
    {
        var properties = entity.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (
                !property.PropertyType.IsGenericType
                || property.PropertyType.GetGenericTypeDefinition() != typeof(List<>)
            )
                continue;
            var values = property.GetValue(entity);

            if (values is null)
                continue;

            foreach (var item in (IEnumerable)values)
                context.Entry(item).State = EntityState.Modified;
        }
    }

    private void AddTimestamps()
    {
        var now = DateTime.UtcNow;
        var entities = context
            .ChangeTracker.Entries()
            .Where(x => x is { Entity: Entity, State: EntityState.Added or EntityState.Modified });
        foreach (var entity in entities)
        {
            var property = entity.State is EntityState.Added ? "CreatedAt" : "UpdatedAt";
            entity.Property(property).CurrentValue = now;
        }
    }
}

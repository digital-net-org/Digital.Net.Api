using System.Collections;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Entities.Repositories;

public class Repository<T>(DigitalContext context) : IRepository<T>
    where T : Entity
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

    public void Update(T entity) => context.Set<T>().Update(entity);

    public T UpdateAndSave(T entity)
    {
        context.Set<T>().Update(entity);
        Save();
        return entity;
    }

    public async Task<T> UpdateAndSaveAsync(T entity)
    {
        context.Set<T>().Update(entity);
        await SaveAsync();
        return entity;
    }

    public void UpdateRange(IEnumerable<T> entities) => context.Set<T>().UpdateRange(entities);

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

    public T? GetById(Guid id) => context.Set<T>().Find(id);

    public async Task<T?> GetByIdAsync(Guid id) => await context.Set<T>().FindAsync(id);

    public int Count(Expression<Func<T, bool>> expression) => context.Set<T>().Count(expression);

    public Task<int> CountAsync(Expression<Func<T, bool>> expression) =>
        context.Set<T>().CountAsync(expression);

    public async Task SaveAsync() => await context.SaveChangesAsync();

    public void Save() => context.SaveChanges();
}

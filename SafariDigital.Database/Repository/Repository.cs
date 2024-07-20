using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Database.Context;
using SafariDigital.Database.Models.Entity;

namespace SafariDigital.Database.Repository;

public class Repository<T>(SafariDigitalContext context) : IRepository<T>
    where T : class
{
    public void Create(T entity) => context.Set<T>().Add(entity);

    public async Task CreateAsync(T entity) => await context.Set<T>().AddAsync(entity);

    public void Update(T entity) => context.Set<T>().Update(entity);

    public void Delete(T entity) => context.Set<T>().Remove(entity);

    public IQueryable<T> Get(Expression<Func<T, bool>> expression) => context.Set<T>().Where(expression);

    public T? GetByPrimaryKey(params object?[]? id) => context.Set<T>().Find(id);

    public async Task<T?> GetByPrimaryKeyAsync(params object?[]? id) => await context.Set<T>().FindAsync(id);

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

    private void AddTimestamps()
    {
        var now = DateTime.UtcNow;
        var entities = context
            .ChangeTracker.Entries()
            .Where(x =>
                x is { Entity: EntityWithGuid, State: EntityState.Added or EntityState.Modified }
            );

        foreach (var entity in entities)
        {
            var property = entity.State is EntityState.Added ? "CreatedAt" : "UpdatedAt";
            entity.Property(property).CurrentValue = now;
        }
    }
}